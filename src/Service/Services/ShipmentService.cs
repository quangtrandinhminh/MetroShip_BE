using MetroShip.Repository.Base;
using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Shipment;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Mapper;
using MetroShip.Utility.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Linq.Expressions;
using MetroShip.Repository.Interfaces;
using MetroShip.Service.ApiModels;
using MetroShip.Service.BusinessModels;
using MetroShip.Utility.Enums;
using Microsoft.AspNetCore.Http;
using MetroShip.Service.Utils;
using MetroShip.Service.Validations;
using MetroShip.Utility.Config;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Exceptions;
using Microsoft.EntityFrameworkCore;
using MetroShip.Service.ApiModels.Graph;
using MetroShip.Service.ApiModels.Pricing;
using Microsoft.Extensions.Caching.Memory;

namespace MetroShip.Service.Services;

public class ShipmentService(IServiceProvider serviceProvider) : IShipmentService
{
    private readonly IUnitOfWork _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
    private readonly IMapperlyMapper _mapperlyMapper = serviceProvider.GetRequiredService<IMapperlyMapper>();
    private readonly IShipmentRepository _shipmentRepository = serviceProvider.GetRequiredService<IShipmentRepository>();
    private readonly IShipmentItineraryRepository _shipmentItineraryRepository = serviceProvider.GetRequiredService<IShipmentItineraryRepository>();
    private readonly IParcelCategoryRepository _parcelCategoryRepository = serviceProvider.GetRequiredService<IParcelCategoryRepository>();
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
    private readonly IHttpContextAccessor _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    private readonly IStationRepository _stationRepository = serviceProvider.GetRequiredService<IStationRepository>();
    private readonly ISystemConfigRepository _systemConfigRepository = serviceProvider.GetRequiredService<ISystemConfigRepository>();
    private readonly IEmailService _emailSender = serviceProvider.GetRequiredService<IEmailService>();
    private readonly IUserRepository _userRepository = serviceProvider.GetRequiredService<IUserRepository>();
    private readonly IBaseRepository<MetroTimeSlot> _metroTimeSlotRepository = serviceProvider.GetRequiredService<IBaseRepository<MetroTimeSlot>>();
    private readonly IPricingService _pricingService = serviceProvider.GetRequiredService<IPricingService>();
    private readonly IRouteRepository _routeRepository = serviceProvider.GetRequiredService<IRouteRepository>();
    private readonly IMemoryCache memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
    private MetroGraph _metroGraph;
    private const string CACHE_KEY = nameof(MetroGraph);
    private const int CACHE_EXPIRY_MINUTES = 30;

    public async Task<PaginatedListResponse<ShipmentListResponse>> GetAllShipments(
        PaginatedListRequest request
        , ShipmentFilterRequest? filterRequest = null, OrderByRequest? orderByRequest = null
        )
    {
        _logger.Information("Get all shipments with request: {@request}", request);
        // Build filter expression based on request
        Expression<Func<Shipment, bool>> filterExpression = BuildShipmentFilterExpression(filterRequest);
        // Build order by expression based on request
        Expression<Func<Shipment, object>>? orderByExpression = BuildShipmentOrderByExpression(orderByRequest,
            out bool? IsDesc);

        var shipments = await _shipmentRepository.GetPaginatedListForListResponseAsync(
            request.PageNumber, request.PageSize,
            filterExpression, orderByExpression, IsDesc
            );

        var shipmentListResponse = _mapperlyMapper.MapToShipmentListResponsePaginatedList(shipments);

        return shipmentListResponse;
    }

    public async Task<ShipmentDetailsResponse?> GetShipmentByTrackingCode(string trackingCode)
    {
        _logger.Information("Get shipment by tracking code: {@trackingCode}", trackingCode);
        var shipment = await _shipmentRepository.GetShipmentByTrackingCodeAsync(trackingCode);
        if (shipment is null)
        {
            return null;
            /*throw new AppException(
            ErrorCode.BadRequest,
            ResponseMessageShipment.SHIPMENT_NOT_FOUND,
            StatusCodes.Status400BadRequest);*/
        }

        var shipmentResponse = _mapperlyMapper.MapToShipmentDetailsResponse(shipment);
        return shipmentResponse;
    }

    public async Task<PaginatedListResponse<ShipmentListResponse>> GetShipmentsHistory(PaginatedListRequest request,
        ShipmentStatusEnum? status)
    {
        var customerId = JwtClaimUltils.GetUserId(_httpContextAccessor);
        _logger.Information("Get shipments history with request: {@request} for {@cus}", request, customerId);
        Expression<Func<Shipment, bool>> expression = x => x.DeletedAt == null && x.SenderId == customerId;
        if (status != null)
        {
            expression = expression.And(x => x.ShipmentStatus == status);
        }

        // var shipments = await _shipmentRepository.GetAllPaginatedQueryable(
        //                    request.PageNumber, request.PageSize, expression);
        //
        // var shipmentListResponse = _mapperlyMapper.MapToShipmentListResponsePaginatedList(shipments);

        var shipments = await _shipmentRepository.GetPaginatedListForListResponseAsync(
                       request.PageNumber, request.PageSize, expression);

        var shipmentListResponse = _mapperlyMapper.MapToShipmentListResponsePaginatedList(shipments);
        return shipmentListResponse;
    }

