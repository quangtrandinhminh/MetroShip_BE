using System.Linq.Expressions;
using System.Net;
using MetroShip.Repository.Base;
using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Shipment;
using MetroShip.Service.ApiModels.Train;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Mapper;
using MetroShip.Service.Utils;
using MetroShip.Service.Validations;
using MetroShip.Utility.Config;
using MetroShip.Utility.Enums;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Exceptions;
using MetroShip.Utility.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using MetroShip.Utility.Helpers;
using SkiaSharp;
using Microsoft.Extensions.Caching.Memory;

namespace MetroShip.Service.Services;

public class TrainService(IServiceProvider serviceProvider) : ITrainService
{
    private readonly ITrainRepository _trainRepository = 
        serviceProvider.GetRequiredService<ITrainRepository>();
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
    private readonly IMapperlyMapper _mapper = serviceProvider.GetRequiredService<IMapperlyMapper>();
    private readonly IUnitOfWork _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
    private readonly ISystemConfigRepository _systemConfigRepository = 
        serviceProvider.GetRequiredService<ISystemConfigRepository>();
    private readonly IShipmentRepository _shipmentRepository =
        serviceProvider.GetRequiredService<IShipmentRepository>();
    private readonly TrainValidator _trainValidator = new();
    private readonly IHttpContextAccessor _httpContextAccessor =
        serviceProvider.GetRequiredService<IHttpContextAccessor>();
    private readonly IBaseRepository<ShipmentItinerary> _shipmentItineraryRepository =
        serviceProvider.GetRequiredService<IBaseRepository<ShipmentItinerary>>();
    private readonly IMemoryCache _cache =
        serviceProvider.GetRequiredService<IMemoryCache>();                 



    public async Task<IList<TrainCurrentCapacityResponse>> GetAllTrainsByLineSlotDateAsync(LineSlotDateFilterRequest request)
    {
        _logger.Information("Get all trains by line, slot, date with request: {@request}", request);
        _trainValidator.ValidateLineSlotDateFilterRequest(request);

        var targetDate = request.Date.Date;

        // Query with combined Any() to improve SQL translation and performance
        var metroTrains = await _trainRepository.GetAllWithCondition(
            t => t.IsActive && t.DeletedAt == null &&
                 t.LineId == request.LineId &&
                 t.ShipmentItineraries.Any(si =>
                     si.TimeSlotId == request.TimeSlotId &&
                     si.Date.HasValue &&
                     si.Date.Value.Date == targetDate
                     ),
            t => t.ShipmentItineraries
                 )
            .ToListAsync();

        // Map to response
        var response = _mapper.MapToTrainCurrentCapacityResponse(metroTrains);

        // Calculate current capacity for each train
        await CalculateCurrentCapacity(metroTrains, response);
        return response;
    }

    public async Task<PaginatedListResponse<TrainListResponse>> PaginatedListResponse(
               TrainListFilterRequest request)
    {
        _logger.Information("Get paginated list of trains with page number: {pageNumber}, page size: {pageSize}",
            request.PageNumber, request.PageSize);

        // Get paginated trains with shipment itineraries
        var paginatedList = await _trainRepository.GetAllPaginatedQueryable(
            request.PageNumber,
            request.PageSize,
            BuildShipmentFilterExpression(request),
            includeProperties: t => t.ShipmentItineraries);

        return _mapper.MapToTrainListResponsePaginatedList(paginatedList);
    }

    // get system config related to train
    public async Task<IList<object>> GetTrainSystemConfigAsync()
    {
        _logger.Information("Get train system config");
        var configKeys = new[]
        {
            nameof(SystemConfigSetting.MAX_CAPACITY_PER_LINE_M3),
            nameof(SystemConfigSetting.MAX_CAPACITY_PER_LINE_KG),
        };

        var configs = await _systemConfigRepository.GetAllSystemConfigs(ConfigKeys: configKeys);
        return [configs.Select(c => new
        {
            c.ConfigKey,
            c.ConfigValue
        }).ToList()];
    }

