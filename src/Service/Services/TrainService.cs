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

    public async Task StartOrContinueSimulationAsync(string trainId, DirectionEnum direction)
    {
        var train = await _trainRepository.GetTrainWithRoutesAsync(trainId, direction)
            ?? throw new AppException(ErrorCode.NotFound, "Train not found", StatusCodes.Status404NotFound);

        var routes = train.Line?.Routes?
            .Where(r => r.FromStation != null && r.ToStation != null && r.Direction == direction)
            .OrderBy(r => r.SeqOrder)
            .ToList();

        if (routes == null || routes.Count == 0)
            throw new AppException(ErrorCode.NotFound, "No routes found for train", StatusCodes.Status404NotFound);

        var segmentKey = $"{trainId}-SegmentIndex";
        var currentIndex = _cache.TryGetValue(segmentKey, out int existingIndex) ? existingIndex : -1;

        var nextIndex = currentIndex + 1;
        if (nextIndex >= routes.Count)
        {
            train.Status = TrainStatusEnum.Completed;
            train.CurrentStationId = routes.Last().ToStationId;
            _trainRepository.Update(train);
            await _trainRepository.SaveChangesAsync();
            _cache.Remove(segmentKey);
            return;
        }

        // Cập nhật cache và trạng thái
        _cache.Set(segmentKey, nextIndex, TimeSpan.FromHours(1));

        train.Status = TrainStatusEnum.Departed;
        train.CurrentStationId = null;

        _trainRepository.Update(train);
        await _trainRepository.SaveChangesAsync();

        // Cache start time cho tracking leg
        _cache.Set($"{trainId}-StartTime", DateTimeOffset.UtcNow, TimeSpan.FromHours(1));

        _logger.Information("🚆 Train {TrainId} started at leg index {Index}", trainId, nextIndex);
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

        var direction = DirectionEnum.Forward;

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

        if (currentIndex >= routes.Count)
            throw new AppException(ErrorCode.BadRequest, "Train has completed all segments.", StatusCodes.Status400BadRequest);

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

        if (progress >= 1 || GeoUtils.Haversine(lat, lng, to.Latitude!.Value, to.Longitude!.Value) < 0.05)
        {
            train.Status = currentIndex + 1 >= routes.Count
                ? TrainStatusEnum.Completed
                : TrainStatusEnum.ArrivedAtStation;

            train.CurrentStationId = to.Id;
            _trainRepository.Update(train);
            await _unitOfWork.SaveChangeAsync(_httpContextAccessor);

            // Chờ gọi lại StartOrContinueSimulationAsync để sang leg tiếp theo
            _cache.Remove(startTimeKey);
        }
        else
        {
            train.Status = progress < 0.1 ? TrainStatusEnum.Departed : TrainStatusEnum.InTransit;
            _trainRepository.Update(train);
            await _trainRepository.SaveChangesAsync();
        }

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

        var itinerary = shipment.ShipmentItineraries
            .FirstOrDefault(i => i.TrainId != null && i.ShipmentId == shipment.Id);

        if (itinerary?.TrainId == null)
            throw new AppException(ErrorCode.NotFound, "Train not assigned", StatusCodes.Status404NotFound);

        if (shipment.ShipmentStatus != ShipmentStatusEnum.AwaitingDelivery)
            throw new AppException(ErrorCode.BadRequest, "Shipment not ready for tracking", StatusCodes.Status400BadRequest);

        var trainId = itinerary.TrainId;

        var position = await GetTrainPositionAsync(trainId);

        var rawTrainStatus = Enum.Parse<TrainStatusEnum>(position.Status);
        var mappedShipmentStatus = MapTrainStatusToShipmentStatus(rawTrainStatus);

        // Cập nhật message trong ShipmentItinerary
        var message = rawTrainStatus switch
        {
            TrainStatusEnum.Departed => $"Rời trạm {position.FromStation} lúc {DateTime.UtcNow:HH:mm:ss dd/MM/yyyy}",
            TrainStatusEnum.InTransit => $"Đang di chuyển từ {position.FromStation} đến {position.ToStation}",
            TrainStatusEnum.ArrivedAtStation => $"Đã đến trạm {position.ToStation} lúc {DateTime.UtcNow:HH:mm:ss dd/MM/yyyy}",
            _ => null
        };

        if (!string.IsNullOrWhiteSpace(message))
        {
            itinerary.Message ??= "";
            if (!itinerary.Message.Contains(message))
            {
                itinerary.Message += $"{message}\n";
                _shipmentItineraryRepository.Update(itinerary);
            }
        }

        // Lưu tracking event
        var tracking = new ShipmentTracking
        {
            ShipmentId = shipment.Id,
            CurrentShipmentStatus = mappedShipmentStatus,
            Status = rawTrainStatus.ToString(),
            EventTime = DateTimeOffset.UtcNow,
            Note = message
        };
        await _shipmentTrackingRepository.AddAsync(tracking);

        // Update shipment nếu cần
        shipment.ShipmentStatus = mappedShipmentStatus;
        _shipmentRepository.Update(shipment);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);

        // Gắn thông tin
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
                Messages = itinerary.Message?.Trim()
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