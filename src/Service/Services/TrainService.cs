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
                    si.Shipment.ShipmentStatus == ShipmentStatusEnum.PickedUp)
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

    public async Task<bool> UpdateTrainStatusAsync(string trainId)
    {
        var train = await _trainRepository.GetTrainWithItineraryAndStationsAsync(trainId);

        if (train == null || train.ShipmentItineraries == null || !train.ShipmentItineraries.Any())
            throw new AppException(ErrorCode.NotFound, "Train or its itineraries not found", StatusCodes.Status404NotFound);

        var orderedLegs = train.ShipmentItineraries
            .Where(it => it.Route != null && it.Route.FromStation != null && it.Route.ToStation != null)
            .OrderBy(it => it.LegOrder)
            .ToList();

        if (!orderedLegs.Any())
            throw new AppException(ErrorCode.NotFound, "Valid shipment itineraries not found", StatusCodes.Status404NotFound);

        var currentLeg = orderedLegs.First();
        var from = currentLeg.Route.FromStation;
        var to = currentLeg.Route.ToStation;

        if (from.Latitude == null || from.Longitude == null || to.Latitude == null || to.Longitude == null)
            throw new AppException(ErrorCode.BadRequest, "Station coordinates are incomplete", StatusCodes.Status400BadRequest);

        var startTime = DateTimeOffset.UtcNow;
        var now = DateTimeOffset.UtcNow;
        var elapsedSeconds = (now - startTime).TotalSeconds;

        var distanceKm = Haversine(from.Latitude.Value, from.Longitude.Value, to.Latitude.Value, to.Longitude.Value);
        var speedKmH = train.TopSpeedKmH ?? 40;
        var etaSeconds = (distanceKm / speedKmH) * 3600;
        var progress = Math.Clamp(elapsedSeconds / etaSeconds, 0, 1);

        // Determine new status & message
        TrainStatusEnum newStatus;
        string? stationMessage;

        if (progress < 0.01)
        {
            newStatus = TrainStatusEnum.AwaitingDeparture;
            stationMessage = $"Chờ khởi hành tại ga {from.StationNameVi}";
        }
        else if (progress < 1.0)
        {
            newStatus = TrainStatusEnum.Departed;
            stationMessage = $"Đang di chuyển từ ga {from.StationNameVi} đến {to.StationNameVi}";
        }
        else if (progress >= 1.0 && orderedLegs.Count == 1)
        {
            newStatus = TrainStatusEnum.Completed;
            stationMessage = $"Đã đến ga cuối cùng {to.StationNameVi}";
        }
        else
        {
            newStatus = TrainStatusEnum.ArrivedAtStation;
            stationMessage = $"Đã đến ga trung chuyển {to.StationNameVi}";
        }

        if (train.Status == newStatus)
        {
            _logger.Information("No train status change for {TrainId}", trainId);
            return false;
        }

        // Update train status
        var oldStatus = train.Status;
        train.Status = newStatus;
        train.LastUpdatedAt = DateTimeOffset.UtcNow; // Fixed property name
        _trainRepository.Update(train);

        // Cập nhật ShipmentItineraries & Shipment & ParcelTracking
        var relatedItineraries = train.ShipmentItineraries.ToList();

        foreach (var it in relatedItineraries)
        {
            // Map train status to shipment status
            var mappedShipmentStatus = MapTrainToShipmentStatus(newStatus);

            // Update itinerary
            it.IsCompleted = mappedShipmentStatus == ShipmentStatusEnum.Completed;
            _shipmentItineraryRepository.Update(it);

            // Update shipment
            if (it.Shipment != null)
            {
                it.Shipment.LastUpdatedAt = DateTimeOffset.UtcNow; // Corrected property name
                it.Shipment.ShipmentStatus = mappedShipmentStatus;
                _shipmentRepository.Update(it.Shipment);
            }
        }

        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);

        _logger.Information("Train {TrainId} status updated from {OldStatus} to {NewStatus}. {StationMessage}",
            trainId, oldStatus, newStatus, stationMessage);

        return true;
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
        if (_cache.TryGetValue<TrainPositionResult>(trainId, out var cached))
        {
            _logger.Information("Returning cached train position for {TrainId}", trainId);
            return cached;
        }

        var train = await _trainRepository.GetTrainWithItineraryAndStationsAsync(trainId);
        if (train == null || train.ShipmentItineraries == null || !train.ShipmentItineraries.Any())
            throw new AppException(ErrorCode.NotFound, "Train or its itinerary not found", StatusCodes.Status404NotFound);

        // ✅ Chỉ cho phép khi trạng thái là Departed
        if (train.Status != TrainStatusEnum.Departed)
            throw new AppException(ErrorCode.BadRequest, $"Train must be in '{TrainStatusEnum.Departed}' state to get position", StatusCodes.Status400BadRequest);

        var itinerary = train.ShipmentItineraries
            .Where(it => it.Route != null && it.Route.FromStation != null && it.Route.ToStation != null)
            .OrderBy(it => it.LegOrder)
            .FirstOrDefault();

        if (itinerary == null)
            throw new AppException(ErrorCode.NotFound, "Valid itinerary not found", StatusCodes.Status404NotFound);

        var from = itinerary.Route.FromStation;
        var to = itinerary.Route.ToStation;

        if (from.Latitude == null || from.Longitude == null || to.Latitude == null || to.Longitude == null)
            throw new AppException(ErrorCode.BadRequest, "Station coordinates are incomplete", StatusCodes.Status400BadRequest);

        var startTime = DateTimeOffset.UtcNow;
        var now = DateTimeOffset.UtcNow;
        var elapsedSeconds = (now - startTime).TotalSeconds;

        var distanceKm = Haversine(from.Latitude.Value, from.Longitude.Value, to.Latitude.Value, to.Longitude.Value);
        var speedKmH = train.TopSpeedKmH ?? 40;
        var etaSeconds = (distanceKm / speedKmH) * 3600;

        var progressFraction = Math.Clamp(elapsedSeconds / etaSeconds, 0, 1);
        var (lat, lng) = Interpolate(from.Latitude.Value, from.Longitude.Value, to.Latitude.Value, to.Longitude.Value, progressFraction);

        var result = new TrainPositionResult
        {
            TrainId = trainId,
            Latitude = lat,
            Longitude = lng,
            FromStation = from.StationNameVi,
            ToStation = to.StationNameVi,
            StartTime = startTime,
            ETA = TimeSpan.FromSeconds(etaSeconds),
            Elapsed = TimeSpan.FromSeconds(elapsedSeconds),
            ProgressPercent = (int)(progressFraction * 100),
            Status = train.Status.ToString()
        };

        _cache.Set(trainId, result, TimeSpan.FromSeconds(5));
        return result;
    }

    // for getting train position based on tracking code
    public async Task<TrainPositionResult> GetPositionByTrackingCodeAsync(string trackingCode)
    {
        var shipment = await _trainRepository.GetShipmentWithTrainAsync(trackingCode);
        if (shipment == null)
            throw new AppException(ErrorCode.NotFound, "Shipment not found", StatusCodes.Status404NotFound);

        var itinerary = shipment.ShipmentItineraries?.FirstOrDefault(i => i.TrainId != null);
        if (itinerary?.TrainId == null)
            throw new AppException(ErrorCode.NotFound, "Train not assigned to shipment", StatusCodes.Status404NotFound);

        // ✅ Kiểm tra trạng thái tàu
        var train = await _trainRepository.GetByIdAsync(itinerary.TrainId);
        if (train == null)
            throw new AppException(ErrorCode.NotFound, "Train not found", StatusCodes.Status404NotFound);

        if (train.Status != TrainStatusEnum.Departed)
            throw new AppException(ErrorCode.BadRequest, $"Train must be in '{TrainStatusEnum.Departed}' state to get position", StatusCodes.Status400BadRequest);

        return await GetTrainPositionAsync(train.Id);
    }

    #region Helpers
    private static double Haversine(double lat1, double lon1, double lat2, double lon2)
    {
        var R = 6371; // Earth radius in km
        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double DegreesToRadians(double deg) => deg * Math.PI / 180;

    private static (double lat, double lng) Interpolate(double lat1, double lng1, double lat2, double lng2, double frac)
    {
        return (
            lat1 + (lat2 - lat1) * frac,
            lng1 + (lng2 - lng1) * frac
        );
    }
    #endregion
}