    public async Task<(string, string)> BookShipment(ShipmentRequest request, CancellationToken cancellationToken = default)
    {
        var customerId = JwtClaimUltils.GetUserId(_httpContextAccessor);
        _logger.Information("Book shipment with request: {@request}", request);

        // validate shipment request, min 48h max 14 days in advance
        CheckShipmentDate(request.ScheduledDateTime);
        ShipmentValidator.ValidateShipmentRequest(request);

        // get departure station, which accepts the shipment
        var stations = await _stationRepository.GetAll().
            Where(x => (x.Id == request.DepartureStationId || x.Id == request.DestinationStationId)
            && x.IsActive && x.DeletedAt == null)
            .Include(x => x.Region)
            .Select(x => new Station
            {
                Id = x.Id,
                StationNameVi = x.StationNameVi,
                Address = x.Address,
                Region = new Region
                {
                    RegionCode = x.Region.RegionCode
                }
            }).ToListAsync(cancellationToken);

        var departureStation = stations?.FirstOrDefault(x => x.Id == request.DepartureStationId);
        var destinationStation = stations?.FirstOrDefault(x => x.Id == request.DestinationStationId);

        if (departureStation == null)
        {
            throw new AppException(
                ErrorCode.BadRequest,
                ResponseMessageShipment.DEPARTURE_STATION_NOT_FOUND,
                StatusCodes.Status400BadRequest);
        }
            

        if (destinationStation == null)
        {
            throw new AppException(
                ErrorCode.BadRequest,
                ResponseMessageShipment.DESTINATION_STATION_NOT_FOUND,
                StatusCodes.Status400BadRequest);
        }    

        // Check if all routes exist
        var routeIds = request.ShipmentItineraries.Select(x => x.RouteId).Distinct().ToList();
        var routes = await _routeRepository.IsExistAsync(
                       x => routeIds.Contains(x.Id) && x.DeletedAt == null);
        if (!routes)
        {
            throw new AppException(
            ErrorCode.NotFound,
            "There is a route not found",
            StatusCodes.Status404NotFound);
        }

        // map shipment request to shipment entity
        var shipment = _mapperlyMapper.MapToShipmentEntity(request);
        shipment.SenderId = customerId;

        // set schedule for the first itinerary same as scheduled date time
        var firstItinerary = shipment.ShipmentItineraries
            .OrderBy(x => x.LegOrder)
            .First();
        var timeSlot = await _metroTimeSlotRepository.GetSingleAsync(
            t => t.Id == request.TimeSlotId);
        if (timeSlot == null)
        {
            throw new AppException(
            ErrorCode.NotFound,
            ResponseMessageShipment.TIME_SLOT_NOT_FOUND,
            StatusCodes.Status404NotFound);
        };
        shipment.ScheduledShift = timeSlot.Shift;
        firstItinerary.TimeSlotId = shipment.TimeSlotId;
        firstItinerary.Date = shipment.ScheduledDateTime.Value;

        // generate shipment tracking code
        var systemTime = request.ScheduledDateTime.UtcToSystemTime();
        shipment.TrackingCode = TrackingCodeGenerator.GenerateShipmentTrackingCode(
            departureStation.Region.RegionCode, systemTime);

        // foreach parcel, set shipment id and generate parcel code
        int index = 0;
        foreach (var parcel in shipment.Parcels)
        {
            parcel.ParcelCode = TrackingCodeGenerator.GenerateParcelCode(
                               shipment.TrackingCode, index);

            shipment.TotalVolumeM3 += parcel.VolumeCm3 /1000000;
            shipment.TotalWeightKg += parcel.WeightKg;

            if (!await _parcelCategoryRepository.IsExistAsync(x => x.Id == parcel.ParcelCategoryId))
            {
                throw new AppException(
                ErrorCode.NotFound,
                ResponseMessageConstantsParcelCategory.NOT_FOUND,
                StatusCodes.Status404NotFound);
            }
            index++;
        }

        var pricingTable = await _pricingService.GetPricingTableAsync(null);
        shipment.PricingConfigId = pricingTable.Id;
        shipment.PriceStructureDescriptionJSON = pricingTable.ToJsonString();
        shipment.PaymentDealine = shipment.BookedAt.Value.AddMinutes(15);
        shipment = await _shipmentRepository.AddAsync(shipment, cancellationToken);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);

        // Check if all itineraries have been scheduled
        await CheckAvailableTimeSlotsAsync(shipment.Id, 3);

        // send email to customer
        _logger.Information("Send email to customer with tracking code: {@trackingCode}", 
            shipment.TrackingCode);
        var user = await _userRepository.GetUserByIdAsync(customerId);
        shipment.ScheduledDateTime = systemTime;
        shipment.DepartureStationName = departureStation.StationNameVi;
        shipment.DepartureStationAddress = departureStation.Address;
        shipment.DestinationStationName = destinationStation.StationNameVi;
        shipment.DestinationStationAddress = destinationStation.Address;
        var sendMailModel = new SendMailModel
        {
            Email = user.Email,
            Type = MailTypeEnum.Shipment,
            Name = request.SenderName,
            Data = shipment,
            Message = request.TrackingLink
        };
        _emailSender.SendMail(sendMailModel);

        // send email to recipient if provided
        if (!string.IsNullOrEmpty(request.RecipientEmail) && request.RecipientEmail != user.Email)
        {
            // send email to recipient
            _logger.Information("Send email to recipient with tracking code: {@trackingCode}", shipment.TrackingCode);
            var recipientSendMailModel = new SendMailModel
            {
                Email = request.RecipientEmail,
                Type = MailTypeEnum.Shipment,
                Name = request.RecipientName,
                Data = shipment,
                Message = request.TrackingLink
            };
            _emailSender.SendMail(recipientSendMailModel);
        }