    public async Task<bool> IsShipmentDeliveredAsync(string trackingCode)
    {
        var shipment = await _shipmentRepository.GetShipmentByTrackingCodeAsync(trackingCode);
        return shipment.ShipmentStatus == ShipmentStatusEnum.Completed;
    }

    public async Task<IList<string>> GetTrackingCodesByTrainAsync(string trainId)
    {
        var shipments = await _trainRepository.GetShipmentsByTrainAsync(trainId);
        var trackingCodes = shipments.Select(s => s.TrackingCode).ToList();

        // Log the tracking codes  
        _logger.Information("Tracking codes for train {TrainId}: {@TrackingCodes}", trainId, trackingCodes);

        return trackingCodes;
    }

    public async Task UpdateTrainLocationAsync(string trainId, double lat, double lng, string stationId)
    {
        await _trainRepository.SaveTrainLocationAsync(trainId, lat, lng, stationId);
    }
    private Expression<Func<MetroTrain, bool>> BuildShipmentFilterExpression(TrainListFilterRequest request)
    {
        _logger.Information("Building filter expression for request: {@request}", request);
        _trainValidator.ValidateTrainListFilterRequest(request);

        Expression<Func<MetroTrain, bool>> expression = x => x.DeletedAt == null;

        // 1. If IsAvailable is provided, require all fields
        if (request.IsAvailable.HasValue)
        {
            // Must provide all required fields if IsAvailable is set
            if (string.IsNullOrEmpty(request.LineId) ||
                string.IsNullOrEmpty(request.TimeSlotId) ||
                !request.Date.HasValue ||
                !request.Direction.HasValue)
            {
                throw new AppException(ErrorCode.BadRequest,
                    $"{nameof(request.LineId)}, {nameof(request.TimeSlotId)}, {nameof(request.Date)}, and {nameof(request.Direction)} must be provided when IsAvailable is set.",
                    StatusCodes.Status400BadRequest
                );
            }

            var targetDate = CoreHelper.UtcToSystemTime(request.Date.Value).Date;

            if (request.IsAvailable.Value)
            {
                // Trains NOT assigned (AVAILABLE)
                expression = expression.And(x => !x.ShipmentItineraries.Any(si =>
                    si.TimeSlotId == request.TimeSlotId &&
                    si.Date.HasValue &&
                    si.Date.Value.Date == targetDate &&
                    si.Route.Direction == request.Direction &&
                    si.Route.LineId == request.LineId
                ));
            }
            else
            {
                // Trains assigned (NOT AVAILABLE)
                expression = expression.And(x => x.ShipmentItineraries.Any(si =>
                    si.TimeSlotId == request.TimeSlotId &&
                    si.Date.HasValue &&
                    si.Date.Value.Date == targetDate &&
                    si.Route.Direction == request.Direction &&
                    si.Route.LineId == request.LineId
                ));
            }
        }
        else // 2. If IsAvailable is not provided, use any individual filter supplied
        {
            if (!string.IsNullOrEmpty(request.LineId))
            {
                expression = expression.And(x => x.LineId == request.LineId);
            }
            if (!string.IsNullOrEmpty(request.ModelName))
            {
                // search string in postgres
                expression = expression.And(x => 
                EF.Functions.ILike(x.ModelName, $"%{request.ModelName}%"));
            }
            if (!string.IsNullOrEmpty(request.TimeSlotId))
            {
                expression = expression.And(x => x.ShipmentItineraries.Any(si => si.TimeSlotId == request.TimeSlotId));
            }
            if (request.Date.HasValue)
            {
                var targetDate = CoreHelper.UtcToSystemTime(request.Date.Value).Date;
                expression = expression.And(x => x.ShipmentItineraries.Any(
                    si => si.Date.HasValue && si.Date.Value.Date == targetDate));
            }
            if (request.Direction.HasValue)
            {
                expression = expression.And(x => x.ShipmentItineraries.Any(
                    si => si.Route.Direction == request.Direction));
            }
        }

        return expression;
    }

