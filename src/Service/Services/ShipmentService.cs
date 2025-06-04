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
using MetroShip.Service.ApiModels.Graph;
using Microsoft.AspNetCore.Identity.UI.Services;
using MetroShip.Repository.Extensions;

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
    private readonly IEmailService _emailSender;
    private readonly IUserRepository _userRepository;
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
        IEmailService emailSender,
        IUserRepository userRepository,
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
        _emailSender = emailSender;
        _userRepository = userRepository;
    }

    public async Task<PaginatedListResponse<ShipmentListResponse>> GetAllShipments(PaginatedListRequest request)
    {
        _logger.Information("Get all shipments with request: {@request}", request);
        /*var shipments = await _shipmentRepository.GetAllPaginatedQueryable(
                request.PageNumber, request.PageSize,
                x => x.DeletedAt == null
                );*/

        /*// get all stations for departure and destination
        var departureStationIds = shipments.Items.Select(x => x.DepartureStationId).Distinct().ToList();
        var destinationStationIds = shipments.Items.Select(x => x.DestinationStationId).Distinct().ToList();
        var departureStationsName = _stationRepository.GetAllWithCondition(
                       x => departureStationIds.Contains(x.Id) && x.DeletedAt == null).Select(x => new
                       {
                           // x.Id, x.Name
                           Id = x.Id,
                           Name = x.StationNameVi
                       });

        var destinationStationsName = _stationRepository.GetAllWithCondition(
            x => destinationStationIds.Contains(x.Id) && x.DeletedAt == null).Select(x => new
            {
                // x.Id, x.Name
                Id = x.Id,
                Name = x.StationNameVi
            });

        var shipmentListResponse = _mapperlyMapper.MapToShipmentListResponsePaginatedList(shipments);
        ShipmentListResponse shipmentResponse = new();
        foreach (var shipment in shipments.Items)
        {
            _logger.Information("Mapping shipment: {@shipment}", shipment);
            shipmentResponse = shipmentListResponse.Items
                .FirstOrDefault(x => x.TrackingCode == shipment.TrackingCode);

            shipmentResponse.DepartureStationName = departureStationsName
                .FirstOrDefault(x => x.Id == shipment.DepartureStationId)?.Name ?? string.Empty;

            shipmentResponse.DestinationStationName = destinationStationsName
                .FirstOrDefault(x => x.Id == shipment.DestinationStationId)?.Name ?? string.Empty;
        }*/
        var shipments = await _shipmentRepository.GetPaginatedListForListResponseAsync(
            request.PageNumber, request.PageSize);

        var shipmentListResponse = _mapperlyMapper.MapToShipmentListResponsePaginatedList(shipments);

        return shipmentListResponse;
    }

    public async Task<ShipmentDetailsResponse?> GetShipmentByTrackingCode(string trackingCode)
    {
        _logger.Information("Get shipment by tracking code: {@trackingCode}", trackingCode);
        var shipment = await _shipmentRepository.GetShipmentByTrackingCodeAsync(trackingCode);

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

        // validate shipment request
        CheckShipmentDate(request.ScheduledDateTime);
        _shipmentValidator.ValidateShipmentRequest(request);

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
        int index = 0;
        foreach (var parcel in shipment.Parcels)
        {
            parcel.ParcelCode = TrackingCodeGenerator.GenerateParcelCode(
                               shipment.TrackingCode, index);
            //parcel.QrCode = TrackingCodeGenerator.GenerateQRCode(parcel.ParcelCode);

            parcel.ParcelTrackings.Add(new ParcelTracking
            {
                ParcelId = parcel.Id,
                Status = ParcelStatusEnum.AwaitingConfirmation.ToString(),
            });

            index++;
        }

        shipment.SenderId = customerId;

        shipment = await _shipmentRepository.AddAsync(shipment, cancellationToken);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);

        // send email to customer
        /*_logger.Information("Send email to customer with tracking code: {@trackingCode}", shipment.TrackingCode);
        var user = await _userRepository.GetUserByIdAsync(customerId);
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
        }*/
        return shipment.TrackingCode;
    }

    private async Task InitializeAsync()
    {
        if (_isInitialized)
            return;

        var (routes, stations, metroLines) =
            await _shipmentItineraryRepository.GetRoutesAndStationsAsync();

        // Khởi tạo đồ thị metro
        _metroGraph = new MetroGraph(routes, stations, metroLines);
        _isInitialized = true;
    }

    public async Task<BestPathGraphResponse> FindPathAsync(BestPathRequest request)
    {
        await InitializeAsync();

        // Sử dụng đồ thị để tìm đường đi
        var path = _metroGraph.FindShortestPathByBFS(request.DepartureStationId, request.DestinationStationId);

        if (path == null || !path.Any())
            throw new AppException(
            ErrorCode.NotFound,
            ResponseMessageShipment.PATH_NOT_FOUND,
            StatusCodes.Status404NotFound);

        // Chuyển đổi đường đi thành itinerary
        return _metroGraph.CreateResponseFromPath(path, _mapperlyMapper);
    }

    public async Task<TotalPriceResponse> GetItineraryAndTotalPrice(TotalPriceCalcRequest request)
    {
        _logger.Information("Get itinerary and total price with request: {@request}", request);
        _shipmentValidator.ValidateTotalPriceCalcRequest(request);
        CheckShipmentDate(request.ScheduleShipmentDate);

        await InitializeAsync();
        List<Station> departureStations = new();
        if (request is { UserLongitude: not null, UserLatitude: not null })
        {
            // get 3 departure stations near user location
            departureStations = await _stationRepository.GetAllStationNearUser(
                request.UserLongitude.Value, request.UserLatitude.Value, 2000, 3);
        }

        // Sử dụng đồ thị để tìm đường đi
        var response = new TotalPriceResponse();
        response.NightDiscount = decimal.Parse(_systemConfigRepository
            .GetSystemConfigValueByKey(nameof(_systemConfigSetting.NIGHT_DISCOUNT)));
        response.ParcelRequests = request.Parcels;
        foreach (var parcel in request.Parcels)
        {
            parcel.ChargeableWeight = CalculateHelper.CalculateChargeableWeight(
                parcel.LengthCm, parcel.WidthCm, parcel.HeightCm, parcel.WeightKg);

            parcel.IsBulk = parcel.ChargeableWeight > parcel.WeightKg;
        }

        List<string> path;
        if (departureStations.Any())
        {
            foreach (var departureStation in departureStations)
            {
                if (_stationRepository.AreStationsInSameMetroLine(departureStation.Id, request.DestinationStationId))
                {
                    path = _metroGraph.FindShortestPathByBFS(request.DepartureStationId, request.DestinationStationId);
                }
                else
                {
                    path = _metroGraph.FindShortestPathByDijkstra(departureStation.Id, request.DestinationStationId);
                }

                if (path == null || !path.Any())
                    continue;

                response.BestPathGraphResponses.Add(_metroGraph.CreateResponseFromPath(path, _mapperlyMapper));
            }
        }
        else
        {
            if (_stationRepository.AreStationsInSameMetroLine(request.DepartureStationId, request.DestinationStationId))
            {
                path = _metroGraph.FindShortestPathByBFS(request.DepartureStationId, request.DestinationStationId);
            }
            else
            {
                path = _metroGraph.FindShortestPathByDijkstra(request.DepartureStationId, request.DestinationStationId);
            }
            response.BestPathGraphResponses.Add(_metroGraph.CreateResponseFromPath(path, _mapperlyMapper));
        }

        return response;
    }

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
}