        return (shipment.Id, shipment.TrackingCode);
    }

    private async Task InitializeGraphAsync()
    {
        var cacheKey = CACHE_KEY;
        if (memoryCache.TryGetValue(cacheKey, out MetroGraph? cachedGraph))
        {
            _logger.Information("Retrieved metro graph from cache.");
            _metroGraph = cachedGraph;
            return;
        }

        var (routes, stations, metroLines) =
            await _shipmentItineraryRepository.GetRoutesAndStationsAsync();

        // Khởi tạo đồ thị metro
        _metroGraph = new MetroGraph(routes, stations, metroLines);
        // save into memory cache
        memoryCache.Set(cacheKey, _metroGraph, TimeSpan.FromMinutes(CACHE_EXPIRY_MINUTES));
    }

    /*private async Task InitializePricingTableAsync()
    {
        if (_isInitializedPricingTable)
            return;

        _pricingTable = await _pricingService.GetPricingTableAsync(null);
        _isInitializedPricingTable = true;
    }*/

    // v2
    public async Task<TotalPriceResponse> GetItineraryAndTotalPrice(TotalPriceCalcRequest request)
    {
        _logger.Information("Get itinerary and total price with request: {@request}", request);

        // Validation
        ShipmentValidator.ValidateTotalPriceCalcRequest(request);

        // Get station options
        var stationIds = await GetNearUserStations(request);

        // Find paths
        var pathResults = await FindOptimalPaths(stationIds.ToList(), request.DestinationStationId);

        // Calculate pricing
        var bestPathResponses = await CalculatePricingForPaths(pathResults, request);

        // Build response
        return BuildTotalPriceResponse(bestPathResponses, stationIds.ToList());
    }

    public async Task PickUpShipment (ShipmentPickUpRequest request)
    {
        _logger.Information("Confirm shipment with ID: {@shipmentId}", request.ShipmentId);
        var shipment = await _shipmentRepository.GetSingleAsync(
                       x => x.Id == request.ShipmentId, false,
                       x => x.Parcels, x => x.ShipmentItineraries);

        // Check if the shipment exists
        if (shipment == null)
        {
            throw new AppException(
            ErrorCode.NotFound,
            ResponseMessageShipment.SHIPMENT_NOT_FOUND,
            StatusCodes.Status404NotFound);
        }

        // Check if the shipment is already confirmed
        if (shipment.ShipmentStatus != ShipmentStatusEnum.AwaitingDropOff)
        {
            throw new AppException(
            ErrorCode.BadRequest,
            ResponseMessageShipment.SHIPMENT_ALREADY_CONFIRMED,
            StatusCodes.Status400BadRequest);
        }

        // Check if all itineraries have been scheduled
        if (shipment.ShipmentItineraries.Any(si => si.Date == null || si.TimeSlotId == null))
        {
            throw new AppException(
            ErrorCode.BadRequest,
            ResponseMessageShipment.SHIPMENT_ITINERARY_NOT_SCHEDULED,
            StatusCodes.Status400BadRequest);
        }

        // Update shipment status and timestamps
        shipment.ShipmentStatus = ShipmentStatusEnum.PickedUp;
        shipment.PickedUpBy = JwtClaimUltils.GetUserId(_httpContextAccessor);
        shipment.PickedUpAt = CoreHelper.SystemTimeNow;
        foreach (var media in request.PickedUpMedias)
        {
            var mediaEntity = _mapperlyMapper.MapToShipmentMediaEntity(media);
            mediaEntity.ShipmentId = shipment.Id;
            mediaEntity.BusinessMediaType = BusinessMediaTypeEnum.Pickup;
            mediaEntity.MediaType = DataHelper.IsImage(mediaEntity.MediaUrl);
            shipment.ShipmentMedias.Add(mediaEntity);
        }

        // Save changes to the database
        _shipmentRepository.Update(shipment);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);

        // Optionally send confirmation email to sender
        var user = await _userRepository.GetUserByIdAsync(shipment.SenderId);
        if (user != null)
        {
            var sendMailModel = new SendMailModel
            {
                Email = user.Email,
                Type = MailTypeEnum.Notification,
                Message = $"Your shipment with tracking code {shipment.TrackingCode} has been accepted.",
            };
            _emailSender.SendMail(sendMailModel);
        }
    }

    public async Task RejectShipment(ShipmentRejectRequest request)
    {
        _logger.Information("Reject shipment with ID: {@shipmentId}", request.ShipmentId);
        var shipment = await _shipmentRepository.GetSingleAsync(
                x => x.Id == request.ShipmentId, false,
                x => x.Parcels, x => x.ShipmentItineraries);

        // Check if the shipment exists
        if (shipment == null)
        {
            throw new AppException(
            ErrorCode.NotFound,
            ResponseMessageShipment.SHIPMENT_NOT_FOUND,
            StatusCodes.Status404NotFound);
        }

        // Check if the shipment is awaiting drop-off
        if (shipment.ShipmentStatus != ShipmentStatusEnum.AwaitingDropOff)
        {
            throw new AppException(
            ErrorCode.BadRequest,
            ResponseMessageShipment.SHIPMENT_ALREADY_CONFIRMED,
            StatusCodes.Status400BadRequest);
        }

        // Update shipment status and timestamps
        shipment.ShipmentStatus = ShipmentStatusEnum.Rejected;
        shipment.RejectedBy = JwtClaimUltils.GetUserId(_httpContextAccessor);
        shipment.RejectionReason = request.Reason;
        shipment.RejectedAt = CoreHelper.SystemTimeNow;

        // Save changes to the database
        _shipmentRepository.Update(shipment);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);

        // Optionally send rejection email to sender
        var user = await _userRepository.GetUserByIdAsync(shipment.SenderId);
        if (user != null)
        {
            var sendMailModel = new SendMailModel
            {
                Email = user.Email,
                Type = MailTypeEnum.Notification,
                Message = $"Your shipment with tracking code {shipment.TrackingCode} has been rejected. " +
                $"Reason: {request.Reason}",
            };
            _emailSender.SendMail(sendMailModel);
        }
    }

    public async Task CancelShipment(ShipmentRejectRequest request)
    {
        _logger.Information("Reject shipment with ID: {@shipmentId}", request.ShipmentId);
        var shipment = await _shipmentRepository.GetSingleAsync(
            x => x.Id == request.ShipmentId, false,
            x => x.Parcels, x => x.ShipmentItineraries);

        // Check if the shipment exists
        if (shipment == null)
        {
            throw new AppException(
                ErrorCode.NotFound,
                ResponseMessageShipment.SHIPMENT_NOT_FOUND,
                StatusCodes.Status404NotFound);
        }

        // Check if the shipment is AwaitingPayment or AwaitingDropOff
        if (shipment.ShipmentStatus != ShipmentStatusEnum.AwaitingDropOff
            || shipment.ShipmentStatus != ShipmentStatusEnum.AwaitingPayment
            )
        {
            throw new AppException(
                ErrorCode.BadRequest,
                ResponseMessageShipment.SHIPMENT_CANNOT_CANCEL,
                StatusCodes.Status400BadRequest);
        }

        shipment.RejectedBy = JwtClaimUltils.GetUserId(_httpContextAccessor);
        shipment.RejectionReason = request.Reason;
        shipment.CancelledAt = CoreHelper.SystemTimeNow;

        // Check if the shipment can be cancelled before a certain time
        var allowedCancelBefore = _systemConfigRepository
            .GetSystemConfigValueByKey(nameof(SystemConfigSetting.Instance.ALLOW_CANCEL_BEFORE_HOUR));
        if ((shipment.ScheduledDateTime - shipment.CancelledAt) >
            TimeSpan.FromHours(int.Parse(allowedCancelBefore)))
        {
            shipment.ShipmentStatus = ShipmentStatusEnum.AwaitingRefund;

            var refundPercent = _systemConfigRepository
                .GetSystemConfigValueByKey(nameof(SystemConfigSetting.Instance.REFUND_PERCENT));
            // create transaction for refund
            var transaction = new Transaction
            {
                ShipmentId = shipment.Id,
                PaymentAmount = shipment.TotalCostVnd *
                                decimal.Parse(refundPercent),
                TransactionType = TransactionTypeEnum.Refund,
                PaymentStatus = PaymentStatusEnum.Pending
            };

            // Add transaction to shipment
            shipment.Transactions.Add(transaction);
        }

        // Update shipment status and timestamps
        shipment.ShipmentStatus = ShipmentStatusEnum.Cancelled;
        

        // Save changes to the database
        _shipmentRepository.Update(shipment);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);

        // Optionally send cancellation email to sender
        var user = await _userRepository.GetUserByIdAsync(shipment.SenderId);
        if (user != null)
        {
            var sendMailModel = new SendMailModel
            {
                Email = user.Email,
                Type = MailTypeEnum.Notification,
                Message = $"Your shipment with tracking code {shipment.TrackingCode} has been cancelled. " +
                          $"Reason: {request.Reason}",
            };
            _emailSender.SendMail(sendMailModel);
        }
    }

    private void CheckShipmentDate(DateTimeOffset scheduledDateTime)
    {
        _logger.Information("Checking shipment date: {@scheduledDateTime}", scheduledDateTime);
        // Get system config values
        var confirmationHour = int.Parse(_systemConfigRepository
            .GetSystemConfigValueByKey(nameof(SystemConfigSetting.Instance.CONFIRMATION_HOUR)));
        var paymentRequestHour = int.Parse(_systemConfigRepository
            .GetSystemConfigValueByKey(nameof(SystemConfigSetting.Instance.PAYMENT_REQUEST_HOUR)));
        var maxScheduleDay = int.Parse(_systemConfigRepository
            .GetSystemConfigValueByKey(nameof(SystemConfigSetting.Instance.MAX_SCHEDULE_SHIPMENT_DAY)));
        //var minBookDate = CoreHelper.SystemTimeNow.AddHours(confirmationHour + paymentRequestHour);
        var minBookDate = CoreHelper.SystemTimeNow;
        var maxBookDate = CoreHelper.SystemTimeNow.AddDays(maxScheduleDay);

        if (scheduledDateTime < minBookDate || scheduledDateTime > maxBookDate)
        {
            throw new AppException(
                ErrorCode.BadRequest,
                $"The ScheduledDateTime must be between {minBookDate} and {maxBookDate}.",
                StatusCodes.Status400BadRequest);
        }
    }

    private Expression<Func<Shipment, bool>> BuildShipmentFilterExpression(ShipmentFilterRequest? request)
    {
        _logger.Information("Building filter expression for request: {@request}", request);
        ShipmentValidator.ValidateShipmentFilterRequest(request);

        // Start with base filter for non-deleted items
        Expression<Func<Shipment, bool>> expression = x => x.DeletedAt == null;

        // Return early if no filter request
        if (request == null) return expression;

        // Apply filters only if values are provided
        if (!string.IsNullOrWhiteSpace(request.TrackingCode))
        {
            expression = expression.And(x => x.TrackingCode.Contains(request.TrackingCode));
        }

        if (!string.IsNullOrWhiteSpace(request.DepartureStationId))
        {
            expression = expression.And(x => x.DepartureStationId == request.DepartureStationId);
        }

        if (!string.IsNullOrWhiteSpace(request.DestinationStationId))
        {
            expression = expression.And(x => x.DestinationStationId == request.DestinationStationId);
        }

        if (!string.IsNullOrWhiteSpace(request.SenderName))
        {
            expression = expression.And(x => x.SenderName.Contains(request.SenderName));
        }

        if (!string.IsNullOrWhiteSpace(request.SenderPhone))
        {
            expression = expression.And(x => x.SenderPhone.Contains(request.SenderPhone));
        }

        if (!string.IsNullOrWhiteSpace(request.RecipientName))
        {
            expression = expression.And(x => x.RecipientName.Contains(request.RecipientName));
        }

        if (!string.IsNullOrWhiteSpace(request.RecipientPhone))
        {
            expression = expression.And(x => x.RecipientPhone.Contains(request.RecipientPhone));
        }

        if (!string.IsNullOrWhiteSpace(request.RecipientEmail))
        {
            expression = expression.And(x => x.RecipientEmail != null &&
                                            x.RecipientEmail.Contains(request.RecipientEmail));
        }

        if (request.ShipmentStatus.HasValue)
        {
            expression = expression.And(x => x.ShipmentStatus == request.ShipmentStatus.Value);
        }

        if (request.FromScheduleDateTime.HasValue)
        {
            expression = expression.And(x => x.ScheduledDateTime >= request.FromScheduleDateTime.Value);
        }

        if (request.ToScheduleDateTime.HasValue)
        {
            expression = expression.And(x => x.ScheduledDateTime <= request.ToScheduleDateTime.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.RegionCode))
        {
            expression = expression.And(x => x.TrackingCode.Contains(request.RegionCode.ToUpperInvariant()));
        }

        if (!string.IsNullOrWhiteSpace(request.LineId))
        {
            expression = expression.And(x => x.ShipmentItineraries
                .Any(si => si.LegOrder == 1 && si.TimeSlotId == request.TimeSlotId));
        }

        if (!string.IsNullOrWhiteSpace(request.LineId))
        {
            expression = expression.And(x => x.ShipmentItineraries
                .Any(si => si.Route.LineId == request.LineId));
        }

        if (!string.IsNullOrWhiteSpace(request.ItineraryIncludeStationId))
        {
            expression = expression.And(x => x.ShipmentItineraries
                .Any(si => si.Route.FromStationId == request.ItineraryIncludeStationId ||
                          si.Route.ToStationId == request.ItineraryIncludeStationId));
        }

        return expression;
    }

    private Expression<Func<Shipment, object>>? BuildShipmentOrderByExpression(OrderByRequest? request, out bool? isDesc)
    {
        _logger.Information("Building order by expression for orderBy: {@orderBy}, isDesc: {@isDesc}",
            request?.OrderBy, request?.IsDesc);

        // Default to ScheduledDateTime if no orderBy specified
        if (string.IsNullOrWhiteSpace(request?.OrderBy))
        {
            isDesc = null;
            return null;
        }

        if (!OrderByMapping.TryGetValue(request.OrderBy, out var orderByExpression))
        {
            // If the key doesn't exist, throw your custom exception
            throw new AppException(
                ErrorCode.BadRequest,
                $"OrderBy '{request.OrderBy}' is not a supported field.",
                StatusCodes.Status400BadRequest);
        }

        // If descending, wrap in a descending expression
        if (request is { IsDesc: not null } && request.IsDesc.Value)
        {
            isDesc = request.IsDesc.Value;
        }

        isDesc = null;
        return orderByExpression;
    }

    private static readonly Dictionary<string, Expression<Func<Shipment, object>>> OrderByMapping =
        new(StringComparer.OrdinalIgnoreCase)
    {
        { nameof(Shipment.ScheduledDateTime), x => x.ScheduledDateTime },
        { nameof(Shipment.BookedAt), x => x.BookedAt },
        { nameof(Shipment.ApprovedAt), x => x.ApprovedAt },
        { nameof(Shipment.PaidAt), x => x.PaidAt },
        { nameof(Shipment.PickedUpAt), x => x.PickedUpAt },
        { nameof(Shipment.DeliveredAt), x => x.DeliveredAt },
        { nameof(Shipment.SurchargeAppliedAt), x => x.SurchargeAppliedAt },
        { nameof(Shipment.CancelledAt), x => x.CancelledAt },
        { nameof(Shipment.RefundedAt), x => x.RefundedAt },
        { nameof(Shipment.RejectedAt), x => x.RejectedAt },
        // Add any other supported fields here
    };

    private async Task<List<string>> GetNearUserStations(TotalPriceCalcRequest request)
    {
        var maxDistanceInMeters = int.Parse(_systemConfigRepository
            .GetSystemConfigValueByKey(nameof(SystemConfigSetting.Instance.MAX_DISTANCE_IN_METERS))); //2000 meters
        var maxStationCount = int.Parse(_systemConfigRepository
            .GetSystemConfigValueByKey(nameof(SystemConfigSetting.Instance.MAX_COUNT_STATION_NEAR_USER))); //5

        var result = new List<string> { request.DepartureStationId };

        if (request is { UserLongitude: not null, UserLatitude: not null })
        {
            var nearStations = await _stationRepository.GetAllStationIdNearUser(
                request.UserLatitude.Value, request.UserLongitude.Value, maxDistanceInMeters, maxStationCount);

            // Remove departure station if present (to avoid duplication)
            nearStations.Remove(request.DepartureStationId);

            // Ensure at least 2 stations are available (including departure)
            while (result.Count + nearStations.Count < 2 && maxDistanceInMeters < maxDistanceInMeters * 2)
            {
                // Extend distance by 1000 meters
                maxDistanceInMeters += 2000;
                nearStations = await _stationRepository.GetAllStationIdNearUser(
                    request.UserLatitude.Value, request.UserLongitude.Value, maxDistanceInMeters, maxStationCount);
                nearStations.Remove(request.DepartureStationId);
            }

            // Add up to (maxStationCount - 1) nearest stations (excluding departure) -- max 6
            result.AddRange(nearStations.Take(maxStationCount));
        }

        return result;
    }

    private async Task<List<(string StationId, List<string> Path)>> FindOptimalPaths(
        List<string> stationIdList, string destinationStationId)
    {
        InitializeGraphAsync().Wait();

        var pathTasks = stationIdList.Select(async departureStationId => {
            List<string> path = _stationRepository.AreStationsInSameMetroLine(
                departureStationId, destinationStationId)
                ? _metroGraph.FindShortestPathByBFS(departureStationId, destinationStationId)
                : _metroGraph.FindShortestPathByDijkstra(departureStationId, destinationStationId);
            return (StationId: departureStationId, Path: path);
        }).ToList();

        var allPaths = await Task.WhenAll(pathTasks);

        // Filter out null/empty paths and log them
        // Valid path: path has more than 1 vertex (path is a station list, 1 route need 2 station)
        var validPaths = allPaths.Where(
            r => r.Path.Count > 1).ToList();

        if (!validPaths.Any())
        {
            _logger.Information("No valid paths found from any station {StationIds} to {DestinationId}",
                string.Join(", ", stationIdList), destinationStationId);
            throw new AppException(
                ErrorCode.NotFound,
                ResponseMessageShipment.PATH_NOT_FOUND,
                StatusCodes.Status404NotFound);
        }

        _logger.Information("Found {ValidPathCount} valid paths out of {TotalPathCount} attempted",
            validPaths.Count, allPaths.Length);

        return validPaths;
    }

    private async Task<List<dynamic>> CalculatePricingForPaths(
        List<(string StationId, List<string> Path)> pathResults, TotalPriceCalcRequest request)
    {
        // Get pricing configuration
        //InitializePricingTableAsync().Wait();

        // Get parcel categories
        var categoryIds = request.Parcels.Select(p => p.ParcelCategoryId).Distinct().ToList();
        var categories = await _parcelCategoryRepository.GetAllWithCondition(
            x => categoryIds.Contains(x.Id) && x.IsActive && x.DeletedAt == null)
            .ToListAsync();

        // check if all categories are exist in categoryIds and if not, throw exception at which is not found
        if (categories.Count() != categoryIds.Count)
        {
            var missingCategories = categoryIds.Except(categories.Select(c => c.Id)).ToList();
            throw new AppException(
            ErrorCode.NotFound,
            $"Parcel categories not found: {string.Join(", ", missingCategories)}",
            StatusCodes.Status404NotFound);
        }

        return pathResults.Select(r =>
        {
            var pathResponse = _metroGraph.CreateResponseFromPath(r.Path, _mapperlyMapper);
            _mapperlyMapper.CloneToParcelRequestList(request.Parcels, pathResponse.Parcels);

            // Calculate pricing for each parcel
            ParcelPriceCalculator.CalculateParcelPricing(
                pathResponse.Parcels, pathResponse, _pricingService, categories);

            // Check est arrival time
            pathResponse.EstArrivalTime = CheckEstArrivalTime(pathResponse, request.TimeSlotId, request.ScheduledDateTime).Result;

            return new
            {
                StationId = r.StationId,
                Response = pathResponse
            };
        }).ToList<dynamic>();
    }

    private TotalPriceResponse BuildTotalPriceResponse(List<dynamic> bestPathResponses, List<string> stationIdList)
    {
        var response = new TotalPriceResponse();

        response.Standard = bestPathResponses
            .FirstOrDefault(r => r.StationId == stationIdList[0])?.Response;

        response.Nearest = bestPathResponses.Count > 1
            ? bestPathResponses.FirstOrDefault(r => r.StationId == stationIdList[1])?.Response
            : null;

        response.Shortest = bestPathResponses.Count > 1
            ? bestPathResponses
                .OrderBy(r => r.Response.TotalCostVnd)
                .FirstOrDefault()?.Response
            : null;

        var maxDistanceInMeters = int.Parse(_systemConfigRepository
            .GetSystemConfigValueByKey(nameof(SystemConfigSetting.Instance.MAX_DISTANCE_IN_METERS)));
        response.StationsInDistanceMeter = maxDistanceInMeters;
        return response;
    }

    private async Task<DateTimeOffset> CheckEstArrivalTime(BestPathGraphResponse pathResponse, string currentSlotId, DateTimeOffset date)
    {
        _logger.Information("Checking estimated arrival time for path: {@PathResponse}", pathResponse);

        // for each line, get next time slot
        var nextTimeSlots = await _metroTimeSlotRepository.GetAllWithCondition(
                       x => !x.IsAbnormal && x.DeletedAt == null)
            .ToListAsync();

        if (nextTimeSlots == null || !nextTimeSlots.Any())
        {
            _logger.Warning("No available time slots found for estimation.");
            throw new AppException(
            ErrorCode.NotFound,
            ResponseMessageShipment.TIME_SLOT_NOT_FOUND,
            StatusCodes.Status404NotFound);
        }

        // Find the current time slot in the list
        var currentSlot = nextTimeSlots.FirstOrDefault(x => x.Id == currentSlotId);
        if (currentSlot == null)
        {
            _logger.Warning("Current time slot with ID {CurrentSlotId} not found in available time slots.",
                               currentSlotId);
            throw new AppException(
            ErrorCode.NotFound,
            ResponseMessageShipment.TIME_SLOT_NOT_FOUND,
            StatusCodes.Status404NotFound);
        }

        // -1 is eliminate if only 1 line, does not change to next slot
        for (var i = 0; i < pathResponse.TotalMetroLines-1; i++)
        {
            (date, currentSlot) = GetNextSlot(date, currentSlot, nextTimeSlots);
        }

        // Normalize date to start of the day, gmt +7, include if for loop not run
        var result = new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.FromHours(7));

        // + hour from 0 to close time
        var hour = currentSlot.CloseTime.ToTimeSpan().TotalHours;
        // calculate how many hour from current slot
        if (currentSlot.CloseTime.ToTimeSpan().TotalHours > currentSlot.OpenTime.ToTimeSpan().TotalHours)
        {
            // date to be 00:00 and add hour
            result = result.AddHours(hour);
        }
        else
        {
            result = result.AddDays(1).AddHours(hour);
        }

        // cus timeonly from slot is +7, convert to +0
        result = result.ToUniversalTime();
        return result;
    }

    /*public async Task<List<ShipmentAvailableTimeSlotResponse>> CheckAvailableTimeSlotsAsync
        (ShipmentAvailableTimeSlotsRequest request)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(request.ShipmentId);
        if (shipment?.Parcels == null)
        {
            _logger.Warning("Shipment or its parcels are null.");
        }

        var availableSlotsRequest = new ShipmentRepository.CheckAvailableTimeSlotsRequest
        {
            ShipmentId = request.ShipmentId,
            MaxAttempts = request.MaxAttempts > 0 ? request.MaxAttempts : 3
        };

        var availableSlots = await _shipmentRepository.FindAvailableTimeSlotsAsync(availableSlotsRequest);

        var validSlots = availableSlots
            .Where(s => s.StartDate != default && s.Date != default)
            .ToList();

        if (!validSlots.Any())
        {
            _logger.Warning("No valid time slots found for shipment {ShipmentId}", request.ShipmentId);
            return new List<ShipmentAvailableTimeSlotResponse>();
        }

        var mappedSlots = validSlots.Select(slot =>
        {
            var slotDetail = new MetroTimeSlotResponse
            {
                Id = slot.TimeSlotId,
                Shift = slot.Shift,
                DayOfWeek = slot.DayOfWeek,
                SpecialDate = slot.SpecialDate,
                OpenTime = slot.OpenTime,
                CloseTime = slot.CloseTime,
                IsAbnormal = slot.IsAbnormal,
                ScheduleBeforeShiftMinutes = 30
            };

            return (
                StartDate: slot.StartDate,
                Date: slot.Date,
                SlotDetail: slotDetail,
                RemainingVolumeM3: slot.RemainingVolumeM3,
                RemainingWeightKg: slot.RemainingWeightKg,
                ShipmentStatus: ShipmentStatusEnum.AwaitingDropOff,
                ParcelIds: slot.ParcelIds ?? new List<string>()
            );
        }).ToList();

        return _mapperlyMapper.MapToAvailableTimeSlotResponseList(mappedSlots);
    }*/

    private record CalculatedItinerary
    {
        public DateTimeOffset? Date { get; init; } = null;
        public string TimeSlotId { get; init; } = string.Empty;
        public string ShipmentId { get; init; } = string.Empty;
        public decimal? TotalWeightKg { get; init; } = null;
        public decimal? TotalVolumeM3 { get; init; } = null;
        public ShiftEnum? Shift { get; init; } = null;
        public string? LineId { get; init; } = null;
        public string? RouteId { get; init; } = null;
    }

    public async Task<List<ItineraryResponse>> CheckAvailableTimeSlotsAsync(
        string shipmentId, int maxAttempt)
    {
        _logger.Information("Checking available time slots for shipment ID: {@ShipmentId}, Max Attempts: {@MaxAttempts}",
            shipmentId, maxAttempt);

        // 1. Get shipment with itineraries and routes
        var shipment = await _shipmentRepository.GetAll()
            .Include(x => x.ShipmentItineraries.OrderBy(i => i.LegOrder))
            .ThenInclude(x => x.Route)
            .FirstOrDefaultAsync(x => x.Id == shipmentId && x.DeletedAt == null);

        if (shipment == null)
        {
            throw new AppException(
                ErrorCode.NotFound,
                ResponseMessageShipment.SHIPMENT_NOT_FOUND,
                StatusCodes.Status400BadRequest);
        }

        // 2. Get system configuration
        var maxWeightKg = decimal.Parse(_systemConfigRepository
            .GetSystemConfigValueByKey(nameof(SystemConfigSetting.Instance.MAX_CAPACITY_PER_LINE_KG)));
        var maxVolumeM3 = decimal.Parse(_systemConfigRepository
            .GetSystemConfigValueByKey(nameof(SystemConfigSetting.Instance.MAX_CAPACITY_PER_LINE_M3)));

        // 3. Get all available time slots
        var timeSlots = await _metroTimeSlotRepository.GetAllWithCondition(
                x => !x.IsAbnormal && x.DeletedAt == null)
            .OrderBy(x => x.Shift)
            .ToListAsync();

        // 4. Bulk fetch all relevant itineraries for capacity calculation
        var capacityData = await BulkFetchCapacityDataAsync(shipment, maxAttempt);

        // 5. Process slot assignment for each itinerary
        await ProcessSlotAssignmentAsync(shipment, capacityData, timeSlots, maxWeightKg, maxVolumeM3, maxAttempt);

        // 6. Save changes to database
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
        return _mapperlyMapper.MapToListShipmentItineraryResponse(shipment.ShipmentItineraries.ToList());
    }


    /// <summary>
    /// Bulk fetch all itineraries that could affect capacity calculation
    /// This is the key performance optimization
    /// </summary>
    private async Task<Dictionary<CapacityKey, List<CalculatedItinerary>>> BulkFetchCapacityDataAsync(
        Shipment shipment, int maxAttempts)
    {
        // Define valid shipment statuses for capacity calculation
        var validStatuses = new[]
        {
            ShipmentStatusEnum.AwaitingPayment,
            ShipmentStatusEnum.AwaitingDropOff,
            ShipmentStatusEnum.PickedUp,
            ShipmentStatusEnum.InTransit,
            ShipmentStatusEnum.ToReturn,
            ShipmentStatusEnum.Returning
        };

        // Calculate date range for bulk fetch
        var startDate = shipment.ScheduledDateTime.Value;
        var endDate = startDate.AddDays(4 * maxAttempts); // 4 shifts per day

        // Get all lines involved in this shipment
        var routeIds = shipment.ShipmentItineraries.Select(i => i.RouteId).Distinct().ToList();

        // Bulk fetch ALL relevant itineraries for capacity calculation
        var allRelevantItineraries = await _shipmentItineraryRepository.GetAllWithCondition(
            x => routeIds.Contains(x.RouteId) &&
                 validStatuses.Contains(x.Shipment.ShipmentStatus) &&
                 x.Date.HasValue &&
                 x.Date.Value >= startDate &&
                 x.Date.Value <= endDate &&
                 x.DeletedAt == null)
            .Include(x => x.Shipment)
            .Include(x => x.Route)
            .Include(x => x.TimeSlot)
            .Select(x => new CalculatedItinerary
            {
                Date = x.Date.Value.Date,
                TimeSlotId = x.TimeSlotId,
                ShipmentId = x.ShipmentId,
                TotalWeightKg = x.Shipment.TotalWeightKg,
                TotalVolumeM3 = x.Shipment.TotalVolumeM3,
                Shift = x.TimeSlot.Shift,
                LineId = x.Route.LineId,
                RouteId = x.RouteId
            })
            .ToListAsync();

        // Group by (RouteId, Date, Shift) for fast lookup
        return allRelevantItineraries
            .GroupBy(x => new CapacityKey(x.RouteId, x.Date.Value.Date, x.Shift.Value))
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    /// <summary>
    /// Process slot assignment for all itineraries in the shipment
    /// </summary>
    private async Task ProcessSlotAssignmentAsync(
        Shipment shipment,
        Dictionary<CapacityKey, List<CalculatedItinerary>> capacityData,
        List<MetroTimeSlot> timeSlots,
        decimal maxWeightKg,
        decimal maxVolumeM3,
        int maxAttempts)
    {
        // Initialize with shipment's scheduled date and shift
        var currentDate = shipment.ScheduledDateTime.Value;
        var currentSlot = timeSlots.FirstOrDefault(ts => ts.Shift == shipment.ScheduledShift);

        if (currentSlot == null)
        {
            throw new AppException(
                ErrorCode.NotFound,
                $"No time slot found for shift {shipment.ScheduledShift}",
                StatusCodes.Status400BadRequest);
        }

        string previousLineId = null;

        foreach (var itinerary in shipment.ShipmentItineraries.OrderBy(i => i.LegOrder))
        {
            var currentLineId = itinerary.Route.LineId;

            // If not first itinerary and different line, find next available slot
            if (itinerary.LegOrder > 1 && currentLineId != previousLineId)
            {
                (currentDate, currentSlot) = GetNextSlot(currentDate, currentSlot, timeSlots);
            }

            // Find available slot with capacity check
            var (assignedDate, assignedSlot) = await FindAvailableSlotWithCapacityAsync(
                currentDate, currentSlot, itinerary, shipment,
                capacityData, timeSlots, maxWeightKg, maxVolumeM3, maxAttempts);

            // Assign slot to itinerary
            itinerary.TimeSlotId = assignedSlot.Id;
            itinerary.Date = assignedDate;
            _shipmentItineraryRepository.Update(itinerary);

            // Update for next iteration
            currentDate = assignedDate;
            currentSlot = assignedSlot;
            previousLineId = currentLineId;

            _logger.Information("Assigned slot {SlotId} on {Date} to itinerary {ItineraryId} (Line: {LineId})",
                assignedSlot.Id, assignedDate, itinerary.Id, currentLineId);
        }
    }

    /// Find available slot with capacity check using pre-fetched data
    private async Task<(DateTimeOffset, MetroTimeSlot)> FindAvailableSlotWithCapacityAsync(
        DateTimeOffset startDate,
        MetroTimeSlot startSlot,
        ShipmentItinerary itinerary,
        Shipment shipment,
        Dictionary<CapacityKey, List<CalculatedItinerary>> capacityData,
        List<MetroTimeSlot> timeSlots,
        decimal maxWeightKg,
        decimal maxVolumeM3,
        int maxAttempts)
    {
        var currentDate = startDate;
        var currentSlot = startSlot;
        var attempts = 0;

        while (attempts < maxAttempts)
        {
            var capacityKey = new CapacityKey(itinerary.Route.LineId, currentDate, currentSlot.Shift);

            // Get existing capacity for this line/date/shift
            var existingItineraries = capacityData
                .GetValueOrDefault(capacityKey, new List<CalculatedItinerary>());

            var totalWeightKg = existingItineraries.Sum(i => i.TotalWeightKg ?? 0);
            var totalVolumeM3 = existingItineraries.Sum(i => i.TotalVolumeM3 ?? 0);

            // Check if adding this shipment exceeds capacity
            if (totalWeightKg + (shipment.TotalWeightKg ?? 0) <= maxWeightKg &&
                totalVolumeM3 + (shipment.TotalVolumeM3 ?? 0) <= maxVolumeM3)
            {
                // Add this shipment to capacity data for future iterations
                var newItinerary = new CalculatedItinerary
                {
                    Date = currentDate,
                    TimeSlotId = currentSlot.Id,
                    ShipmentId = shipment.Id,
                    TotalWeightKg = shipment.TotalWeightKg,
                    TotalVolumeM3 = shipment.TotalVolumeM3,
                    Shift = currentSlot.Shift,
                    LineId = itinerary.Route.LineId,
                    RouteId = itinerary.RouteId
                };

                if (!capacityData.ContainsKey(capacityKey))
                    capacityData[capacityKey] = new List<CalculatedItinerary>();
                capacityData[capacityKey].Add(newItinerary);

                return (currentDate, currentSlot);
            }

            // Try next slot
            (currentDate, currentSlot) = GetNextSlot(currentDate, currentSlot, timeSlots);
            attempts++;

            _logger.Debug("Attempt {Attempt}: Slot {SlotId} on {Date} has insufficient capacity for route {Route}",
                attempts, currentSlot.Id, currentDate, itinerary.Route.RouteNameEn);
        }

        _shipmentRepository.Delete(shipment);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
        throw new AppException(
            ErrorCode.BadRequest,
            $"No available slot found for itinerary on route {itinerary.Route.RouteNameEn} after {maxAttempts} attempts",
            StatusCodes.Status400BadRequest);
    }

    /// Get next slot
    private (DateTimeOffset, MetroTimeSlot) GetNextSlot(
        DateTimeOffset date,
        MetroTimeSlot currentSlot,
        List<MetroTimeSlot> timeSlots)
    {
        // Normalize date to start of the day, default to UTC+0
        date = new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.Zero);
        var nextShift = currentSlot.Shift switch
        {
            ShiftEnum.Morning => ShiftEnum.Afternoon,
            ShiftEnum.Afternoon => ShiftEnum.Evening,
            ShiftEnum.Evening => ShiftEnum.Night,
            ShiftEnum.Night => ShiftEnum.Morning,
            _ => ShiftEnum.Morning
        };

        // If current slot is Night, move to next date's Morning
        var nextDate = currentSlot.Shift == ShiftEnum.Night ? date.AddDays(1) : date;
        var nextSlot = timeSlots.FirstOrDefault(ts => ts.Shift == nextShift);

        if (nextSlot == null)
        {
            throw new AppException(
                ErrorCode.NotFound,
                $"No time slot found for shift {nextShift}",
                StatusCodes.Status500InternalServerError);
        }

        return (nextDate, nextSlot);
    }

    public record CapacityKey(string RouteId, DateTimeOffset Date, ShiftEnum Shift);

    public async Task<ShipmentLocationResponse> GetShipmentLocationAsync(string trackingCode)
    {
        var shipment = await _shipmentRepository.GetShipmentByTrackingCodeAsync(trackingCode);
        if (shipment == null)
            throw new ArgumentException($"Không tìm thấy shipment với mã tracking {trackingCode}");

        var finalLeg = shipment.ShipmentItineraries
            .OrderBy(i => i.LegOrder)
            .LastOrDefault();

        var train = finalLeg?.Train;

        // ✅ Lấy trạm hiện tại từ shipment
        var currentStationId = shipment.CurrentStationId;
        Station? currentStation = null;

        if (!string.IsNullOrEmpty(currentStationId))
        {
            currentStation = await _stationRepository.GetByIdAsync(currentStationId);
        }

        // ✅ Lấy trạm đích từ leg cuối
        var destinationStationId = finalLeg?.Route?.ToStationId;
        Station? destinationStation = null;

        if (!string.IsNullOrEmpty(destinationStationId))
        {
            destinationStation = await _stationRepository.GetByIdAsync(destinationStationId);
        }

        // Tracking history
        var parcelTrackingDtos = new List<ParcelTrackingDto>();

        foreach (var parcel in shipment.Parcels)
        {
            foreach (var pt in parcel.ParcelTrackings)
            {
                var stationName = !string.IsNullOrEmpty(pt.StationId)
                    ? await _stationRepository.GetStationNameByIdAsync(pt.StationId)
                    : null;

                parcelTrackingDtos.Add(new ParcelTrackingDto
                {
                    ParcelCode = parcel.ParcelCode,
                    Status = pt.Status,
                    StationId = pt.StationId,
                    StationName = stationName,
                    EventTime = pt.EventTime,
                    Note = pt.Note
                });
            }
        }

        parcelTrackingDtos = parcelTrackingDtos
            .OrderByDescending(x => x.EventTime)
            .ToList();

        return new ShipmentLocationResponse
        {
            TrackingCode = shipment.TrackingCode,
            TrainId = train?.Id,
            TrainCode = train?.TrainCode,
            Latitude = currentStation?.Latitude,
            Longitude = currentStation?.Longitude,
            CurrentStationId = currentStation?.Id,
            CurrentStationName = currentStation?.StationNameVi,
            DestinationStationId = destinationStation?.Id,
            DestinationStationName = destinationStation?.StationNameVi,
            ShipmentStatus = shipment.ShipmentStatus.ToString(),
            EstimatedArrivalTime = finalLeg?.Date,
            ParcelTrackingHistory = parcelTrackingDtos
        };
    }

    public async Task<UpdateShipmentStatusResponse> UpdateShipmentStatusByStationAsync(UpdateShipmentStatusRequest request, string staffId)
    {
        var shipment = await _shipmentRepository.GetShipmentByTrackingCodeAsync(request.TrackingCode);
        if (shipment == null)
            throw new Exception($"Không tìm thấy shipment với mã {request.TrackingCode}");

        var itineraries = shipment.ShipmentItineraries.OrderBy(i => i.LegOrder).ToList();
        if (!itineraries.Any())
            throw new Exception("Không có lịch trình hợp lệ cho shipment này.");

        var finalLeg = itineraries.Last();
        var currentStationId = request.CurrentStationId;

        var train = finalLeg.Train;
        var currentStationName = await _stationRepository.GetStationNameByIdAsync(currentStationId);
        var destinationStationId = finalLeg.Route?.ToStationId;
        var destinationStationName = finalLeg.Route?.ToStation?.StationNameVi ?? "Không rõ";

        string message;
        string shipmentStatus;

        bool isArrivedAtFinalDestination = !string.IsNullOrEmpty(destinationStationId)
            && destinationStationId.Equals(currentStationId, StringComparison.OrdinalIgnoreCase);

        if (isArrivedAtFinalDestination)
        {
            // ✅ Đánh dấu shipment hoàn tất
            shipment.ShipmentStatus = ShipmentStatusEnum.Completed;

            // ✅ Đánh dấu tất cả itinerary cũng hoàn tất
            foreach (var itinerary in shipment.ShipmentItineraries)
            {
                itinerary.IsCompleted = true;
            }

            message = $"🎯 Đơn hàng đã được giao thành công tại trạm **{destinationStationName}**.";
            shipmentStatus = ShipmentStatusEnum.Completed.ToString();

            foreach (var parcel in shipment.Parcels)
            {
                await _shipmentRepository.AddParcelTrackingAsync(
                    parcel.Id,
                    $"Đã giao hàng tại trạm {destinationStationName}",
                    currentStationId,
                    staffId);
            }
        }
        else
        {
            shipment.ShipmentStatus = ShipmentStatusEnum.InTransit;
            message = $"✅ Đơn hàng đã đi qua trạm **{currentStationName}**, chưa đến trạm đích.";
            shipmentStatus = ShipmentStatusEnum.InTransit.ToString();

            foreach (var parcel in shipment.Parcels)
            {
                await _shipmentRepository.AddParcelTrackingAsync(
                    parcel.Id,
                    $"Đã đi qua trạm {currentStationName}",
                    currentStationId,
                    staffId);
            }
        }

        // ✅ Ghi lại station hiện tại
        shipment.CurrentStationId = currentStationId;

        // ✅ Lưu toàn bộ thay đổi
        _shipmentRepository.Update(shipment);

        // Tạo tracking history
        var parcelTrackingDtos = shipment.Parcels
            .SelectMany(p => p.ParcelTrackings.Select(pt => new ParcelTrackingDto
            {
                ParcelCode = p.ParcelCode,
                Status = pt.Status,
                StationId = pt.StationId,
                StationName = pt.StationId != null ? currentStationName : null,
                EventTime = pt.EventTime,
                Note = pt.Note
            }))
            .OrderByDescending(x => x.EventTime)
            .ToList();

        return new UpdateShipmentStatusResponse
        {
            Message = message,
            TrackingCode = shipment.TrackingCode,
            TrainId = train?.Id,
            TrainCode = train?.TrainCode,
            Latitude = train?.Latitude,
            Longitude = train?.Longitude,
            CurrentStationId = currentStationId,
            CurrentStationName = currentStationName,
            DestinationStationId = destinationStationId,
            DestinationStationName = destinationStationName,
            ShipmentStatus = shipmentStatus,
            EstimatedArrivalTime = finalLeg?.Date,
            ParcelTrackingHistory = parcelTrackingDtos
        };
    }

    public async Task<List<ShipmentItineraryResponseDto>> AssignTrainToShipmentAsync(string trackingCode, string trainId)
    {
        var updatedItineraries = await _shipmentItineraryRepository.AssignTrainIdToEmptyLegsAsync(trackingCode, trainId);

        var shipment = await _shipmentRepository.GetShipmentByTrackingCodeAsync(trackingCode);
        string message;

        if (shipment.ShipmentStatus != ShipmentStatusEnum.Completed)
        {
            await _shipmentRepository.UpdateShipmentStatusAsync(shipment.Id, ShipmentStatusEnum.InTransit);
            message = $"🚆 Đã gán tàu thành công và cập nhật trạng thái đơn hàng {trackingCode} thành 'InTransit'.";
        }
        //else
        //{
        //    message = $"📦 Đơn hàng {trackingCode} đã hoàn thành trước đó. Gán tàu chỉ để hiển thị lịch trình.";
        //}

        return updatedItineraries.Select(it => new ShipmentItineraryResponseDto
        {
            LegOrder = it.LegOrder,
            RouteId = it.RouteId,
            TrainId = it.TrainId,
            TrainCode = it.Train?.TrainCode,
            Date = it.Date,
            TimeSlotId = it.TimeSlotId,
            IsCompleted = it.IsCompleted
        }).ToList();
    }

}