    public async Task<string> AddShipmentItinerariesForTrain(AddTrainToItinerariesRequest request)
    {
        _logger.Information("Adding shipment itineraries for train with request: {@request}", request);
        _trainValidator.ValidateAddTrainToItinerariesRequest(request);

        // Ensure train exists
        var train = await _trainRepository.GetByIdAsync(request.TrainId);
        if (train == null)
        {
            throw new AppException(ErrorCode.NotFound,
                ResponseMessageTrain.TRAIN_NOT_FOUND,
                StatusCodes.Status400BadRequest);
        }

        if (train.LineId != request.LineId)
        {
            throw new AppException(ErrorCode.NotFound,
            ResponseMessageTrain.TRAIN_MUST_BE_SAME_LINE,
            StatusCodes.Status400BadRequest);
        }

        // Prevent double-assignment of train to same slot/date
        var targetDate = request.Date.Date;
        var alreadyAssigned = await _trainRepository.GetAllWithCondition(
            t => t.Id == request.TrainId &&
                 t.ShipmentItineraries.Any(si =>
                     si.TimeSlotId == request.TimeSlotId &&
                     si.Date.HasValue &&
                     si.Date.Value.Date == targetDate &&
                        si.Route.Direction == request.Direction
                     ))
            .AnyAsync();

        if (alreadyAssigned)
        {
            throw new AppException(ErrorCode.BadRequest,
                ResponseMessageTrain.TRAIN_ALREADY_ASSIGNED_TO_SLOT_ON_DATE,
                StatusCodes.Status400BadRequest);
        }

        // ensure direction of previous shift itineraries not same as request direction
        // coming soon ...

        // Fetch all shipment itineraries for the line, slot, date, direction, and train id null
        var shipmentItineraries = await _shipmentItineraryRepository.GetAllWithCondition(
            si => si.Route.LineId == request.LineId &&
                  si.TimeSlotId == request.TimeSlotId &&
                  si.Date.HasValue &&
                  si.Date.Value.Date == targetDate &&
                  si.Route.Direction == request.Direction &&
                  si.TrainId == null && // Only consider itineraries not yet assigned to a train
                  (si.Shipment.ShipmentStatus == ShipmentStatusEnum.AwaitingDropOff ||
                    si.Shipment.ShipmentStatus == ShipmentStatusEnum.AwaitingPayment ||
                    si.Shipment.ShipmentStatus == ShipmentStatusEnum.InTransit )
                  )
            .ToListAsync();

        if (!shipmentItineraries.Any())
        {
            throw new AppException(ErrorCode.NotFound,
                ResponseMessageTrain.SHIPMENT_ITINERARIES_NOT_FOUND,
                StatusCodes.Status400BadRequest);
        }

        // Assign train to each itinerary
        foreach (var itinerary in shipmentItineraries)
        {
            itinerary.TrainId = request.TrainId;
            _shipmentItineraryRepository.Update(itinerary);
        }

        // Persist changes
        var count = await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
        var response = $"Successfully added {count} shipment itineraries for train {train.Id}";
        _logger.Information(response,
            count, request.TrainId);

        return response;
    }

    public async Task StartOrContinueSimulationAsync(string trainId)
    {
        var train = await _trainRepository.GetTrainWithItineraryAndStationsAsync(trainId);
        if (train == null)
            throw new AppException(ErrorCode.NotFound, "Train not found", StatusCodes.Status404NotFound);

        var itineraries = train.ShipmentItineraries?
            .Where(i => i.Route?.FromStation != null && i.Route.ToStation != null)
            .OrderBy(i => i.LegOrder)
            .ToList();

        if (itineraries == null || itineraries.Count == 0)
            throw new AppException(ErrorCode.NotFound, "No valid itinerary found", StatusCodes.Status404NotFound);

        var currentLeg = itineraries.FirstOrDefault(i => !i.IsCompleted);
        if (currentLeg == null)
        {
            train.Status = TrainStatusEnum.Completed;
            _trainRepository.Update(train);
            await _trainRepository.SaveChangesAsync();
            return;
        }

        var from = currentLeg.Route.FromStation!;
        var to = currentLeg.Route.ToStation!;
        var distanceKm = GeoUtils.Haversine(from.Latitude!.Value, from.Longitude!.Value, to.Latitude!.Value, to.Longitude!.Value);
        var speedKmh = train.TopSpeedKmH ?? 40;
        var etaSeconds = (distanceKm / speedKmh) * 3600;

        currentLeg.Date = DateTimeOffset.UtcNow;
        train.Status = TrainStatusEnum.Departed;
        _trainRepository.Update(train);
        await _trainRepository.SaveChangesAsync();

        _logger.Information("🚆 Simulation started for Train {TrainId}, ETA: {Eta}s", trainId, etaSeconds);
    }

