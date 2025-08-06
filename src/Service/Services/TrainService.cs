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
    private readonly IShipmentTrackingRepository _shipmentTrackingRepository =
        serviceProvider.GetRequiredService<IShipmentTrackingRepository>();
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

    public async Task StartOrContinueSimulationAsync(string trainId)
    {
        var train = await _trainRepository.GetTrainWithAllRoutesAsync(trainId)
            ?? throw new AppException(ErrorCode.NotFound, "Train not found", StatusCodes.Status404NotFound);

        if (train.Line?.Routes == null || train.Line.Routes.Count == 0)
            throw new AppException(ErrorCode.NotFound, "No routes found for train", StatusCodes.Status404NotFound);

        var allRoutes = train.Line.Routes
            .Where(r => r.FromStation != null && r.ToStation != null)
            .ToList();

        var segmentKey = $"{trainId}-SegmentIndex";
        var directionKey = $"{trainId}-Direction";

        var currentIndex = _cache.TryGetValue(segmentKey, out int existingIndex) ? existingIndex : -1;

        // ✅ Lấy direction từ cache hoặc đoán
        var direction = _cache.TryGetValue(directionKey, out DirectionEnum cachedDirection)
            ? cachedDirection
            : InferTrainDirectionFromCurrentStation(train, train.CurrentStationId ?? throw new AppException(ErrorCode.BadRequest, "Train has no current station", StatusCodes.Status400BadRequest));

        // 🔁 Kiểm tra nếu đang ở đầu của chiều ngược lại thì đổi chiều luôn
        var reverseDirection = direction == DirectionEnum.Forward ? DirectionEnum.Backward : DirectionEnum.Forward;
        var reverseRoutes = allRoutes
            .Where(r => r.Direction == reverseDirection)
            .OrderBy(r => r.SeqOrder)
            .ToList();

        var reverseStartStationId = reverseRoutes.FirstOrDefault()?.FromStationId;

        if (train.Status == TrainStatusEnum.Completed && train.CurrentStationId == reverseStartStationId)
        {
            direction = reverseDirection;
            currentIndex = -1;

            _cache.Set(directionKey, direction, TimeSpan.FromHours(1));
            _cache.Set(segmentKey, currentIndex, TimeSpan.FromHours(1));

            _logger.Information("🔁 Train {TrainId} auto reversed to direction {Direction}", trainId, direction);
        }

        // ✅ Lấy route theo direction sau khi xác định lại
        var routes = allRoutes
            .Where(r => r.Direction == direction)
            .OrderBy(r => r.SeqOrder)
            .ToList();

        if (routes.Count == 0)
            throw new AppException(ErrorCode.NotFound, $"No routes found for direction {direction}", StatusCodes.Status404NotFound);

        // ❌ Không cho phép chạy nếu đang chạy
        if (train.Status == TrainStatusEnum.InTransit || train.Status == TrainStatusEnum.Departed)
            throw new AppException(ErrorCode.BadRequest, "Train is already running", StatusCodes.Status400BadRequest);

        // ✅ Nếu vừa completed → kiểm tra xem có ở cuối tuyến cũ không
        if (train.Status == TrainStatusEnum.Completed && currentIndex != -1)
        {
            var lastRoute = routes.Last();
            var expectedStationId = lastRoute.ToStationId;
            var expectedLat = lastRoute.ToStation?.Latitude;
            var expectedLng = lastRoute.ToStation?.Longitude;

            if (train.CurrentStationId != expectedStationId ||
                !IsSameCoordinate(train.Latitude, train.Longitude, expectedLat, expectedLng))
            {
                throw new AppException(ErrorCode.BadRequest, $"Train must be at end of direction {direction} to restart", StatusCodes.Status400BadRequest);
            }

            currentIndex = -1;
        }

        // ✅ Nếu vừa Arrived → cập nhật lại vị trí
        if (train.Status == TrainStatusEnum.ArrivedAtStation && existingIndex >= 0 && existingIndex < routes.Count)
        {
            var prevRoute = routes[existingIndex];
            train.CurrentStationId = prevRoute.ToStationId;
            train.Latitude = prevRoute.ToStation?.Latitude;
            train.Longitude = prevRoute.ToStation?.Longitude;

            _trainRepository.Update(train);
            await _trainRepository.SaveChangesAsync();
        }

        var nextIndex = currentIndex + 1;

        // ✅ Nếu hết tuyến → đánh dấu completed
        if (nextIndex >= routes.Count)
        {
            var lastRoute = routes.Last();
            train.Status = TrainStatusEnum.Completed;
            train.CurrentStationId = lastRoute.ToStationId;
            train.Latitude = lastRoute.ToStation?.Latitude;
            train.Longitude = lastRoute.ToStation?.Longitude;

            _trainRepository.Update(train);
            await _trainRepository.SaveChangesAsync();

            _cache.Remove(segmentKey);
            _cache.Remove($"{trainId}-StartTime");

            _logger.Information("✅ Train {TrainId} completed direction {Direction}.", trainId, direction);
            return;
        }

        // ✅ Bắt đầu leg tiếp theo
        var nextRoute = routes[nextIndex];

        _cache.Set(segmentKey, nextIndex, TimeSpan.FromHours(1));
        _cache.Set(directionKey, direction, TimeSpan.FromHours(1));
        _cache.Set($"{trainId}-StartTime", DateTimeOffset.UtcNow, TimeSpan.FromHours(1));

        train.Status = TrainStatusEnum.Departed;
        train.CurrentStationId = null;

        _trainRepository.Update(train);
        await _trainRepository.SaveChangesAsync();

        _logger.Information("🚆 Train {TrainId} started leg {Index} from {From} to {To} (Direction: {Direction})",
            trainId,
            nextIndex + 1,
            nextRoute.FromStation?.StationNameVi ?? "Unknown",
            nextRoute.ToStation?.StationNameVi ?? "Unknown",
            direction);
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
            return cachedPosition!;

        // ✅ Lấy direction từ cache (giống simulation)
        var directionKey = $"{trainId}-Direction";
        if (!_cache.TryGetValue(directionKey, out DirectionEnum direction))
            throw new AppException(ErrorCode.BadRequest, "Train direction not initialized. Call StartOrContinueSimulationAsync first.", StatusCodes.Status400BadRequest);

        var train = await _trainRepository.GetTrainWithRoutesAsync(trainId, direction)
            ?? throw new AppException(ErrorCode.NotFound, "Train not found", StatusCodes.Status404NotFound);

        var routes = train.Line?.Routes?
            .Where(r => r.FromStation != null && r.ToStation != null && r.Direction == direction)
            .OrderBy(r => r.SeqOrder)
            .ToList();

        if (routes == null || routes.Count == 0)
            throw new AppException(ErrorCode.NotFound, "No route data found", StatusCodes.Status404NotFound);

        var segmentKey = $"{trainId}-SegmentIndex";
        if (!_cache.TryGetValue(segmentKey, out int currentIndex))
            throw new AppException(ErrorCode.BadRequest, "Train segment not initialized. Call StartOrContinueSimulationAsync.", StatusCodes.Status400BadRequest);

        // ✅ Check index valid
        if (currentIndex < 0 || currentIndex >= routes.Count)
            throw new AppException(ErrorCode.BadRequest, "Train segment index out of range.", StatusCodes.Status400BadRequest);

        var currentRoute = routes[currentIndex];
        var from = currentRoute.FromStation!;
        var to = currentRoute.ToStation!;

        var startTimeKey = $"{trainId}-StartTime";
        if (!_cache.TryGetValue(startTimeKey, out DateTimeOffset startTime))
            throw new AppException(ErrorCode.BadRequest, "Start time not initialized. Call simulation start first.", StatusCodes.Status400BadRequest);

        var now = DateTimeOffset.UtcNow;
        var elapsed = (now - startTime).TotalSeconds;
        var distanceKm = (double)currentRoute.LengthKm;
        var speedKmh = 100;
        var eta = (distanceKm / speedKmh) * 3600;
        var progress = Math.Clamp(elapsed / eta, 0, 1);

        var (lat, lng) = GeoUtils.Interpolate(
            from.Latitude!.Value, from.Longitude!.Value,
            to.Latitude!.Value, to.Longitude!.Value,
            progress);

        train.Status = progress < 0.1
            ? TrainStatusEnum.Departed
            : TrainStatusEnum.InTransit;

        train.Latitude = lat;
        train.Longitude = lng;

        _trainRepository.Update(train);
        await _trainRepository.SaveChangesAsync();

        // Tạo path animation
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

        // Tạo toàn bộ tuyến đường
        var routePath = routes.Select(r => new
        {
            FromStation = new
            {
                r.FromStation.StationNameVi,
                r.FromStation.Latitude,
                r.FromStation.Longitude
            },
            ToStation = new
            {
                r.ToStation.StationNameVi,
                r.ToStation.Latitude,
                r.ToStation.Longitude
            },
            r.SeqOrder,
            r.Direction
        }).ToList();

        // Trả kết quả
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
            Path = path,
            AdditionalData = new
            {
                RoutePath = routePath
            }
        };

        _cache.Set(trainId, result, TimeSpan.FromSeconds(1));
        return result;
    }

    public async Task ConfirmTrainArrivedAsync(string trainId, string stationId)
    {
        // Lấy toàn bộ thông tin tuyến (bao gồm tất cả direction)
        var train = await _trainRepository.GetTrainWithAllRoutesAsync(trainId)
            ?? throw new AppException(ErrorCode.NotFound, "Train not found", StatusCodes.Status404NotFound);

        if (train.Line == null || train.Line.Routes == null || !train.Line.Routes.Any())
            throw new AppException(ErrorCode.NotFound, "No route information found for this train's line", StatusCodes.Status404NotFound);

        // Kiểm tra trạm có thuộc tuyến không
        var allStationIds = train.Line.Routes
            .SelectMany(r => new[] { r.FromStationId, r.ToStationId })
            .Where(id => id != null)
            .Distinct()
            .ToList();

        if (!allStationIds.Contains(stationId))
            throw new AppException(ErrorCode.BadRequest, "Station does not belong to this train's line", StatusCodes.Status400BadRequest);

        // Lấy tất cả các tuyến theo direction hiện tại của tàu
        var currentDirection = InferTrainDirectionFromCurrentStation(train, stationId);

        var directionRoutes = train.Line.Routes
            .Where(r => r.Direction == currentDirection)
            .OrderBy(r => r.SeqOrder)
            .ToList();

        if (directionRoutes.Count == 0)
            throw new AppException(ErrorCode.BadRequest, "No routes found for current direction", StatusCodes.Status400BadRequest);

        // Lấy toStationId cuối cùng
        var lastStationId = directionRoutes.Last().ToStationId;

        // So sánh để xác định trạng thái
        if (stationId == lastStationId)
        {
            train.Status = TrainStatusEnum.Completed;
            _logger.Information("✅ Train {TrainId} completed its journey at station {StationId}", trainId, stationId);
        }
        else
        {
            train.Status = TrainStatusEnum.ArrivedAtStation;
            _logger.Information("🚉 Train {TrainId} arrived at station {StationId}", trainId, stationId);
        }

        train.CurrentStationId = stationId;

        _trainRepository.Update(train);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
    }

    private DirectionEnum InferTrainDirectionFromCurrentStation(MetroTrain train, string stationId)
    {
        var forwardStart = train.Line!.Routes!
            .Where(r => r.Direction == DirectionEnum.Forward)
            .OrderBy(r => r.SeqOrder)
            .FirstOrDefault()?.FromStationId;

        var backwardStart = train.Line!.Routes!
            .Where(r => r.Direction == DirectionEnum.Backward)
            .OrderBy(r => r.SeqOrder)
            .FirstOrDefault()?.FromStationId;

        if (stationId == forwardStart)
            return DirectionEnum.Forward;

        if (stationId == backwardStart)
            return DirectionEnum.Backward;

        throw new AppException(ErrorCode.BadRequest, "Cannot determine direction from current station", StatusCodes.Status400BadRequest);
    }

    private bool IsSameCoordinate(double? lat1, double? lng1, double? lat2, double? lng2, double threshold = 0.0001)
    {
        return lat1.HasValue && lng1.HasValue && lat2.HasValue && lng2.HasValue &&
               Math.Abs(lat1.Value - lat2.Value) < threshold &&
               Math.Abs(lng1.Value - lng2.Value) < threshold;
    }

    public async Task<TrainPositionResult> GetTrainPositionByTrackingCodeAsync(string trackingCode)
    {
        var shipment = await _trainRepository.GetShipmentWithTrainAsync(trackingCode);

        if (shipment == null || shipment.ShipmentItineraries == null)
            throw new AppException(ErrorCode.NotFound, "Shipment not found", StatusCodes.Status404NotFound);

        var itinerary = shipment.ShipmentItineraries
            .FirstOrDefault(i => i.TrainId != null && i.ShipmentId == shipment.Id);

        if (itinerary?.TrainId == null)
            throw new AppException(ErrorCode.NotFound, "Train not assigned", StatusCodes.Status404NotFound);

        // ✅ Nếu chưa gọi Start simulation → chưa tracking
        if (!_cache.TryGetValue($"{itinerary.TrainId}-SegmentIndex", out int _))
            throw new AppException(ErrorCode.BadRequest, "Train has not started simulation.", StatusCodes.Status400BadRequest);

        // ❌ Không tracking nếu shipment chưa lên tàu
        if (shipment.ShipmentStatus != ShipmentStatusEnum.LoadOnMetro &&
            shipment.ShipmentStatus != ShipmentStatusEnum.InTransit &&
            shipment.ShipmentStatus != ShipmentStatusEnum.AwaitingDelivery)
        {
            throw new AppException(ErrorCode.BadRequest, "Shipment not ready for tracking", StatusCodes.Status400BadRequest);
        }

        // Lấy trạng thái tàu thật sự
        var position = await GetTrainPositionAsync(itinerary.TrainId);

        var rawTrainStatus = Enum.Parse<TrainStatusEnum>(position.Status);
        var mappedShipmentStatus = MapTrainStatusToShipmentStatus(rawTrainStatus);

        // ✅ Nếu shipment status thay đổi → update DB + log tracking
        if (shipment.ShipmentStatus != mappedShipmentStatus)
        {
            shipment.ShipmentStatus = mappedShipmentStatus;

            _shipmentTrackingRepository.Add(new ShipmentTracking
            {
                ShipmentId = shipment.Id,
                CurrentShipmentStatus = mappedShipmentStatus,
                Status = mappedShipmentStatus.ToString(),
                EventTime = DateTimeOffset.UtcNow,
                Note = $"Shipment moved to status: {mappedShipmentStatus}"
            });

            _shipmentRepository.Update(shipment);
            await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
        }

        // ✅ Đánh dấu leg hiện tại là hoàn thành nếu tàu đã đến trạm
        var currentLeg = shipment.ShipmentItineraries
            .OrderBy(i => i.LegOrder)
            .FirstOrDefault(i => !i.IsCompleted);

        if (currentLeg != null && rawTrainStatus == TrainStatusEnum.ArrivedAtStation)
        {
            currentLeg.IsCompleted = true;

            var toStationName = currentLeg.Route?.ToStation?.StationNameVi ?? "Unknown";
            var messageLine = $"[Arrived at {toStationName} - {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm}]";

            currentLeg.Message = string.IsNullOrEmpty(currentLeg.Message)
                ? messageLine
                : $"{currentLeg.Message}\n{messageLine}";

            _shipmentItineraryRepository.Update(currentLeg);
            await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
        }

        // ✅ Nếu tất cả leg đã hoàn tất → shipment completed
        if (shipment.ShipmentItineraries.All(i => i.IsCompleted))
        {
            shipment.ShipmentStatus = ShipmentStatusEnum.Completed;

            _shipmentTrackingRepository.Add(new ShipmentTracking
            {
                ShipmentId = shipment.Id,
                CurrentShipmentStatus = ShipmentStatusEnum.Completed,
                Status = "Completed",
                EventTime = DateTimeOffset.UtcNow,
                Note = "All legs completed. Shipment delivered."
            });

            _shipmentRepository.Update(shipment);
            await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
        }

        // ✅ Tạo toàn bộ hành trình (path) theo các leg
        var fullPath = shipment.ShipmentItineraries
            .OrderBy(i => i.LegOrder)
            .Where(i => i.Route?.FromStation != null && i.Route?.ToStation != null)
            .SelectMany(i =>
            {
                const int steps = 10;
                var from = i.Route.FromStation!;
                var to = i.Route.ToStation!;
                return Enumerable.Range(0, steps + 1)
                    .Select(s =>
                    {
                        var p = s / (double)steps;
                        var (lat, lng) = GeoUtils.Interpolate(
                            from.Latitude!.Value, from.Longitude!.Value,
                            to.Latitude!.Value, to.Longitude!.Value,
                            p);
                        return new GeoPoint { Latitude = lat, Longitude = lng };
                    });
            })
            .ToList();

        // ✅ Định nghĩa kết quả trả về
        position.Status = mappedShipmentStatus.ToString();

        position.AdditionalData = new
        {
            RawTrainStatus = rawTrainStatus.ToString(),
            Shipment = new
            {
                shipment.Id,
                shipment.TrackingCode,
                shipment.SenderName,
                shipment.DestinationStationId,
                shipment.ShipmentStatus,
                shipment.TotalWeightKg,
                shipment.TotalVolumeM3,
                shipment.CreatedAt,

                ShipmentItineraries = shipment.ShipmentItineraries
                .OrderBy(x => x.LegOrder)
                .Select(x => new
                {
                    x.LegOrder,
                    From = new
                    {
                        Name = x.Route.FromStation.StationNameVi,
                        Latitude = x.Route.FromStation.Latitude,
                        Longitude = x.Route.FromStation.Longitude
                    },
                    To = new
                    {
                        Name = x.Route.ToStation.StationNameVi,
                        Latitude = x.Route.ToStation.Latitude,
                        Longitude = x.Route.ToStation.Longitude
                    },
                    x.IsCompleted,
                    x.Message
                })
                .ToList(),

                FullPath = fullPath
            }
        };

        return position;
    }

    private ShipmentStatusEnum MapTrainStatusToShipmentStatus(TrainStatusEnum trainStatus)
    {
        return trainStatus switch
        {
            TrainStatusEnum.Departed => ShipmentStatusEnum.InTransit,
            TrainStatusEnum.InTransit => ShipmentStatusEnum.InTransit,
            TrainStatusEnum.ArrivedAtStation => ShipmentStatusEnum.AwaitingDelivery,
            _ => ShipmentStatusEnum.AwaitingDelivery // Default case
        };
    }
}