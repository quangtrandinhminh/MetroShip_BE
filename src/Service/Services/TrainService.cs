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

        var targetDate = request.Date;

        // Query with combined Any() to improve SQL translation and performance
        var metroTrains = await _trainRepository.GetAllWithCondition(
            t => t.IsActive && t.DeletedAt == null &&
                 t.LineId == request.LineId &&
                 t.ShipmentItineraries.Any(si =>
                     si.TimeSlotId == request.TimeSlotId &&
                     si.Date.HasValue &&
                     si.Date.Value == targetDate
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

            var targetDate = request.Date.Value;

            if (request.IsAvailable.Value)
            {
                // Trains NOT assigned (AVAILABLE)
                expression = expression.And(x => !x.ShipmentItineraries.Any(si =>
                    si.TimeSlotId == request.TimeSlotId &&
                    si.Date.HasValue &&
                    si.Date.Value == targetDate &&
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
                    si.Date.Value.Equals(targetDate) &&
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
                var targetDate = request.Date.Value;
                expression = expression.And(x => x.ShipmentItineraries.Any(
                    si => si.Date.HasValue && si.Date.Value.Equals(targetDate)));
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

        /*if (train.LineId != request.LineId)
        {
            throw new AppException(ErrorCode.NotFound,
            ResponseMessageTrain.TRAIN_MUST_BE_SAME_LINE,
            StatusCodes.Status400BadRequest);
        }*/

        // Prevent double-assignment of train to same slot/date
        var targetDate = request.Date;
        var alreadyAssigned = await _trainRepository.GetAllWithCondition(
            t => t.Id == request.TrainId &&
                 t.ShipmentItineraries.Any(si =>
                     si.TimeSlotId == request.TimeSlotId &&
                     si.Date.HasValue &&
                     si.Date.Value.Equals(targetDate) &&
                        si.Route.Direction == request.Direction &&
                        si.Route.LineId == train.LineId
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
            si => si.Route.LineId == train.LineId &&
                  si.TimeSlotId == request.TimeSlotId &&
                  si.Date.HasValue &&
                  si.Date.Value.Equals(targetDate) &&
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
        int itineraryCount = shipmentItineraries.Count;
        int shipmentCount = shipmentItineraries
            .Select(si => si.ShipmentId)
            .Distinct()
            .Count();
        foreach (var itinerary in shipmentItineraries)
        {
            itinerary.TrainId = request.TrainId;
            _shipmentItineraryRepository.Update(itinerary);
        }

        // Persist changes
        var count = await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
        var response = $"Successfully added {count} changes with {itineraryCount} shipment itineraries from {shipmentCount} shipments for train {train.Id}";
        _logger.Information(response,
            count, request.TrainId);

        return response;
    }

    public async Task StartOrContinueSimulationAsync(string trainId, DirectionEnum direction)
    {


        var train = await _trainRepository.GetTrainWithRoutesAsync(trainId, direction);
        if (train == null)
            throw new AppException(ErrorCode.NotFound, "Train not found", StatusCodes.Status404NotFound);

        var routes = train.Line?.Routes?
        .Where(r => r.FromStation != null && r.ToStation != null)
        .OrderBy(r => r.SeqOrder)
        .ToList();

        if (routes == null || routes.Count == 0)
            throw new AppException(ErrorCode.NotFound, "No route data found for this train line", StatusCodes.Status404NotFound);

        var currentLeg = train.ShipmentItineraries
         .Where(si => si.Route != null && routes.Any(r => r.Id == si.Route.Id))
         .OrderBy(si => si.LegOrder)
         .FirstOrDefault(si => !si.IsCompleted);
        if (currentLeg == null)
        {
            train.Status = TrainStatusEnum.Completed;
            train.CurrentStationId = routes.Last().ToStationId;
            _trainRepository.Update(train);
            await _trainRepository.SaveChangesAsync();
            return;
        }

        // Gán lại thời gian bắt đầu mới
        currentLeg.Date = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
        train.Status = TrainStatusEnum.Departed;
        train.CurrentStationId = null; // vì đang chạy
        _trainRepository.Update(train);
        _shipmentItineraryRepository.Update(currentLeg);

        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);

        _logger.Information("🚆 Simulation resumed for Train {TrainId} at Leg {LegOrder}", trainId, currentLeg.LegOrder);
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
        // Nếu đã cache kết quả gần đây → trả luôn
        if (_cache.TryGetValue<TrainPositionResult>(trainId, out var cachedPosition))
        {
            _logger.Information("Returning cached position for train {TrainId}", trainId);
            return cachedPosition!;
        }

        var dateOffset = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
        var timeSlotId = TimeSlotHelper.GetCurrentTimeSlotId().ToString();
        var direction = DirectionEnum.Forward;

        var train = await _trainRepository.GetTrainWithItineraryAndStationsAsync(trainId, dateOffset, timeSlotId, direction)
            ?? throw new AppException(ErrorCode.NotFound, "Train not found", StatusCodes.Status404NotFound);

        var leg = train.ShipmentItineraries
            .Where(i => i.Route?.FromStation != null && i.Route.ToStation != null && !i.IsCompleted)
            .OrderBy(i => i.LegOrder)
            .FirstOrDefault()
            ?? throw new AppException(ErrorCode.NotFound, "Train has no active leg to calculate position", StatusCodes.Status404NotFound);

        var from = leg.Route.FromStation!;
        var to = leg.Route.ToStation!;

        // Lấy startTime từ cache (key theo trainId + "StartTime")
        var startTimeKey = $"{trainId}-StartTime";
        if (!_cache.TryGetValue(startTimeKey, out DateTimeOffset startTime))
        {
            // Nếu chưa có thì gán mới tại thời điểm gọi lần đầu
            startTime = DateTimeOffset.UtcNow;
            _cache.Set(startTimeKey, startTime, TimeSpan.FromMinutes(60));
        }

        var now = DateTimeOffset.UtcNow;
        var elapsed = (now - startTime).TotalSeconds;

        var distanceKm = (double)leg.Route.LengthKm;
        var speedKmh = 2000; // Tạm thời dùng fixed tốc độ
        var eta = (distanceKm / speedKmh) * 3600;
        var progress = Math.Clamp(elapsed / eta, 0, 1);

        var (lat, lng) = GeoUtils.Interpolate(
            from.Latitude!.Value, from.Longitude!.Value,
            to.Latitude!.Value, to.Longitude!.Value,
            progress);

        // Cập nhật trạng thái theo % tiến trình
        if (progress >= 1 || GeoUtils.Haversine(lat, lng, to.Latitude!.Value, to.Longitude!.Value) < 0.05)
        {
            leg.IsCompleted = true;
            train.Status = TrainStatusEnum.ArrivedAtStation;
            train.CurrentStationId = to.Id;

            _shipmentItineraryRepository.Update(leg);
            _trainRepository.Update(train);
            await _unitOfWork.SaveChangeAsync(_httpContextAccessor);

            // Gỡ khỏi cache để leg tiếp theo lấy startTime mới
            _cache.Remove(startTimeKey);
        }
        else
        {
            if (progress < 0.1)
                train.Status = TrainStatusEnum.Departed;
            else
                train.Status = TrainStatusEnum.InTransit;

            _trainRepository.Update(train);
            await _trainRepository.SaveChangesAsync();
        }

        // Chỉ vẽ tuyến giữa 2 trạm của leg hiện tại (chia thành các bước nhỏ)
        var path = new List<GeoPoint>();
        const int steps = 10;
        for (int i = 0; i <= steps; i++)
        {
            var p = i / (double)steps;
            var (stepLat, stepLng) = GeoUtils.Interpolate(
                from.Latitude.Value, from.Longitude.Value,
                to.Latitude.Value, to.Longitude.Value,
                p
            );

            path.Add(new GeoPoint
            {
                Latitude = stepLat,
                Longitude = stepLng
            });
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
            Status = train.Status.ToString(),
            Path = path
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