    private static ShipmentStatusEnum MapTrainToShipmentStatus(TrainStatusEnum trainStatus)
    {
        return trainStatus switch
        {
            TrainStatusEnum.Completed => ShipmentStatusEnum.Completed,
            TrainStatusEnum.AwaitingDeparture => ShipmentStatusEnum.WaitingForNextTrain,
            TrainStatusEnum.Departed => ShipmentStatusEnum.InTransit,
            TrainStatusEnum.InTransit => ShipmentStatusEnum.InTransit,
            TrainStatusEnum.ArrivedAtStation => ShipmentStatusEnum.UnloadingAtStation,
            TrainStatusEnum.ResumingTransit => ShipmentStatusEnum.TransferringToNextTrain,
            _ => ShipmentStatusEnum.InTransit // fallback
        };
    }

    private async Task CalculateCurrentCapacity(IList<MetroTrain> metroTrains, IList<TrainCurrentCapacityResponse> response)
    {
        _logger.Information("Calculating current capacity for trains");

        // Get all unique shipment IDs from the trains
        var shipmentIds = metroTrains
            .SelectMany(t => t.ShipmentItineraries)
            .Select(si => si.ShipmentId)
            .Distinct()
            .ToList();

        // Fetch shipment data in one query
        var shipmentData = await _shipmentRepository.GetAllWithCondition(
                       s => shipmentIds.Contains(s.Id))
            .Select(s => new
            {
                s.Id,
                s.TotalVolumeM3,
                s.TotalWeightKg
            })
            .ToListAsync();

        // Create a lookup dictionary for better performance
        var shipmentLookup = shipmentData.ToDictionary(
            s => s.Id);

        // Calculate current weight and volume for each train
        foreach (var train in response)
        {
            var trainEntity = metroTrains.FirstOrDefault(t => t.Id == train.Id);

            if (trainEntity?.ShipmentItineraries != null)
            {
                var trainShipmentIds = trainEntity.ShipmentItineraries
                    .Select(si => si.ShipmentId)
                    .Where(shipmentLookup.ContainsKey)
                    .ToList();

                if (trainShipmentIds.Any())
                {
                    train.CurrentWeightKg = trainShipmentIds.Sum(id => shipmentLookup[id].TotalWeightKg ?? 0);
                    train.CurrentVolumeM3 = trainShipmentIds.Sum(id => shipmentLookup[id].TotalVolumeM3 ?? 0);
                }
            }
        }
    }

