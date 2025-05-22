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
using MetroShip.Service.BusinessModels;
using MetroShip.Utility.Enums;
using Microsoft.AspNetCore.Http;
using MetroShip.Service.Utils;
using MetroShip.Service.Validations;
using MetroShip.Utility.Config;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Exceptions;
using MetroShip.Service.ApiModels.Graph;

namespace MetroShip.Service.Services;

public class ShipmentService : IShipmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapperlyMapper _mapperlyMapper;
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IShipmentItineraryRepository _shipmentItineraryRepository;
    private readonly ILogger _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IStationRepository _stationRepository;
    private readonly ShipmentValidator _shipmentValidator;
    private readonly ISystemConfigRepository _systemConfigRepository;
    private readonly SystemConfigSetting _systemConfigSetting;
    private bool _isInitialized = false;
    private MetroGraph _metroGraph;

    public ShipmentService(
        IServiceProvider serviceProvider, 
        IUnitOfWork unitOfWork,
        IShipmentRepository shipmentRepository,
        IShipmentItineraryRepository shipmentItineraryRepository,
        IStationRepository stationRepository,
        ISystemConfigRepository systemConfigRepository,
        ILogger logger,
        IHttpContextAccessor httpContextAccessor,
        IMapperlyMapper mapperlyMapper)
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
    }

    public async Task<PaginatedListResponse<ShipmentListResponse>> GetAllShipments(PaginatedListRequest request)
    {
        _logger.Information("Get all shipments with request: {@request}", request);
        var shipments = await _shipmentRepository.GetAllPaginatedQueryable(
                request.PageNumber, request.PageSize,
                x => x.DeletedAt == null);

        var shipmentListResponse = _mapperlyMapper.MapToShipmentListResponsePaginatedList(shipments);
        return shipmentListResponse;
    }

    public async Task<ShipmentDetailsResponse?> GetShipmentByTrackingCode(string trackingCode)
    {
        _logger.Information("Get shipment by tracking code: {@trackingCode}", trackingCode);
        var shipment = await _shipmentRepository.GetSingleAsync(
                       x => x.TrackingCode == trackingCode, false,
                       x => x.ShipmentItineraries, x => x.Transactions
                       );

        var shipmentResponse = (shipment is not null) ? _mapperlyMapper.MapToShipmentDetailsResponse(shipment) : null;
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

        var shipments = await _shipmentRepository.GetAllPaginatedQueryable(
                           request.PageNumber, request.PageSize, expression);

        var shipmentListResponse = _mapperlyMapper.MapToShipmentListResponsePaginatedList(shipments);
        return shipmentListResponse;
    }

    public async Task BookShipment(ShipmentRequest request, CancellationToken cancellationToken = default)
    {
        var customerId = JwtClaimUltils.GetUserId(_httpContextAccessor);
        _logger.Information("Book shipment with request: {@request}", request);

        // Get system config values
        var confirmationHour = int.Parse(_systemConfigRepository
            .GetSystemConfigValueByKey(nameof(_systemConfigSetting.CONFIRMATION_HOUR)));
        var paymentRequestHour = int.Parse(_systemConfigRepository
            .GetSystemConfigValueByKey(nameof(_systemConfigSetting.PAYMENT_REQUEST_HOUR)));
        var maxScheduleDay = int.Parse(_systemConfigRepository
                       .GetSystemConfigValueByKey(nameof(_systemConfigSetting.MAX_SCHEDULE_SHIPMENT_DAY)));
        var minBookDate = CoreHelper.SystemTimeNow.AddHours(confirmationHour + paymentRequestHour);
        var maxBookDate = CoreHelper.SystemTimeNow.AddDays(maxScheduleDay);

        // validate shipment request
        _shipmentValidator.ValidateShipmentRequest(request, minBookDate, maxBookDate);

        // valid parcel cate

        // get  departure station
        var departureStation = await _stationRepository.GetSingleAsync(
                       x => x.Id == request.DepartureStationId, false,
                       x => x.Region);
        if (departureStation == null)
            throw new AppException(
                               ErrorCode.NotFound,
                                              ResponseMessageShipment.DEPARTURE_STATION_NOT_FOUND,
                                                             StatusCodes.Status404NotFound);

            // map shipment request to shipment entity
            var shipment = _mapperlyMapper.MapToShipmentEntity(request);

        // quantity of booked shipment at region per date
        var quantity = _shipmentRepository.GetQuantityByBookedAtAndRegion(
            CoreHelper.SystemTimeNow.Date, departureStation.Region.RegionCode);

        // generate shipment tracking code
        shipment.TrackingCode = TrackingCodeGenerator.GenerateShipmentTrackingCode(
            departureStation.Region.RegionCode, shipment.ScheduledDateTime.Value, quantity);

        // foreach parcel, set shipment id and generate parcel code
        int index = 1;
        foreach (var parcel in shipment.Parcels)
        {
            parcel.ParcelCode = TrackingCodeGenerator.GenerateParcelCode(
                               shipment.TrackingCode, index);

            parcel.ParcelTrackings = new List<ParcelTracking>
            {
                new ParcelTracking
                {
                    ParcelId = parcel.Id,
                    Status = ParcelStatusEnum.AwaitingConfirmation.ToString(),
                }
            };
            index++;
        }

        shipment.SenderId = customerId;

        await _shipmentRepository.AddAsync(shipment, cancellationToken);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
    }

    private async Task InitializeAsync()
    {
        if (_isInitialized)
            return;

        var (routes, stations, metroLines) = await _shipmentItineraryRepository.GetRoutesAndStationsAsync();

        // Khởi tạo đồ thị metro
        _metroGraph = new MetroGraph(routes, stations, metroLines);
        _isInitialized = true;
    }

    public async Task<BestPathGraphResponse> FindPathAsync(BestPathRequest request)
    {
        await InitializeAsync();

        // Sử dụng đồ thị để tìm đường đi
        var path = _metroGraph.FindShortestPath(request.DepartureStationId, request.DestinationStationId);

        if (path == null || !path.Any())
            throw new AppException(
            ErrorCode.NotFound,
            ResponseMessageShipment.PATH_NOT_FOUND,
            StatusCodes.Status404NotFound);

        // Chuyển đổi đường đi thành itinerary
        return _metroGraph.CreateResponseFromPath(path, _mapperlyMapper);
    }

}