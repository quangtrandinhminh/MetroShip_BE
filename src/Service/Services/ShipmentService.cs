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
using MetroShip.Repository.Extensions;
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
using MetroShip.Service.Jobs;
using Microsoft.Extensions.Caching.Memory;
using Quartz;

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
    private readonly IRouteStationRepository _routeStationRepository = serviceProvider.GetRequiredService<IRouteStationRepository>();
    private readonly IParcelRepository _parcelRepository = serviceProvider.GetRequiredService<IParcelRepository>();
    private readonly IMemoryCache memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
    private readonly IBaseRepository<ShipmentMedia> _shipmentMediaRepository = serviceProvider.GetRequiredService<IBaseRepository<ShipmentMedia>>();
    private readonly ISchedulerFactory _schedulerFactory = serviceProvider.GetRequiredService<ISchedulerFactory>();
    private readonly ITransactionRepository _transactionRepository = serviceProvider.GetRequiredService<ITransactionRepository>();
    private readonly IBaseRepository<ShipmentTracking> _shipmentTrackingRepository = serviceProvider.GetRequiredService<IBaseRepository<ShipmentTracking>>();
    private readonly IBaseRepository<ParcelTracking> _parcelTrackingRepository = serviceProvider.GetRequiredService<IBaseRepository<ParcelTracking>>();
    private readonly IItineraryService _itineraryService = serviceProvider.GetRequiredService<IItineraryService>();
    private readonly IBaseRepository<CategoryInsurance> _categoryInsuranceRepository = serviceProvider.GetRequiredService<IBaseRepository<CategoryInsurance>>();
    private MetroGraph _metroGraph;
    private const string CACHE_KEY = nameof(MetroGraph);
    private const int CACHE_EXPIRY_MINUTES = 30;

    public async Task<PaginatedListResponse<ShipmentListResponse>> GetAllShipmentsAsync(PaginatedListRequest paginatedRequest, ShipmentFilterRequest? filterRequest = null,
    string? searchKeyword = null, DateTimeOffset? createdFrom = null, DateTimeOffset? createdTo = null, OrderByRequest? orderByRequest = null)
    {
        /*_logger.Information("Get all shipments with request: {@request}", paginatedRequest);
        // Build filter expression based on request
        Expression<Func<Shipment, bool>> filterExpression = BuildShipmentFilterExpression(filterRequest);
        // Build order by expression based on request
        Expression<Func<Shipment, object>>? orderByExpression = BuildShipmentOrderByExpression(orderByRequest,
            out bool? IsDesc);

        PaginatedList<Shipment> shipments;
        if (filterRequest.IsAwaitingNextTrain.HasValue && filterRequest.IsAwaitingNextTrain.Value)
        {
            var stationId = filterRequest.ItineraryIncludeStationId ?? JwtClaimUltils.GetUserStation(_httpContextAccessor);
            shipments = await _shipmentRepository.GetShipmentsCanWaitNextTrainAtStation(
                paginatedRequest.PageNumber, paginatedRequest.PageSize, filterRequest.ItineraryIncludeStationId,
                filterExpression, orderByExpression, IsDesc);
        }
        else
        {
            shipments = await _shipmentRepository.GetPaginatedListForListResponseAsync(
                paginatedRequest.PageNumber, paginatedRequest.PageSize,
                filterExpression, orderByExpression, IsDesc
            );
        }

        var shipmentListResponse = _mapperlyMapper.MapToShipmentListResponsePaginatedList(shipments);
        return shipmentListResponse;*/

        _logger.Information(
            $"Get all shipments, search '{searchKeyword}', order by '{orderByRequest?.OrderBy}' {(orderByRequest?.IsDesc == true ? "desc" : "asc")}");

        // ====== FILTER ======
        Expression<Func<Shipment, bool>> predicate = s => true;

        // Search keyword
        if (!string.IsNullOrWhiteSpace(searchKeyword))
        {
            var keywordLower = searchKeyword.Trim().ToLower();
            predicate = predicate.And(s =>
                (s.TrackingCode != null && s.TrackingCode.ToLower().Contains(keywordLower)) ||
                (s.SenderName != null && s.SenderName.ToLower().Contains(keywordLower)) ||
                (s.RecipientName != null && s.RecipientName.ToLower().Contains(keywordLower)) ||
                (s.SenderPhone != null && s.SenderPhone.Contains(keywordLower)) ||
                (s.RecipientPhone != null && s.RecipientPhone.Contains(keywordLower))
            );
        }


        // Created date range
        if (createdFrom.HasValue)
            predicate = predicate.And(s => s.CreatedAt >= createdFrom.Value);
        if (createdTo.HasValue)
            predicate = predicate.And(s => s.CreatedAt <= createdTo.Value);

        // ====== Apply filterRequest ======
        if (filterRequest != null)
        {
            if (!string.IsNullOrWhiteSpace(filterRequest.TrackingCode))
                predicate = predicate.And(s => s.TrackingCode != null &&
                    s.TrackingCode.ToLower().Contains(filterRequest.TrackingCode.ToLower()));

            if (filterRequest.ShipmentStatus.HasValue)
                predicate = predicate.And(s => s.ShipmentStatus == filterRequest.ShipmentStatus);

            if (filterRequest.FromScheduleDateTime.HasValue)
                predicate = predicate.And(s => s.ScheduledDateTime >= filterRequest.FromScheduleDateTime.Value);

            if (filterRequest.ToScheduleDateTime.HasValue)
                predicate = predicate.And(s => s.ScheduledDateTime <= filterRequest.ToScheduleDateTime.Value);

            if (!string.IsNullOrWhiteSpace(filterRequest.DepartureStationId))
                predicate = predicate.And(s => s.DepartureStationId == filterRequest.DepartureStationId);

            if (!string.IsNullOrWhiteSpace(filterRequest.DestinationStationId))
                predicate = predicate.And(s => s.DestinationStationId == filterRequest.DestinationStationId);

            if (!string.IsNullOrWhiteSpace(filterRequest.SenderName))
                predicate = predicate.And(s => s.SenderName != null &&
                    s.SenderName.ToLower().Contains(filterRequest.SenderName.ToLower()));

            if (!string.IsNullOrWhiteSpace(filterRequest.SenderPhone))
                predicate = predicate.And(s => s.SenderPhone != null &&
                    s.SenderPhone.Contains(filterRequest.SenderPhone));

            if (!string.IsNullOrWhiteSpace(filterRequest.RecipientName))
                predicate = predicate.And(s => s.RecipientName != null &&
                    s.RecipientName.ToLower().Contains(filterRequest.RecipientName.ToLower()));

            if (!string.IsNullOrWhiteSpace(filterRequest.RecipientPhone))
                predicate = predicate.And(s => s.RecipientPhone != null &&
                    s.RecipientPhone.Contains(filterRequest.RecipientPhone));

            if (!string.IsNullOrWhiteSpace(filterRequest.RecipientEmail))
                predicate = predicate.And(s => s.RecipientEmail != null &&
                    s.RecipientEmail.ToLower().Contains(filterRequest.RecipientEmail.ToLower()));

            if (!string.IsNullOrWhiteSpace(filterRequest.TimeSlotId))
                predicate = predicate.And(s => s.TimeSlotId == filterRequest.TimeSlotId);

            if (!string.IsNullOrWhiteSpace(filterRequest.LineId))
                predicate = predicate.And(s => s.ShipmentItineraries.Any(si => si.Route.LineId == filterRequest.LineId));

            if (!string.IsNullOrWhiteSpace(filterRequest.ItineraryIncludeStationId))
                predicate = predicate.And(s => s.ShipmentItineraries.Any(si =>
                    si.Route.FromStationId == filterRequest.ItineraryIncludeStationId ||
                    si.Route.ToStationId == filterRequest.ItineraryIncludeStationId));
        }

        // ====== SORT ======
        Expression<Func<Shipment, object>>? orderBy = orderByRequest?.OrderBy?.ToLower() switch
        {
            "trackingcode" => s => s.TrackingCode!,
            "departurestationname" => s => s.DepartureStationName!,
            "destinationstationname" => s => s.DestinationStationName!,
            "currentstationname" => s => s.CurrentStationName!,
            "sendername" => s => s.SenderName!,
            "recipientname" => s => s.RecipientName!,
            "senderphone" => s => s.SenderPhone!,
            "recipientphone" => s => s.RecipientPhone!,
            "totalcostvnd" => s => s.TotalCostVnd,
            "scheduleddatetime" => s => s.ScheduledDateTime,
            "bookedat" => s => s.BookedAt,
            _ => s => s.CreatedAt
        };

        bool sortDesc = orderByRequest?.IsDesc ?? true;

        // ====== GET LIST ======
        PaginatedList<Shipment> shipments;
        if (filterRequest.IsAwaitingNextTrain.HasValue && filterRequest.IsAwaitingNextTrain.Value)
        {
            var stationId = filterRequest.ItineraryIncludeStationId ?? JwtClaimUltils.GetUserStation(_httpContextAccessor);
            shipments = await _shipmentRepository.GetShipmentsCanWaitNextTrainAtStation(
                paginatedRequest.PageNumber, paginatedRequest.PageSize, filterRequest.ItineraryIncludeStationId,
                predicate, orderBy, sortDesc);
        }
        else
        {
            shipments = await _shipmentRepository.GetPaginatedListForListResponseAsync(
                paginatedRequest.PageNumber,
                paginatedRequest.PageSize,
                predicate,
                orderBy,
                sortDesc);
        }

        var shipmentResponse = _mapperlyMapper.MapToShipmentListResponsePaginatedList(shipments);

        return shipmentResponse;
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
        InitializeGraphAsync().Wait();
        shipmentResponse.ItineraryGraph = _metroGraph.CreateItineraryGraphResponses(
            shipmentResponse.ShipmentItineraries.Select(x => x.RouteId).ToList(),
            _mapperlyMapper);
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

    // from customer, no need tracking parcel
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
        var routes = await _routeStationRepository.IsExistAsync(
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

        // set scheduled date time and time slot for the first itinerary
        shipment.ScheduledShift = timeSlot.Shift;
        firstItinerary.TimeSlotId = shipment.TimeSlotId;
        var scheduledSystemTime = request.ScheduledDateTime.UtcToSystemTime();
        var startReceiveAtSystemTime = request.StartReceiveAt?.UtcToSystemTime() ?? null;
        firstItinerary.Date = new DateOnly(scheduledSystemTime.Year,
            scheduledSystemTime.Month, scheduledSystemTime.Day);

        // generate shipment tracking code
        shipment.TrackingCode = TrackingCodeGenerator.GenerateShipmentTrackingCode(
            departureStation.Region.RegionCode, scheduledSystemTime);

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
        await ScheduleUnpaidJob(shipment.Id, shipment.PaymentDealine.Value);

        shipment.ShipmentTrackings.Add(new ShipmentTracking
        {
            ShipmentId = shipment.Id,
            CurrentShipmentStatus = ShipmentStatusEnum.AwaitingPayment,
            Status = $"Đơn hàng đã được đặt với mã vận đơn {shipment.TrackingCode}",
            EventTime = shipment.BookedAt.Value,
            UpdatedBy = customerId
        });
        shipment = await _shipmentRepository.AddAsync(shipment, cancellationToken);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);

        // Check if all itineraries have been scheduled
        /*var maxAttempt = _systemConfigRepository
            .GetSystemConfigValueByKey(nameof(SystemConfigSetting.Instance.MAX_NUMBER_OF_SHIFT_ATTEMPTS));*/
        await _itineraryService.CheckAvailableTimeSlotsAsync(shipment.Id, 3);

        // send email to customer
        _logger.Information("Scheduling to send email to customer with tracking code: {@trackingCode}", 
            shipment.TrackingCode);
        var user = await _userRepository.GetUserByIdAsync(customerId);
        shipment.StartReceiveAt = startReceiveAtSystemTime;
        shipment.ScheduledDateTime = scheduledSystemTime;
        shipment.DepartureStationName = departureStation.StationNameVi;
        shipment.DepartureStationAddress = departureStation.Address;
        shipment.DestinationStationName = destinationStation.StationNameVi;
        shipment.DestinationStationAddress = destinationStation.Address;

        // Schedule email to customer
        await _emailSender.ScheduleEmailJob(new SendMailModel
        {
            Email = user.Email,
            Type = MailTypeEnum.Shipment,
            //Name = request.SenderName,
            Name = user.UserName,
            Data = shipment,
            Message = request.TrackingLink
        });

        // Schedule email to recipient if provided
        if (!string.IsNullOrEmpty(request.RecipientEmail) && request.RecipientEmail != user.Email)
        {
            await _emailSender.ScheduleEmailJob(new SendMailModel
            {
                Email = request.RecipientEmail,
                Type = MailTypeEnum.Shipment,
                Name = request.RecipientName,
                Data = shipment,
                Message = request.TrackingLink
            });
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

    // from staff, need tracking parcel
    public async Task PickUpShipment (ShipmentPickUpRequest request)
    {
        _logger.Information("Confirm shipment with ID: {@shipmentId}", request.ShipmentId);
        var shipment = await _shipmentRepository.GetSingleAsync(
                       x => x.Id == request.ShipmentId, false,
                       x => x.Parcels
                       );

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

        // Check if pick up in time range
        var now = CoreHelper.SystemTimeNow;
        if (now < shipment.StartReceiveAt || now > shipment.ScheduledDateTime)
        {
            throw new AppException(
                ErrorCode.BadRequest,
                "Parcel confirmation is outside the allowed pickup time range.",
                StatusCodes.Status400BadRequest);
        }

        // Update shipment status and timestamps
        shipment.ShipmentStatus = ShipmentStatusEnum.PickedUp;
        //shipment.PickedUpBy = JwtClaimUltils.GetUserId(_httpContextAccessor);
        shipment.PickedUpAt = CoreHelper.SystemTimeNow;
        shipment.CurrentStationId = shipment.DepartureStationId;
        var stationName = await _stationRepository.GetStationNameByIdAsync(shipment.DepartureStationId);
        _shipmentTrackingRepository.Add(new ShipmentTracking
        {
            ShipmentId = shipment.Id,
            CurrentShipmentStatus = ShipmentStatusEnum.PickedUp,
            Status = $"Đơn hàng đã được nhận tại Ga {stationName}",
            EventTime = shipment.PickedUpAt.Value,
            UpdatedBy = JwtClaimUltils.GetUserId(_httpContextAccessor)
        });

        foreach (var parcel in shipment.Parcels)
        {
            parcel.Status = ParcelStatusEnum.Normal;
            _parcelRepository.Update(parcel);

            _parcelTrackingRepository.Add(new ParcelTracking
            {
                ParcelId = parcel.Id,
                CurrentShipmentStatus = shipment.ShipmentStatus,
                CurrentParcelStatus = parcel.Status,
                TrackingForShipmentStatus = ShipmentStatusEnum.PickedUp,
                Status = $"Kiện hàng đã được nhận tại Ga {stationName}",
                EventTime = shipment.PickedUpAt.Value,
                UpdatedBy = JwtClaimUltils.GetUserId(_httpContextAccessor)
            });
        }
        await CancelUpdateNoDropOffJob(shipment.Id);

        foreach (var media in request.PickedUpMedias)
        {
            var mediaEntity = _mapperlyMapper.MapToShipmentMediaEntity(media);
            mediaEntity.ShipmentId = shipment.Id;
            mediaEntity.BusinessMediaType = BusinessMediaTypeEnum.Pickup;
            mediaEntity.MediaType = DataHelper.IsImage(mediaEntity.MediaUrl);
            _shipmentMediaRepository.Add(mediaEntity);
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
                Message = $"Đơn hàng {shipment.TrackingCode} của bạn đã được nhân viên nhận tại Ga {stationName}.",
            };
            //_emailSender.SendMail(sendMailModel);
            await _emailSender.ScheduleEmailJob(sendMailModel);
        }
    }
    public async Task RejectShipment(ShipmentRejectRequest request)
    {
        _logger.Information("Reject shipment with ID: {@shipmentId}", request.ShipmentId);
        var shipment = await _shipmentRepository.GetSingleAsync(
                x => x.Id == request.ShipmentId, false,
                x => x.Parcels);

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
        var stationName = await _stationRepository.GetStationNameByIdAsync(shipment.DepartureStationId);
        shipment.ShipmentStatus = ShipmentStatusEnum.Rejected;
        //shipment.RejectedBy = JwtClaimUltils.GetUserId(_httpContextAccessor);
        //shipment.RejectionReason = request.Reason;
        shipment.RejectedAt = CoreHelper.SystemTimeNow;
        _shipmentTrackingRepository.Add(new ShipmentTracking
        {
            ShipmentId = shipment.Id,
            CurrentShipmentStatus = ShipmentStatusEnum.Rejected,
            Status = $"Đơn hàng đã bị từ chối tại Ga {stationName}",
            EventTime = shipment.RejectedAt.Value,
            UpdatedBy = JwtClaimUltils.GetUserId(_httpContextAccessor),
            Note = request.Reason
        });

        foreach (var parcel in shipment.Parcels)
        {
            parcel.Status = ParcelStatusEnum.Canceled;
            _parcelRepository.Update(parcel);

            _parcelTrackingRepository.Add(new ParcelTracking
            {
                ParcelId = parcel.Id,
                CurrentShipmentStatus = shipment.ShipmentStatus,
                CurrentParcelStatus = parcel.Status,
                TrackingForShipmentStatus = ShipmentStatusEnum.Rejected,
                Status = $"Kiện hàng đã bị từ chối tại Ga {stationName}",
                EventTime = shipment.RejectedAt.Value,
                UpdatedBy = JwtClaimUltils.GetUserId(_httpContextAccessor),
                Note = request.Reason
            });
        }

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
                Message = $"Đơn hàng với mã vận đơn {shipment.TrackingCode} của bạn đã bị Từ chối" +
                $"Lý do: {request.Reason}",
            };
            //_emailSender.SendMail(sendMailModel);
            await _emailSender.ScheduleEmailJob(sendMailModel);
        }
    }
    public async Task CompleteShipment(ShipmentPickUpRequest request)
    {
        _logger.Information("Complete shipment with ID: {@shipmentId}", request.ShipmentId);
        //ShipmentValidator.ValidateShipmentCompleteRequest(request);

        var shipment = await _shipmentRepository.GetSingleAsync(
        x => x.Id == request.ShipmentId, false, x => x.Parcels);

        // Check if the shipment exists
        if (shipment == null)
        {
            throw new AppException(
            ErrorCode.NotFound,
            ResponseMessageShipment.SHIPMENT_NOT_FOUND,
            StatusCodes.Status400BadRequest);
        }

        // Check if the shipment is awaiting delivery
        if (shipment.ShipmentStatus != ShipmentStatusEnum.AwaitingDelivery)
        {
            throw new AppException(
            ErrorCode.BadRequest,
            ResponseMessageShipment.SHIPMENT_NOT_AWAITING_DELIVERY,
            StatusCodes.Status400BadRequest);
        }

        /*if (shipment.ShipmentStatus == ShipmentStatusEnum.ApplyingSurcharge)
        {
            var transaction = await _transactionRepository.GetSingleAsync(
                x => x.ShipmentId == shipment.Id && x.TransactionType == TransactionTypeEnum.Surcharge
                                && x.PaymentStatus == PaymentStatusEnum.Pending);
            if (transaction != null)
            {
                // If there is a pending surcharge transaction, we cannot complete the shipment
                throw new AppException(
                ErrorCode.BadRequest,
                "Cannot complete shipment while surcharge is pending.",
                StatusCodes.Status400BadRequest);
            }
        }*/

        // Update shipment status and timestamps
        shipment.ShipmentStatus = shipment.Parcels.Any(p => p.Status == ParcelStatusEnum.Lost) 
            ? ShipmentStatusEnum.CompletedWithCompensation
            : ShipmentStatusEnum.Completed;
        shipment.CompletedAt = CoreHelper.SystemTimeNow;
        var stationName = await _stationRepository.GetStationNameByIdAsync(shipment.DestinationStationId);
        if (shipment.ShipmentStatus == ShipmentStatusEnum.CompletedWithCompensation)
        {
            // If there are lost parcels, calculate compensation
            var lostParcels = shipment.Parcels
                .Where(p => p.Status == ParcelStatusEnum.Lost)
                .ToList();
            var categoryInsurances = _categoryInsuranceRepository.GetAllWithCondition(
                x => lostParcels.Select(p => p.CategoryInsuranceId).Contains(x.Id),
                x => x.ParcelCategory, _ => _.InsurancePolicy
            ).ToList();

            shipment.TotalCompensationFeeVnd = ParcelPriceCalculator.CalculateParcelCompensation(
                lostParcels, categoryInsurances, _parcelRepository);
        }

        _shipmentTrackingRepository.Add(new ShipmentTracking
        {
            ShipmentId = shipment.Id,
            CurrentShipmentStatus = shipment.ShipmentStatus,
            Status = $"Đơn hàng đã được giao thành công tại Ga {stationName}",
            EventTime = shipment.CompletedAt.Value,
            UpdatedBy = JwtClaimUltils.GetUserId(_httpContextAccessor)
        });

        var normalParcel = shipment.Parcels
            .Where(p => p.Status == ParcelStatusEnum.Normal)
            .ToList();
        foreach (var parcel in normalParcel)
        {
            _parcelTrackingRepository.Add(new ParcelTracking
            {
                ParcelId = parcel.Id,
                CurrentShipmentStatus = shipment.ShipmentStatus,
                CurrentParcelStatus = parcel.Status,
                StationId = shipment.DestinationStationId,
                TrackingForShipmentStatus = ShipmentStatusEnum.Completed,
                Status = $"Kiện hàng đã được giao thành công tại Ga {stationName}",
                EventTime = shipment.CompletedAt.Value,
                UpdatedBy = JwtClaimUltils.GetUserId(_httpContextAccessor)
            });
        }

        foreach (var media in request.PickedUpMedias)
        {
            var mediaEntity = _mapperlyMapper.MapToShipmentMediaEntity(media);
            mediaEntity.ShipmentId = shipment.Id;
            mediaEntity.BusinessMediaType = BusinessMediaTypeEnum.IdentityVerification;
            mediaEntity.MediaType = DataHelper.IsImage(mediaEntity.MediaUrl);
            _shipmentMediaRepository.Add(mediaEntity);
        }

        // Save changes to the database
        _shipmentRepository.Update(shipment);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);

        // Optionally send completion email to sender
        var user = await _userRepository.GetUserByIdAsync(shipment.SenderId);
        if (user != null)
        {
            var sendMailModel = new SendMailModel
            {
                Email = user.Email,
                Type = MailTypeEnum.Notification,
                Message = $"Đơn hàng {shipment.TrackingCode} của bạn đã hoàn thành",
            };
            //_emailSender.SendMail(sendMailModel);
            await _emailSender.ScheduleEmailJob(sendMailModel);
        }
    }
    public async Task ApplySurchargeForShipment(string shipmentId)
    {
        _logger.Information("Applying surcharge for shipment ID: {@shipmentId}", shipmentId);
        var shipment = await _shipmentRepository.GetSingleAsync(
            x => x.Id == shipmentId,
            false,
            x => x.Parcels
        );

        // Check if the shipment exists
        if (shipment == null)
        {
            throw new AppException(
            ErrorCode.NotFound,
            ResponseMessageShipment.SHIPMENT_NOT_FOUND,
            StatusCodes.Status400BadRequest);
        }

        // Check if the shipment is awaiting payment
        if (shipment.ShipmentStatus != ShipmentStatusEnum.AwaitingDelivery ||
            shipment.ShipmentStatus != ShipmentStatusEnum.ApplyingSurcharge)
        {
            throw new AppException(
            ErrorCode.BadRequest,
            "Shipment must be in AwaitingDelivery or ApplyingSurcharge status to apply surcharge.",
            StatusCodes.Status400BadRequest);
        }

        // Update the shipment status and save changes
        if (shipment.SurchargeAppliedAt == null)
        {
            shipment.SurchargeAppliedAt = CoreHelper.SystemTimeNow;
            shipment.ShipmentStatus = ShipmentStatusEnum.ApplyingSurcharge;
            await _pricingService.CalculateOverdueSurcharge(shipment);
            _shipmentTrackingRepository.Add(new ShipmentTracking
            {
                ShipmentId = shipment.Id,
                CurrentShipmentStatus = ShipmentStatusEnum.ApplyingSurcharge,
                Status = $"Đơn hàng bắt đầu áp dụng phụ phí lưu kho quá hạn",
                EventTime = CoreHelper.SystemTimeNow,
                UpdatedBy = JwtClaimUltils.GetUserId(_httpContextAccessor)
            });

            // Optionally send notification email about the surcharge
            var user = await _userRepository.GetUserByIdAsync(shipment.SenderId);
            if (user != null)
            {
                var sendMailModel = new SendMailModel
                {
                    Email = user.Email,
                    Type = MailTypeEnum.Notification,
                    Message = $"Đơn hàng {shipment.TrackingCode} của bạn đã bắt đầu tính phụ phí lưu kho." +
                    $"Vui lòng liên lạc người nhận nhanh chóng nhận hàng để tránh phát sinh thêm chi phí nhận hàng."
                };
                await _emailSender.ScheduleEmailJob(sendMailModel);
            }
        }
        else
        {
            await _pricingService.CalculateOverdueSurcharge(shipment);
        }
        _shipmentRepository.Update(shipment);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
    }

    // from customer, no need tracking parcel
    public async Task CancelShipment(ShipmentRejectRequest request)
    {
        _logger.Information("Reject shipment with ID: {@shipmentId}", request.ShipmentId);
        var shipment = await _shipmentRepository.GetSingleAsync(
            x => x.Id == request.ShipmentId, false,
            x => x.Parcels);

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
            && shipment.ShipmentStatus != ShipmentStatusEnum.AwaitingPayment
            )
        {
            throw new AppException(
                ErrorCode.BadRequest,
                ResponseMessageShipment.SHIPMENT_CANNOT_CANCEL,
                StatusCodes.Status400BadRequest);
        }

        var customerId = JwtClaimUltils.GetUserId(_httpContextAccessor);
        //shipment.RejectionReason = request.Reason;
        shipment.CancelledAt = CoreHelper.SystemTimeNow;

        // Check if the shipment can be cancelled before a certain time
        /*var allowedCancelBefore = _systemConfigRepository
            .GetSystemConfigValueByKey(nameof(SystemConfigSetting.Instance.ALLOW_CANCEL_BEFORE_HOUR));
        var deadlineForRefund = shipment.ScheduledDateTime.Value.AddHours(-int.Parse(allowedCancelBefore));*/
        var allowedCancelBefore = await _pricingService.GetRefundForCancellationBeforeScheduledHours(shipment.PricingConfigId);
        var deadlineForRefund = shipment.ScheduledDateTime.Value.AddHours(-allowedCancelBefore);
        if (shipment.ShipmentStatus is ShipmentStatusEnum.AwaitingDropOff 
            && shipment.CancelledAt < deadlineForRefund)
        {
            shipment.ShipmentStatus = ShipmentStatusEnum.AwaitingRefund;
            shipment.TotalRefundedFeeVnd = await _pricingService.CalculateRefund(shipment.PricingConfigId, shipment.TotalCostVnd);

            _shipmentTrackingRepository.Add(new ShipmentTracking
            {
                ShipmentId = shipment.Id,
                CurrentShipmentStatus = ShipmentStatusEnum.AwaitingRefund,
                Status = $"Đơn hàng đã bị người gửi hủy và đang chờ hoàn tiền",
                EventTime = shipment.CancelledAt.Value,
                UpdatedBy = customerId,
                Note = request.Reason
            });
        }
        else
        {
            // Update shipment status and timestamps
            shipment.ShipmentStatus = ShipmentStatusEnum.Cancelled;
            _shipmentTrackingRepository.Add(new ShipmentTracking
            {
                ShipmentId = shipment.Id,
                CurrentShipmentStatus = ShipmentStatusEnum.Cancelled,
                Status = $"Đơn hàng đã bị người gửi hủy",
                EventTime = shipment.CancelledAt.Value,
                UpdatedBy = customerId,
                Note = request.Reason
            });
        }

        foreach (var parcel in shipment.Parcels)
        {
            parcel.Status = ParcelStatusEnum.Canceled;
            _parcelRepository.Update(parcel);
        }

        // Save changes to the database
        _shipmentRepository.Update(shipment);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);

        // Optionally send cancellation email to sender
        /*var user = await _userRepository.GetUserByIdAsync(shipment.SenderId);
        if (user != null)
        {
            var sendMailModel = new SendMailModel
            {
                Email = user.Email,
                Type = MailTypeEnum.Notification,
                Message = $"Your shipment with tracking code {shipment.TrackingCode} has been cancelled. " +
                          $"Reason: {request.Reason}",
            };
            //_emailSender.SendMail(sendMailModel);
            await _emailSender.ScheduleEmailJob(sendMailModel);
        }*/
    }
    public async Task UpdateShipmentStatusNoDropOff(string shipmentId)
    {
        _logger.Information("Update shipment status to NoDropOff for ID: {@shipmentId}", shipmentId);
        var shipment = await _shipmentRepository.GetSingleAsync(x => x.Id == shipmentId);

        // Check if the shipment exists
        if (shipment == null)
        {
            throw new AppException(
                ErrorCode.NotFound,
                ResponseMessageShipment.SHIPMENT_NOT_FOUND,
                StatusCodes.Status400BadRequest);
        }

        // Check if the shipment is awaiting drop-off
        if (shipment.ShipmentStatus != ShipmentStatusEnum.AwaitingDropOff)
        {
            throw new AppException(
                ErrorCode.BadRequest,
                "Shipment must be in AwaitingDropOff status to update to NoDropOff.",
                StatusCodes.Status400BadRequest);
        }

        // Update shipment status to NoDropOff
        shipment.ShipmentStatus = ShipmentStatusEnum.NoDropOff;
        _shipmentTrackingRepository.Add(new ShipmentTracking
        {
            ShipmentId = shipment.Id,
            CurrentShipmentStatus = ShipmentStatusEnum.NoDropOff,
            Status = $"Đã tới giờ cắt hàng nhưng Kiện hàng trong đơn chưa được nhận",
            EventTime = CoreHelper.SystemTimeNow,
            UpdatedBy = "System",
        });

        foreach (var parcel in shipment.Parcels)
        {
            parcel.Status = ParcelStatusEnum.Canceled;
            _parcelRepository.Update(parcel);
        }

        // Save changes to the database
        _shipmentRepository.Update(shipment);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
    }
    public async Task UpdateShipmentStatusUnpaid(string shipmentId)
    {
        _logger.Information("Update shipment status to unpaid for ID: {@shipmentId}", shipmentId);
        var shipment = await _shipmentRepository.GetSingleAsync(x => x.Id == shipmentId);

        // Check if the shipment exists
        if (shipment == null)
        {
            throw new AppException(
            ErrorCode.NotFound,
            ResponseMessageShipment.SHIPMENT_NOT_FOUND,
            StatusCodes.Status400BadRequest);
        }

        // Check if the shipment is awaiting payment
        if (shipment.ShipmentStatus != ShipmentStatusEnum.AwaitingPayment)
        {
            throw new AppException(
            ErrorCode.BadRequest,
            "Shipment must be in AwaitingPayment status to update to Unpaid.",
            StatusCodes.Status400BadRequest);
        }

        // Update shipment status to unpaid
        shipment.ShipmentStatus = ShipmentStatusEnum.Unpaid;
        shipment.PaymentDealine = null; // Clear payment deadline
        _shipmentTrackingRepository.Add(new ShipmentTracking
        {
            ShipmentId = shipment.Id,
            CurrentShipmentStatus = ShipmentStatusEnum.Unpaid,
            Status = $"Đơn hàng đã quá hạn thanh toán",
            EventTime = CoreHelper.SystemTimeNow,
            UpdatedBy = JwtClaimUltils.GetUserId(_httpContextAccessor)
        });

        foreach (var parcel in shipment.Parcels)
        {
            parcel.Status = ParcelStatusEnum.Canceled;
            _parcelRepository.Update(parcel);
        }

        // Save changes to the database
        _shipmentRepository.Update(shipment);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
    }
    public async Task FeedbackShipment(ShipmentFeedbackRequest request)
    {
        _logger.Information("Feedback shipment with ID: {@shipmentId}", request.ShipmentId);
        ShipmentValidator.ValidateShipmentFeedbackRequest(request);

        var shipment = await _shipmentRepository.GetSingleAsync(
                       x => x.Id == request.ShipmentId);

        // Check if the shipment exists
        if (shipment == null)
        {
            throw new AppException(
            ErrorCode.BadRequest,
            ResponseMessageShipment.SHIPMENT_NOT_FOUND,
            StatusCodes.Status400BadRequest);
        }

        // Check if the shipment is delivered
        if (shipment.ShipmentStatus != ShipmentStatusEnum.Completed)
        {
            throw new AppException(
            ErrorCode.BadRequest,
            ResponseMessageShipment.SHIPMENT_NOT_COMPLETED,
            StatusCodes.Status400BadRequest);
        }

        // Update feedback
        shipment.Feedback = request.Feedback;
        shipment.Rating = (byte) request.Rating;
        shipment.FeedbackAt = CoreHelper.SystemTimeNow;

        // Save changes to the database
        _shipmentRepository.Update(shipment);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
    }


    public async Task CancelUpdateNoDropOffJob(string shipmentId)
    {
        _logger.Information("Cancelling job to update shipment status to NoDropOff for ID: {@shipmentId}", shipmentId);
        var jobKey = new JobKey($"UpdateShipmentToNoDropOff-{shipmentId}");
        var scheduler = await _schedulerFactory.GetScheduler();
        if (await scheduler.CheckExists(jobKey))
        {
            await scheduler.DeleteJob(jobKey);
            _logger.Information("Cancelled job for shipment ID: {@shipmentId}", shipmentId);
        }
        else
        {
            _logger.Warning("No scheduled job found for shipment ID: {@shipmentId}", shipmentId);
        }
    }

    private async Task ScheduleUnpaidJob(string shipmentId, DateTimeOffset paymentDeadline)
    {
        _logger.Information("Scheduling job to update shipment status to unpaid for ID: {@shipmentId}", shipmentId);
        var jobData = new JobDataMap
        {
            { "Unpaid-for-shipmentId", shipmentId }
        };

        // Schedule the job to run after 15 minutes
        var jobDetail = JobBuilder.Create<UpdateShipmentToUnpaid>()
            .WithIdentity($"UpdateShipmentToUnpaid-{shipmentId}")
            .UsingJobData(jobData)
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity($"Trigger-UpdateShipmentToUnpaid-{shipmentId}")
            .StartAt(paymentDeadline)
            //.StartAt(DateTimeOffset.UtcNow.AddSeconds(5))
            .Build();

        await _schedulerFactory.GetScheduler().Result.ScheduleJob(jobDetail, trigger);
    }

    private void CheckShipmentDate(DateTimeOffset scheduledDateTime)
    {
        _logger.Information("Checking shipment date: {@scheduledDateTime}", scheduledDateTime);
        // Get system config values
        /*var confirmationHour = int.Parse(_systemConfigRepository
            .GetSystemConfigValueByKey(nameof(SystemConfigSetting.Instance.CONFIRMATION_HOUR)));
        var paymentRequestHour = int.Parse(_systemConfigRepository
            .GetSystemConfigValueByKey(nameof(SystemConfigSetting.Instance.PAYMENT_REQUEST_HOUR)));*/
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

        if (!string.IsNullOrWhiteSpace(request.SenderId))
        {
            expression = expression.And(x => x.SenderId == request.SenderId);
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
        var validPaths = allPaths.Where(r => r.Path?.Any() == true).ToList();

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
        // Get insurance policy base on categoryInsuranceIds
        var categoryInsuranceIds = request.Parcels
            .Select(p => p.CategoryInsuranceId)
            .Distinct()
            .ToList();
        var categoryInsurance = await _categoryInsuranceRepository
            .GetAllWithCondition(x => categoryInsuranceIds.Contains(x.Id) && x.IsActive && x.DeletedAt == null
            , x => x.InsurancePolicy, x => x.ParcelCategory
            )
            .ToListAsync();

        return pathResults.Select(r =>
        {
            var pathResponse = _metroGraph.CreateResponseFromPath(r.Path, _mapperlyMapper);
            _mapperlyMapper.CloneToParcelRequestList(request.Parcels, pathResponse.Parcels);

            // Calculate pricing for each parcel
            /*ParcelPriceCalculator.CalculateParcelPricing(
                pathResponse.Parcels, pathResponse, _pricingService, categories);*/

            ParcelPriceCalculator.CalculateParcelPricing(
                pathResponse.Parcels, pathResponse, _pricingService, categoryInsurance);

            // Check est arrival time
            var date = new DateOnly(request.ScheduledDateTime.Year,
                               request.ScheduledDateTime.Month, request.ScheduledDateTime.Day);
            pathResponse.EstArrivalTime = _itineraryService.CheckEstArrivalTime(pathResponse, request.TimeSlotId, date).Result;

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
            //EstimatedArrivalTime = finalLeg?.Date,
            ParcelTrackingHistory = parcelTrackingDtos
        };
    }

    public async Task<UpdateShipmentStatusResponse> UpdateShipmentStatusAsync(UpdateShipmentStatusRequest request, ShipmentStatusEnum targetStatus, string staffId)
    {
        var shipment = await _shipmentRepository.GetShipmentByTrackingCodeAsync(request.TrackingCode)
                       ?? throw new Exception($"Không tìm thấy shipment với mã {request.TrackingCode}");

        // Xác định trạng thái hiện tại và điều kiện chuyển tiếp hợp lệ
        if (!IsValidStatusTransition(shipment.ShipmentStatus, targetStatus))
            throw new Exception($"Không thể chuyển trạng thái từ {shipment.ShipmentStatus} sang {targetStatus}");

        // Lấy thông tin điểm hiện tại (station/warehouse)
        var currentStationId = request.CurrentStationId;
        var stationName = await _stationRepository.GetStationNameByIdAsync(currentStationId)
                         ?? throw new Exception("Không tìm thấy điểm đến tương ứng.");

        shipment.CurrentStationId = currentStationId;
        shipment.ShipmentStatus = targetStatus;

        // Tạo message tùy theo trạng thái
        string message = targetStatus switch
        {
            ShipmentStatusEnum.UnloadingAtStation =>
                $"🚉 Đơn hàng đã được dỡ xuống tại trạm **{stationName}** sau khi giao hàng.",

            ShipmentStatusEnum.StorageInWarehouse =>
                $"📦 Đơn hàng đã được lưu kho tại **{stationName}** sau khi dỡ xuống từ trạm.",

            _ => throw new Exception("Trạng thái không hợp lệ.")
        };

        // Ghi tracking cho từng parcel
        foreach (var parcel in shipment.Parcels)
        {
            string note = targetStatus == ShipmentStatusEnum.UnloadingAtStation
                ? $"Đã dỡ hàng tại trạm {stationName}"
                : $"Đã lưu kho tại {stationName}";

            await _shipmentRepository.AddParcelTrackingAsync(parcel.Id, note, currentStationId, staffId);
        }

        _shipmentRepository.Update(shipment);

        var trackingDtos = shipment.Parcels
            .SelectMany(p => p.ParcelTrackings.Select(pt => new ParcelTrackingDto
            {
                ParcelCode = p.ParcelCode,
                Status = pt.Status,
                StationId = pt.StationId,
                StationName = stationName,
                EventTime = pt.EventTime,
                Note = pt.Note
            }))
            .OrderByDescending(x => x.EventTime)
            .ToList();

        return new UpdateShipmentStatusResponse
        {
            Message = message,
            TrackingCode = shipment.TrackingCode,
            ShipmentStatus = targetStatus.ToString(),
            CurrentStationId = currentStationId,
            CurrentStationName = stationName,
            ParcelTrackingHistory = trackingDtos
        };
    }

    private bool IsValidStatusTransition(ShipmentStatusEnum current, ShipmentStatusEnum target)
    {
        return (current == ShipmentStatusEnum.Completed && target == ShipmentStatusEnum.UnloadingAtStation)
            || (current == ShipmentStatusEnum.UnloadingAtStation && target == ShipmentStatusEnum.StorageInWarehouse);
    }

    public async Task<List<ShipmentItineraryResponseDto>> AssignTrainToShipmentAsync(string trackingCode, string trainId)
    {
        var updatedItineraries = await _shipmentItineraryRepository.AssignTrainIdToEmptyLegsAsync(trackingCode, trainId);

        var shipment = await _shipmentRepository.GetShipmentByTrackingCodeAsync(trackingCode);
        string message;

        if (shipment.ShipmentStatus != ShipmentStatusEnum.Completed)
        {
            await _shipmentRepository.UpdateShipmentStatusAsync(shipment.Id, ShipmentStatusEnum.LoadOnMetro);
            message = $"🚆 Đã gán tàu thành công và cập nhật trạng thái đơn hàng {trackingCode} thành 'AwaitingDelivery'.";
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
            //Date = it.Date,
            TimeSlotId = it.TimeSlotId,
            IsCompleted = it.IsCompleted
        }).ToList();
    }
}