    // for getting train position based on trainId
    public async Task<TrainPositionResult> GetTrainPositionAsync(string trainId)
    {
        if (_cache.TryGetValue<TrainPositionResult>(trainId, out var cachedPosition))
        {
            return cachedPosition!;
        }

        // narrow query lại trong 1 ca, ngày, hướng cụ thể
        // thêm request body cụ thể là date,timeslot, direction nào
        // lúc include shipment itineraries thì phải include route, where shipment itineraries có date, timeslot, route.direction như vậy
        // có thể thêm where i => !i.IsCompleted ở đây luôn
        //var train = await _trainRepository.GetTrainWithItineraryAndStationsAsync(trainId).ConfigureAwait(false);

        // lấy từ request
        // hôm nay +07:00
        var date = CoreHelper.SystemTimeNow.Date;
        // convert còn mỗi date +00:00
        var dateOffset = new DateTimeOffset(2025, 07, 31, 0, 0, 0, TimeSpan.Zero);
        // ca sáng
        var timeSlotId = "a1b2c3d4-e5f6-7a8b-9c0d-e1f2a3b4c5d6"; // lấy từ request
        // hướng đi
        var direction = DirectionEnum.Forward; // lấy từ request
        var train = await _trainRepository.GetTrainWithItineraryAndStationsAsync(trainId, dateOffset, timeSlotId, direction)
            .ConfigureAwait(false);
        if (train == null)
            throw new AppException(ErrorCode.NotFound, "Train not found", StatusCodes.Status404NotFound);

        // khúc này lấy order by legOrder xong lấy first
        /*var leg = train.ShipmentItineraries?
            .Where(i => i.Route?.FromStation != null && i.Route.ToStation != null)
            .OrderBy(i => i.LegOrder)
            .FirstOrDefault(i => !i.IsCompleted);*/

        var leg = train.ShipmentItineraries
            .Where(i => i.Route?.FromStation != null && i.Route.ToStation != null)
            .OrderBy(i => i.LegOrder)
            .FirstOrDefault();

        if (leg == null)
            throw new AppException(ErrorCode.NotFound, "Train was not dispatched for any itineraries", StatusCodes.Status404NotFound);

        var from = leg.Route.FromStation!;
        var to = leg.Route.ToStation!;

        // Để chuẩn, startTime nên được gửi từ request mỗi khi tàu dừng lại và bắt đầu đi tiếp
        var startTime = leg.Date ?? DateTimeOffset.UtcNow;
        var now = new DateTimeOffset(2025, 07, 31, 0, 45, 0, TimeSpan.Zero);


        // tính toán đúng khi 2 datetimeoffset cùng timeSpan 
        //var elapsed = (DateTimeOffset.UtcNow - startTime).TotalSeconds;
        var elapsed = (now - startTime).TotalSeconds;
        //var distanceKm = GeoUtils.Haversine(from.Latitude!.Value, from.Longitude!.Value, to.Latitude!.Value, to.Longitude!.Value);
        // Chiều dài thực của routeStation
        var distanceKm = (double) leg.Route.LengthKm;
        //var speedKmh = train.TopSpeedKmH ?? 40;
        // chạy cho lẹ
        var speedKmh = 2000;
        var eta = (distanceKm / speedKmh) * 3600;

        var progress = Math.Clamp(elapsed / eta, 0, 1);
        var (lat, lng) = GeoUtils.Interpolate(from.Latitude.Value, from.Longitude.Value, to.Latitude.Value, to.Longitude.Value, progress);

        // Nếu progress = 1 thì có thể là đã đến trạm, update trạng thái itinerary
        if (progress >= 1
            || GeoUtils.Haversine(from.Latitude!.Value, from.Longitude!.Value, to.Latitude!.Value, to.Longitude!.Value) < 0.1 
            // khoảng cách nhỏ hơn 100m thì coi như đã đến trạm, progress chuẩn có thể bỏ haversine
            )
        {
            leg.IsCompleted = true;
            _shipmentItineraryRepository.Update(leg);
            await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
        }

        var result = new TrainPositionResult
        {
            TrainId = trainId,
            Latitude = lat,
            Longitude = lng,
            StartTime = startTime,
            ETA = TimeSpan.FromSeconds(eta),
            Elapsed = TimeSpan.FromSeconds(elapsed),
            ProgressPercent = (int)(progress * 100),
            FromStation = from.StationNameVi,
            ToStation = to.StationNameVi,
            Status = train.Status.ToString()
        };

        _cache.Set(trainId, result, TimeSpan.FromSeconds(5));
        return result;
    }

    public async Task<TrainPositionResult> GetTrainPositionByTrackingCodeAsync(string trackingCode)
    {
        var shipment = await _trainRepository.GetShipmentWithTrainAsync(trackingCode);
        if (shipment == null || shipment.ShipmentItineraries == null)
            throw new AppException(ErrorCode.NotFound, "Shipment not found", StatusCodes.Status404NotFound);

        var itinerary = shipment.ShipmentItineraries.FirstOrDefault(x => x.TrainId != null);
        if (itinerary?.TrainId == null)
            throw new AppException(ErrorCode.NotFound, "Train not assigned", StatusCodes.Status404NotFound);

        return await GetTrainPositionAsync(itinerary.TrainId);
    }
}