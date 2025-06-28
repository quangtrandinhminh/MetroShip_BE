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
using System.Text.Json;
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
using MetroShip.Service.ApiModels.Graph;
using MetroShip.Service.ApiModels.Parcel;
using System;
using Microsoft.EntityFrameworkCore;

namespace MetroShip.Service.Services;

public class ShipmentService : IShipmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapperlyMapper _mapperlyMapper;
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IShipmentItineraryRepository _shipmentItineraryRepository;
    private readonly IParcelCategoryRepository _parcelCategoryRepository;
    private readonly ILogger _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IStationRepository _stationRepository;
    private readonly ShipmentValidator _shipmentValidator;
    private readonly ISystemConfigRepository _systemConfigRepository;
    private readonly SystemConfigSetting _systemConfigSetting;
    private readonly IEmailService _emailSender;
    private readonly IUserRepository _userRepository;
    private readonly IBaseRepository<MetroTimeSlot> _metroTimeSlotRepository;
    private bool _isInitializedGraph = false;
    private MetroGraph _metroGraph;
    private bool _isInitializedPricingTable = false;
    private PricingTable _pricingTable;

    public ShipmentService(
        IServiceProvider serviceProvider,
        IUnitOfWork unitOfWork,
        IShipmentRepository shipmentRepository,
        IShipmentItineraryRepository shipmentItineraryRepository,
        IStationRepository stationRepository,
        ISystemConfigRepository systemConfigRepository,
        IEmailService emailSender,
        IParcelCategoryRepository parcelCategoryRepository,
        IBaseRepository<MetroTimeSlot> metroTimeSlotRepository,
        IUserRepository userRepository)
    {
        _unitOfWork = unitOfWork;
        _mapperlyMapper = serviceProvider.GetRequiredService<IMapperlyMapper>();
        _shipmentRepository = shipmentRepository;
        _shipmentItineraryRepository = shipmentItineraryRepository;
        _logger = serviceProvider.GetRequiredService<ILogger>();
        _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
        _shipmentValidator = new ShipmentValidator();
        _stationRepository = stationRepository;
        _systemConfigRepository = systemConfigRepository;
        _emailSender = emailSender;
        _userRepository = userRepository;
        _parcelCategoryRepository = parcelCategoryRepository;
        _metroTimeSlotRepository = metroTimeSlotRepository;
    }

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

    public async Task<string> BookShipment(ShipmentRequest request, CancellationToken cancellationToken = default)
    {
        var customerId = JwtClaimUltils.GetUserId(_httpContextAccessor);
        _logger.Information("Book shipment with request: {@request}", request);

        // validate shipment request, min 48h max 14 days in advance
        CheckShipmentDate(request.ScheduledDateTime);
        _shipmentValidator.ValidateShipmentRequest(request);

        // get departure station, which accepts the shipment
        var departureStation = await _stationRepository.GetSingleAsync(
                       x => x.Id == request.DepartureStationId && x.IsActive, false,
                       x => x.Region);
        if (departureStation == null)
            throw new AppException(
            ErrorCode.NotFound,
            ResponseMessageShipment.DEPARTURE_STATION_NOT_FOUND,
            StatusCodes.Status404NotFound);

        // Check if all routes exist

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
        firstItinerary.TimeSlotId = request.TimeSlotId;
        firstItinerary.Date = request.ScheduledDateTime;

        // generate shipment tracking code
        shipment.TrackingCode = TrackingCodeGenerator.GenerateShipmentTrackingCode(
            departureStation.Region.RegionCode, shipment.ScheduledDateTime.Value);

        // foreach parcel, set shipment id and generate parcel code
        int index = 0;
        foreach (var parcel in shipment.Parcels)
        {
            parcel.ParcelCode = TrackingCodeGenerator.GenerateParcelCode(
                               shipment.TrackingCode, index);
            // shipment.TotalCostVnd += parcel.PriceVnd;
            // shipment.TotalShippingFeeVnd += parcel.PriceVnd;

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

        if (_isInitializedPricingTable == false) await InitializePricingTableAsync();
        shipment.PriceStructureDescriptionJSON = JsonSerializer.Serialize(_pricingTable);
        shipment = await _shipmentRepository.AddAsync(shipment, cancellationToken);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);

        // send email to customer
        _logger.Information("Send email to customer with tracking code: {@trackingCode}", 
            shipment.TrackingCode);
        var user = await _userRepository.GetUserByIdAsync(customerId);
        shipment.ScheduledDateTime = request.ScheduledDateTime;
        var sendMailModel = new SendMailModel
        {
            Email = user.Email,
            Type = MailTypeEnum.Shipment,
            Name = request.SenderName,
            Data = shipment,
        };
        _emailSender.SendMail(sendMailModel);

        // send email to recipient if provided
        if (request.RecipientEmail is not null && request.RecipientEmail != user.Email)
        {
            // send email to recipient
            _logger.Information("Send email to recipient with tracking code: {@trackingCode}", shipment.TrackingCode);
            var recipientSendMailModel = new SendMailModel
            {
                Email = request.RecipientEmail,
                Type = MailTypeEnum.Shipment,
                Name = request.RecipientName,
                Data = shipment,
            };
            _emailSender.SendMail(recipientSendMailModel);
        }

        return shipment.TrackingCode;
    }

    private async Task InitializeGraphAsync()
    {
        if (_isInitializedGraph)
            return;

        var (routes, stations, metroLines) =
            await _shipmentItineraryRepository.GetRoutesAndStationsAsync();

        // Khởi tạo đồ thị metro
        _metroGraph = new MetroGraph(routes, stations, metroLines);
        _isInitializedGraph = true;
    }

    private async Task InitializePricingTableAsync()
    {
        if (_isInitializedPricingTable)
            return;

        // Lấy tất cả cấu hình giá từ hệ thống
        var allConfigs = await _systemConfigRepository
            .GetAllSystemConfigs(ConfigTypeEnum.PriceStructure);
        var pricingTableBuilder = new PricingTableBuilder();
        _pricingTable = pricingTableBuilder.BuildPricingTable(allConfigs);
        _isInitializedPricingTable = true;
    }

    /*public async Task<BestPathGraphResponse> FindPathAsync(BestPathRequest request)
    {
        await InitializeGraphAsync();

        // Sử dụng đồ thị để tìm đường đi
        var path = _metroGraph.FindShortestPathByBFS(request.DepartureStationId, request.DestinationStationId);

        if (path == null || !path.Any())
            throw new AppException(
            ErrorCode.NotFound,
            ResponseMessageShipment.PATH_NOT_FOUND,
            StatusCodes.Status404NotFound);

        // Chuyển đổi đường đi thành itinerary
        return _metroGraph.CreateResponseFromPath(path, _mapperlyMapper);
    }*/

    // v0
    /*public async Task<TotalPriceResponse> GetItineraryAndTotalPrice(TotalPriceCalcRequest request)
    {
        _logger.Information("Get itinerary and total price with request: {@request}", request);
        _shipmentValidator.ValidateTotalPriceCalcRequest(request);

        // check if schedule shipment date is min 48h and max 14 days in advance
        CheckShipmentDate(request.ScheduleShipmentDate);

        // get departure stations near user location, which accepts the shipment
        var maxDistanceInMeters = int.Parse(_systemConfigRepository
                       .GetSystemConfigValueByKey(nameof(_systemConfigSetting.MAX_DISTANCE_IN_METERS)));
        var maxStationCount = int.Parse(_systemConfigRepository
                       .GetSystemConfigValueByKey(nameof(_systemConfigSetting.MAX_COUNT_STATION_NEAR_USER)));

        // add departure station to the list
        var stationIds = new HashSet<string> { request.DepartureStationId };
        if (request is { UserLongitude: not null, UserLatitude: not null })
        {
            // get all stations near user location, ordered by distance
            stationIds.UnionWith(await _stationRepository.GetAllStationIdNearUser(
                request.UserLatitude.Value, request.UserLongitude.Value, maxDistanceInMeters, maxStationCount));

            // stationIds should contain at least 2 stations, if not, extend maxDistanceInMeters more 1000
            /*while (stationIds.Count < 2 && maxDistanceInMeters < 10000)
            {
                maxDistanceInMeters += 1000; // extend distance by 1000 meters
                stationIds.UnionWith(await _stationRepository.GetAllStationIdNearUser(
                                       request.UserLatitude.Value, request.UserLongitude.Value, maxDistanceInMeters, maxStationCount));
            }#1#
        }

        var response = new TotalPriceResponse();
        // calculate chargeable weight and check if parcel is bulk
        response.ParcelRequests = request.Parcels;
        foreach (var parcel in request.Parcels)
        {
            parcel.ChargeableWeight = CalculateHelper.CalculateChargeableWeight(
                parcel.LengthCm, parcel.WidthCm, parcel.HeightCm, parcel.WeightKg);

            parcel.IsBulk = parcel.ChargeableWeight > parcel.WeightKg;
        }

        // Assume stationIds is a HashSet<string> where:
        // - stationIds.ElementAt(0) is the user's chosen departure station
        // - stationIds.ElementAt(1) is the nearest station
        var stationIdList = stationIds.ToList();

        // use algorithm to find paths from each departure station to the destination
        InitializeAsync().Wait(); // Ensure the graph is initialized
        var pathTasks = stationIdList.Select(async departureStationId => {
            List<string> path = _stationRepository.AreStationsInSameMetroLine(departureStationId, request.DestinationStationId)
                ? _metroGraph.FindShortestPathByBFS(departureStationId, request.DestinationStationId)
                : _metroGraph.FindShortestPathByDijkstra(departureStationId, request.DestinationStationId);
            return (StationId: departureStationId, Path: path);
        }).ToList();
        var allPaths = await Task.WhenAll(pathTasks);

        // Filter out null/empty paths and keep mapping
        List<(string StationId, List<string> Path)> pathResults = allPaths
            .Where(r => r.Path != null && r.Path.Any())
            .ToList();

        // Now create responses (BestPathGraphResponse) from those valid paths
        var bestPathResponses = pathResults.Select(r => new {
            StationId = r.StationId,
            Response = _metroGraph.CreateResponseFromPath(r.Path, _mapperlyMapper)
        }).ToList();

        // Assign Standard (user choice), Nearest, and Cheapest
        response.Standard = bestPathResponses
            .FirstOrDefault(r => r.StationId == stationIdList[0])?.Response;

        response.Nearest = bestPathResponses.Count > 1
            ? bestPathResponses.FirstOrDefault(r => r.StationId == stationIdList[1])?.Response
            : null;

        response.Cheapest = bestPathResponses.Count > 1
            ? bestPathResponses
                //.OrderBy(r => r.Response.ShippingFeeByItinerary)
                .FirstOrDefault()?.Response
            : null;

        response.StationsInDistanceMeter = maxDistanceInMeters;
        return response;
    }*/

    // v1
    /*public async Task<TotalPriceResponse> GetItineraryAndTotalPrice(TotalPriceCalcRequest request)
    {
        _logger.Information("Get itinerary and total price with request: {@request}", request);
        _shipmentValidator.ValidateTotalPriceCalcRequest(request);

        // Check if schedule shipment date is min 48h and max 14 days in advance
        CheckShipmentDate(request.ScheduleShipmentDate);

        // Get departure stations near user location, which accepts the shipment
        var maxDistanceInMeters = int.Parse(_systemConfigRepository
                       .GetSystemConfigValueByKey(nameof(_systemConfigSetting.MAX_DISTANCE_IN_METERS)));
        var maxStationCount = int.Parse(_systemConfigRepository
                       .GetSystemConfigValueByKey(nameof(_systemConfigSetting.MAX_COUNT_STATION_NEAR_USER)));

        // Add departure station to the list
        var stationIds = new HashSet<string> { request.DepartureStationId };
        if (request is { UserLongitude: not null, UserLatitude: not null })
        {
            stationIds.UnionWith(await _stationRepository.GetAllStationIdNearUser(
                request.UserLatitude.Value, request.UserLongitude.Value, maxDistanceInMeters, maxStationCount));
        }

        var response = new TotalPriceResponse();
        var stationIdList = stationIds.ToList();
        // Use algorithm to find paths from each departure station to the destination
        InitializeAsync().Wait();
        var pathTasks = stationIdList.Select(async departureStationId => {
            List<string> path = _stationRepository.AreStationsInSameMetroLine(departureStationId, request.DestinationStationId)
                ? _metroGraph.FindShortestPathByBFS(departureStationId, request.DestinationStationId)
                : _metroGraph.FindShortestPathByDijkstra(departureStationId, request.DestinationStationId);
            return (StationId: departureStationId, Path: path);
        }).ToList();
        var allPaths = await Task.WhenAll(pathTasks);

        List<(string StationId, List<string> Path)> pathResults = allPaths
            .Where(r => r.Path != null && r.Path.Any())
            .ToList();

        // Create responses with calculated shipping fees
        var categoryIds = request.Parcels.Select(p => p.ParcelCategoryId).Distinct().ToList();
        var categories = _parcelCategoryRepository.GetAllWithCondition

            (x => categoryIds.Contains(x.Id) && x.IsActive && x.DeletedAt == null);
        var category = new ParcelCategory();
        var chargeableWeight = 0m;
        // Build pricing table from system configs
        var allConfigs = await _systemConfigRepository.GetAllSystemConfigs(ConfigTypeEnum.PriceStructure);
        var pricingTableBuilder = new PricingTableBuilder();
        var pricingTable = pricingTableBuilder.BuildPricingTable(allConfigs);
        var priceCalculationService = new PriceCalculationService(pricingTable);
        var bestPathResponses = pathResults.Select(r => {
            var pathResponse = _metroGraph.CreateResponseFromPath(r.Path, _mapperlyMapper);

            // Calculate chargeable weight and check if parcel is bulk
            foreach (var parcel in request.Parcels)
            {
                chargeableWeight = CalculateHelper.CalculateChargeableWeight(
                    parcel.LengthCm, parcel.WidthCm, parcel.HeightCm, parcel.WeightKg);
                parcel.ChargeableWeight = chargeableWeight;
                parcel.IsBulk = parcel.ChargeableWeight > parcel.WeightKg;

                // Calculate shipping fee based on total weight and path distance
                parcel.PriceVnd = priceCalculationService.CalculateShippingPrice(chargeableWeight
                    , pathResponse.TotalKm);
                pathResponse.TotalShippingFeeVnd += (decimal) parcel.PriceVnd;

                category = categories
                    .FirstOrDefault(c => c.Id == parcel.ParcelCategoryId);

                if (category != null && category.IsInsuranceRequired)
                {
                    if (category.InsuranceRate != null)
                    {
                        parcel.InsuranceFeeVnd = parcel.PriceVnd * category.InsuranceRate;
                    }
                    else
                    {
                        parcel.InsuranceFeeVnd = category.InsuranceFeeVnd;
                    }

                    parcel.PriceVnd += parcel.InsuranceFeeVnd;
                }
            }
            //response.ParcelRequests = request.Parcels;
            pathResponse.Parcels = request.Parcels;

            return new
            {
                StationId = r.StationId,
                Response = pathResponse
            };
        }).ToList();

        // Assign Standard (user choice), Nearest, and Cheapest
        response.Standard = bestPathResponses
            .FirstOrDefault(r => r.StationId == stationIdList[0])?.Response;

        response.Nearest = bestPathResponses.Count > 1
            ? bestPathResponses.FirstOrDefault(r => r.StationId == stationIdList[1])?.Response
            : null;

        response.Cheapest = bestPathResponses.Count > 1
            ? bestPathResponses
                .OrderBy(r => r.Response.TotalKm)
                .FirstOrDefault()?.Response
            : null;

        response.StationsInDistanceMeter = maxDistanceInMeters;
        return response;
    }*/

    // v2
    public async Task<TotalPriceResponse> GetItineraryAndTotalPrice(TotalPriceCalcRequest request)
    {
        _logger.Information("Get itinerary and total price with request: {@request}", request);

        // Validation
        _shipmentValidator.ValidateTotalPriceCalcRequest(request);

        // Get station options
        var stationIds = await GetNearUserStations(request);

        // Find paths
        var pathResults = await FindOptimalPaths(stationIds.ToList(), request.DestinationStationId);

        // Calculate pricing
        var bestPathResponses = await CalculatePricingForPaths(pathResults, request);

        // Build response
        return BuildTotalPriceResponse(bestPathResponses, stationIds.ToList());
    }

    /*public async Task<PaginatedListResponse<ShipmentListResponse>> GetShipmentByLineAndDate(
        PaginatedListRequest request, string lineCode, DateTimeOffset date, string? regionCode, ShiftEnum? shift)
    {
        _logger.Information("Get shipment by line code: {@lineCode} and date: {@date}", lineCode, date);
        var shipments = await _shipmentRepository.GetShipmentsByLineIdAndDateAsync(
            request.PageNumber, request.PageSize, 
            lineCode, date, regionCode, shift);

        var shipmentResponses =
            _mapperlyMapper.MapToShipmentListResponsePaginatedList(shipments);
        return shipmentResponses;
    }*/

    /*public async Task AcceptShipment (string shipmentId)
    {
        _logger.Information("Confirm shipment with ID: {@shipmentId}", shipmentId);
        var shipment = await _shipmentRepository.GetSingleAsync(
                       x => x.Id == shipmentId, false,
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
        if (shipment.ShipmentStatus > ShipmentStatusEnum.Processing)
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
        shipment.ShipmentStatus = ShipmentStatusEnum.Accepted;
        shipment.ApprovedAt = CoreHelper.SystemTimeNow;
        await shipment.Parcels.ParallelForEachAsync(p =>
        {
            p.ParcelStatus = ParcelStatusEnum.AwaitingPayment;
            return Task.CompletedTask;
        });

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
    }*/

    /*public async Task RejectShipment(string shipmentId, string rejectReason)
    {
        _logger.Information("Reject shipment with ID: {@shipmentId}", shipmentId);
        var shipment = await _shipmentRepository.GetSingleAsync(
                x => x.Id == shipmentId, false,
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
        if (shipment.ShipmentStatus > ShipmentStatusEnum.Processing)
        {
            throw new AppException(
            ErrorCode.BadRequest,
            ResponseMessageShipment.SHIPMENT_ALREADY_CONFIRMED,
            StatusCodes.Status400BadRequest);
        }

        // Update shipment status and timestamps
        shipment.ShipmentStatus = ShipmentStatusEnum.Rejected;
        shipment.RejectedAt = CoreHelper.SystemTimeNow;
        await shipment.Parcels.ParallelForEachAsync(p =>
        {
            p.ParcelStatus = ParcelStatusEnum.Rejected;
            return Task.CompletedTask;
        });

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
                $"Reason: {rejectReason}",
            };
            _emailSender.SendMail(sendMailModel);
        }
    }*/

    private void CheckShipmentDate(DateTimeOffset scheduledDateTime)
    {
        // Get system config values
        var confirmationHour = int.Parse(_systemConfigRepository
            .GetSystemConfigValueByKey(nameof(_systemConfigSetting.CONFIRMATION_HOUR)));
        var paymentRequestHour = int.Parse(_systemConfigRepository
            .GetSystemConfigValueByKey(nameof(_systemConfigSetting.PAYMENT_REQUEST_HOUR)));
        var maxScheduleDay = int.Parse(_systemConfigRepository
            .GetSystemConfigValueByKey(nameof(_systemConfigSetting.MAX_SCHEDULE_SHIPMENT_DAY)));
        var minBookDate = CoreHelper.SystemTimeNow.AddHours(confirmationHour + paymentRequestHour);
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
        _shipmentValidator.ValidateShipmentFilterRequest(request);

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

    private async Task<HashSet<string>> GetNearUserStations(TotalPriceCalcRequest request)
    {
        var maxDistanceInMeters = int.Parse(_systemConfigRepository
            .GetSystemConfigValueByKey(nameof(_systemConfigSetting.MAX_DISTANCE_IN_METERS)));
        var maxStationCount = int.Parse(_systemConfigRepository
            .GetSystemConfigValueByKey(nameof(_systemConfigSetting.MAX_COUNT_STATION_NEAR_USER)));

        var stationIds = new HashSet<string> { request.DepartureStationId };

        if (request is { UserLongitude: not null, UserLatitude: not null })
        {
            stationIds.UnionWith(
                await _stationRepository.GetAllStationIdNearUser(
                request.UserLatitude.Value, request.UserLongitude.Value, 
                maxDistanceInMeters, maxStationCount));

            // Ensure at least 2 stations are available
            while (stationIds.Count < 2 && maxDistanceInMeters < maxDistanceInMeters*2)
            {
                maxDistanceInMeters += 2000; // Extend distance by 1000 meters
                stationIds.UnionWith(
                    await _stationRepository.GetAllStationIdNearUser(
                            request.UserLatitude.Value, request.UserLongitude.Value, 
                            maxDistanceInMeters, maxStationCount));
            }
        }

        return stationIds;
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
        InitializePricingTableAsync().Wait();
        var priceCalculationService = new PriceCalculationService(_pricingTable);
        
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

        return pathResults.Select(r => {
            var pathResponse = _metroGraph.CreateResponseFromPath(r.Path, _mapperlyMapper);

            // Calculate pricing for each parcel
            CalculateParcelPricing(request.Parcels, pathResponse, priceCalculationService, categories);

            pathResponse.Parcels = request.Parcels;

            return new
            {
                StationId = r.StationId,
                Response = pathResponse
            };
        }).ToList<dynamic>();
    }

    private void CalculateParcelPricing(
        List<ParcelRequest> parcels,
        BestPathGraphResponse pathResponse,
        PriceCalculationService priceCalculationService,
        List<ParcelCategory> categories)
    {
        foreach (var parcel in parcels)
        {
            // Calculate chargeable weight
            var chargeableWeight = CalculateHelper.CalculateChargeableWeight(
                parcel.LengthCm, parcel.WidthCm, parcel.HeightCm, parcel.WeightKg);

            parcel.ChargeableWeight = chargeableWeight;
            parcel.IsBulk = parcel.ChargeableWeight > parcel.WeightKg;

            // Calculate shipping fee
            parcel.ShippingFeeVnd = priceCalculationService.
                CalculateShippingPrice(chargeableWeight, pathResponse.TotalKm);
            parcel.PriceVnd += parcel.ShippingFeeVnd;

            // Calculate insurance if required
            CalculateInsurance(parcel, categories);
        }
    }

    private void CalculateInsurance(ParcelRequest parcel, List<ParcelCategory> categories)
    {
        var category = categories.FirstOrDefault(c => c.Id == parcel.ParcelCategoryId);
        if (category.IsInsuranceRequired && !parcel.ValueVnd.HasValue)
        {
            throw new AppException(
                ErrorCode.BadRequest,
                $"Category '{category.CategoryName}' has required insurance " +
                $"and requires ValueVnd of the parcel for insurance calculation.",
                StatusCodes.Status400BadRequest);
        }

        // Calculate insurance fee if any insurance configuration exists
        if (category.InsuranceRate != null || category.InsuranceFeeVnd != null)
        {
            parcel.InsuranceFeeVnd = CalculateInsuranceFee(parcel, category);

            // Add to price only if insurance is required
            if (category.IsInsuranceRequired)
            {
                parcel.PriceVnd += parcel.InsuranceFeeVnd;
                parcel.IsInsuranceIncluded = true;
            }
        }
    }

    private decimal? CalculateInsuranceFee(ParcelRequest parcel, ParcelCategory category)
    {
        if (category.InsuranceRate != null)
        {
            return parcel.ValueVnd * category.InsuranceRate;
        }

        return category.InsuranceFeeVnd;
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
                .OrderBy(r => r.Response.TotalKm)
                .FirstOrDefault()?.Response
            : null;

        var maxDistanceInMeters = int.Parse(_systemConfigRepository
            .GetSystemConfigValueByKey(nameof(_systemConfigSetting.MAX_DISTANCE_IN_METERS)));
        response.StationsInDistanceMeter = maxDistanceInMeters;
        //response.PriceStructureDescriptionJSON = JsonSerializer.Serialize(_pricingTable);
        return response;
    }
}