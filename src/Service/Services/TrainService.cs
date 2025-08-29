using MetroShip.Repository.Base;
using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Shipment;
using MetroShip.Service.ApiModels.Train;
using MetroShip.Service.BusinessModels;
using MetroShip.Service.Helpers;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Jobs;
using MetroShip.Service.Mapper;
using MetroShip.Service.Utils;
using MetroShip.Service.Validations;
using MetroShip.Utility.Config;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Enums;
using MetroShip.Utility.Exceptions;
using MetroShip.Utility.Helpers;
using MetroShip.Utility.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using RestSharp.Extensions;
using Serilog;
using SkiaSharp;
using System.Linq.Expressions;
using System.Net;

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
    private readonly IMetroTimeSlotRepository _timeSlotRepository =
        serviceProvider.GetRequiredService<IMetroTimeSlotRepository>();
    private readonly IMemoryCache _cache =
        serviceProvider.GetRequiredService<IMemoryCache>();
    private readonly ScheduleTrainJob _scheduleTrainJob = serviceProvider.GetRequiredService<ScheduleTrainJob>();
    private readonly IMetroRouteRepository _metroRoute = serviceProvider.GetRequiredService<IMetroRouteRepository>();
    private readonly ITrainStateStoreService _trainStateStore =
        serviceProvider.GetRequiredService<ITrainStateStoreService>();
    private readonly ITrainScheduleRepository _trainScheduleRepository = serviceProvider.GetRequiredService<ITrainScheduleRepository>();

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
        await CalculateCurrentCapacity(metroTrains, response, request);
        return response;
    }

    public async Task<PaginatedListResponse<TrainListResponse>> PaginatedListResponse(
    TrainListFilterRequest request)
    {
        _logger.Information("Get paginated list of trains with page number: {pageNumber}, page size: {pageSize}",
            request.PageNumber, request.PageSize);

        // Lấy danh sách MetroTrain từ DB (entity)
        var paginatedList = await _trainRepository.GetAllPaginatedQueryable(
            request.PageNumber,
            request.PageSize,
            BuildShipmentFilterExpression(request),
            t => t.TrainCode, true);

        var trainIds = paginatedList.Items.Select(t => t.Id).ToList();
        var trainSchedules = await _trainScheduleRepository.GetTrainSchedulesByTrainListAsync(trainIds);

        // 👉 Map entity -> DTO
        var response = _mapper.MapToTrainListResponsePaginatedList(paginatedList);

        foreach (var train in response.Items) // train bây giờ là TrainListResponse
        {
            // Gán train schedules
            train.TrainSchedules = trainSchedules
                .Where(ts => ts.TrainId == train.Id)
                .OrderBy(ts => ts.Shift)
                .Select(ts => new TrainScheduleResponse
                {
                    Id = ts.Id,
                    TrainId = ts.TrainId,
                    TimeSlotId = ts.TimeSlotId,
                    Shift = ts.Shift,
                    LineId = ts.LineId,
                    LineName = ts.LineName,
                    DepartureStationId = ts.DepartureStationId,
                    DepartureStationName = ts.DepartureStationName,
                    DestinationStationId = ts.DestinationStationId,
                    DestinationStationName = ts.DestinationStationName,
                    Direction = ts.Direction
                })
                .ToList();

            //Lấy direction hiện tại từ Firebase
            var firebaseDirection = await _trainStateStore.GetDirectionAsync(train.Id);
            var segmentIndex = await _trainStateStore.GetSegmentIndexAsync(train.Id);

            if (firebaseDirection.HasValue && segmentIndex.HasValue)
            {
                //Lấy direction thực tế theo route hiện tại
                var currentDirection = await _trainScheduleRepository
                    .GetTrainDirectionByTrainAndSegmentAsync(train.Id, segmentIndex.Value, firebaseDirection.Value);

                if (currentDirection.HasValue)
                {
                    train.Direction = currentDirection.Value;
                }
                else
                {
                    //fallback: nếu DB không tìm thấy route, gán theo Firebase
                    train.Direction = firebaseDirection.Value;
                }
            }
        }

        return response;
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

    // create a train
    public async Task<string> CreateTrainAsync(CreateTrainRequest request)
    {
        _logger.Information("Creating a new train with request: {@request}", request);
        _trainValidator.ValidateCreateTrainRequest(request);

        var metroRouteCode = await _metroRoute.GetAll()
            .Where(l => l.DeletedAt == null && l.Id == request.LineId)
            .Select(l => l.LineCode)
            .FirstOrDefaultAsync();

        if (metroRouteCode == null)
        {
            throw new AppException(ErrorCode.NotFound,
                MetroRouteMessageConstants.METROROUTE_NOT_FOUND,
            StatusCodes.Status404NotFound);
        }

        request.TrainCode = string.IsNullOrEmpty(request.TrainCode)
            ? MetroCodeGenerator.GenerateTrainCode(metroRouteCode, request.TrainNumber)
            : request.TrainCode;
        // Check if train code already exists
        var existingTrain = await _trainRepository.IsExistAsync(t => t.TrainCode == request.TrainCode);
        if (existingTrain)
        {
            throw new AppException(ErrorCode.BadRequest,
            ResponseMessageTrain.TRAIN_EXISTED,
            StatusCodes.Status400BadRequest);
        }

        // Map to train entity
        var train = _mapper.MapToMetroTrainEntity(request);
        await _trainRepository.AddAsync(train);
        await _scheduleTrainJob.CreateTrainScheduleForNewTrain(train);
        return ResponseMessageTrain.TRAIN_CREATE_SUCCESS;
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

        // Available: Train's capacity is not fully utilized
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
            if (!string.IsNullOrEmpty(request.StationId))
            {
                expression = expression.And(x => x.Line.Routes.Any(
                    r => r.FromStationId == request.StationId ||
                    r.ToStationId == request.StationId));
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
        //await ValidateDirectionRule(request.TrainId, request.TimeSlotId, request.Date, request.Direction);

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

    private async Task ValidateDirectionRule(string trainId, string timeSlotId, DateOnly date, DirectionEnum requestDirection)
    {
        // Get the current timeslot to determine shift
        var currentTimeSlot = await _timeSlotRepository.GetByIdAsync(timeSlotId);
        if (currentTimeSlot == null)
        {
            throw new AppException(ErrorCode.NotFound, "TimeSlot not found", StatusCodes.Status400BadRequest);
        }

        // Check if this is morning shift and validate against previous night
        if (currentTimeSlot.Shift == ShiftEnum.Morning)
        {
            var previousDate = date.AddDays(-1);

            // Find night timeslot from previous day
            var nightTimeSlot = await _timeSlotRepository.GetAllWithCondition(
                ts => ts.Shift == ShiftEnum.Night).FirstOrDefaultAsync();

            if (nightTimeSlot != null)
            {
                // Check if train ran same direction in previous night
                var previousNightItinerary = await _shipmentItineraryRepository.GetAllWithCondition(
                    si => si.TrainId.Equals(trainId) &&
                          si.TimeSlotId == nightTimeSlot.Id &&
                          si.Date.HasValue &&
                          si.Date.Value.Equals(previousDate) &&
                          si.Route.Direction == requestDirection)
                    .FirstOrDefaultAsync();

                if (previousNightItinerary != null)
                {
                    throw new AppException(ErrorCode.BadRequest,
                        $"Train cannot run same direction ({requestDirection}) in Morning after running same direction in previous Night",
                        StatusCodes.Status400BadRequest);
                }
            }
        }

        // For other shifts, check against immediate previous shift
        else
        {
            var previousShift = GetPreviousShift(currentTimeSlot.Shift);
            var previousTimeSlot = await _timeSlotRepository.GetAllWithCondition(
                ts => ts.Shift == previousShift).FirstOrDefaultAsync();

            if (previousTimeSlot != null)
            {
                var previousItinerary = await _shipmentItineraryRepository.GetAllWithCondition(
                    si => si.TrainId.Equals(trainId) &&
                          si.TimeSlotId == previousTimeSlot.Id &&
                          si.Date.HasValue &&
                          si.Date.Value.Equals(date) &&
                          si.Route.Direction == requestDirection)
                    .FirstOrDefaultAsync();

                if (previousItinerary != null)
                {
                    throw new AppException(ErrorCode.BadRequest,
                        $"Train cannot run same direction ({requestDirection}) in consecutive shifts",
                        StatusCodes.Status400BadRequest);
                }
            }
        }
    }

    private ShiftEnum GetPreviousShift(ShiftEnum currentShift)
    {
        return currentShift switch
        {
            ShiftEnum.Morning => ShiftEnum.Night,
            ShiftEnum.Afternoon => ShiftEnum.Morning,
            ShiftEnum.Evening => ShiftEnum.Afternoon,
            ShiftEnum.Night => ShiftEnum.Evening,
            _ => ShiftEnum.Morning
        };
    }

    public async Task StartOrContinueSimulationAsync(string trainId)
    {
        _logger.Information("Starting or continuing simulation for train {TrainId}", trainId);

        var train = await _trainRepository.GetTrainWithAllRoutesAsync(trainId)
            ?? throw new AppException(ErrorCode.NotFound, "Train not found", StatusCodes.Status404NotFound);

        if (train.Line?.Routes == null || train.Line.Routes.Count == 0)
            throw new AppException(ErrorCode.NotFound, "No routes found for train", StatusCodes.Status404NotFound);

        var allRoutes = train.Line.Routes
            .Where(r => r.FromStation != null && r.ToStation != null)
            .ToList();

        var existingIndex = await _trainStateStore.GetSegmentIndexAsync(trainId) ?? -1;

        // ✅ Lấy direction từ Firebase hoặc đoán từ station hiện tại
        var direction = await _trainStateStore.GetDirectionAsync(trainId)
            ?? InferTrainDirectionFromCurrentStation(
                train,
                train.CurrentStationId ?? throw new AppException(ErrorCode.BadRequest, "Train has no current station", StatusCodes.Status400BadRequest)
            );

        // 🔄 Đổi chiều nếu Completed và đang ở đầu tuyến ngược
        var reverseDirection = direction == DirectionEnum.Forward ? DirectionEnum.Backward : DirectionEnum.Forward;
        var reverseRoutes = allRoutes
            .Where(r => r.Direction == reverseDirection)
            .OrderBy(r => r.SeqOrder)
            .ToList();

        var reverseStartStationId = reverseRoutes.FirstOrDefault()?.FromStationId;

        if (train.Status == TrainStatusEnum.Completed && train.CurrentStationId == reverseStartStationId)
        {
            direction = reverseDirection;
            existingIndex = -1;

            await _trainStateStore.SetDirectionAsync(trainId, direction);
            await _trainStateStore.SetSegmentIndexAsync(trainId, existingIndex);

            _logger.Information("🔁 Train {TrainId} auto reversed to direction {Direction}", trainId, direction);
        }

        // ✅ Lấy tất cả route theo direction
        var routes = allRoutes
            .Where(r => r.Direction == direction)
            .OrderBy(r => r.SeqOrder)
            .ToList();

        if (routes.Count == 0)
            throw new AppException(ErrorCode.NotFound, $"No routes found for direction {direction}", StatusCodes.Status404NotFound);

        // 🚫 Không cho phép chạy nếu đang chạy
        if (train.Status == TrainStatusEnum.InTransit || train.Status == TrainStatusEnum.Departed)
            throw new AppException(ErrorCode.BadRequest, "Train is already running", StatusCodes.Status400BadRequest);

        // ✅ Nếu Completed nhưng chưa reset index → kiểm tra đúng endpoint + tọa độ
        if (train.Status == TrainStatusEnum.Completed && existingIndex != -1)
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

            existingIndex = -1;
        }

        // ✅ Nếu ArrivedAtStation → cập nhật vị trí station hiện tại
        if (train.Status == TrainStatusEnum.ArrivedAtStation && existingIndex >= 0 && existingIndex < routes.Count)
        {
            var prevRoute = routes[existingIndex];
            train.CurrentStationId = prevRoute.ToStationId;
            train.Latitude = prevRoute.ToStation?.Latitude;
            train.Longitude = prevRoute.ToStation?.Longitude;

            _trainRepository.Update(train);
            await _trainRepository.SaveChangesAsync();
        }

        var nextIndex = existingIndex + 1;

        // ✅ Nếu đã hết tuyến → đánh dấu Completed
        if (nextIndex >= routes.Count)
        {
            var lastRoute = routes.Last();
            train.Status = TrainStatusEnum.Completed;
            train.CurrentStationId = lastRoute.ToStationId;
            train.Latitude = lastRoute.ToStation?.Latitude;
            train.Longitude = lastRoute.ToStation?.Longitude;

            _trainRepository.Update(train);
            await _trainRepository.SaveChangesAsync();

            await _trainStateStore.RemoveSegmentIndexAsync(trainId);
            await _trainStateStore.RemoveStartTimeAsync(trainId);

            _logger.Information("✅ Train {TrainId} completed direction {Direction}.", trainId, direction);
            return;
        }

        // ✅ Bắt đầu leg tiếp theo
        var nextRoute = routes[nextIndex];

        await _trainStateStore.SetSegmentIndexAsync(trainId, nextIndex);
        await _trainStateStore.SetDirectionAsync(trainId, direction);
        await _trainStateStore.SetStartTimeAsync(trainId, DateTimeOffset.UtcNow);

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

        // ✅ Cập nhật ShipmentStatus sang InTransit
        //var rawShipments = await _trainRepository.GetLoadedShipmentsByTrainAsync(train.Id);
        /*var rawShipments = await _trainRepository.GetLoadedShipmentsByTrainAsync(train.Id);

        var shipmentsToUpdate = rawShipments
            .Where(s => s.ShipmentStatus == ShipmentStatusEnum.LoadOnMetro)
            .ToList();

        if (shipmentsToUpdate.Count > 0)
        {
            foreach (var shipment in shipmentsToUpdate)
            {
                shipment.ShipmentStatus = ShipmentStatusEnum.InTransit;

                _shipmentTrackingRepository.Add(new ShipmentTracking
                {
                    ShipmentId = shipment.Id,
                    Status = ShipmentStatusEnum.InTransit.ToString(),
                    CurrentShipmentStatus = ShipmentStatusEnum.InTransit,
                    EventTime = DateTimeOffset.UtcNow,
                    Note = "Shipment status updated as train departed"
                });

                _shipmentRepository.Update(shipment);
            }

            await _unitOfWork.SaveChangeAsync(_httpContextAccessor);

            _logger.Information("📦 Updated {Count} shipments to InTransit as train {TrainId} departed.", shipmentsToUpdate.Count, train.Id);
        }*/
    }

    private async Task CalculateCurrentCapacity(IList<MetroTrain> metroTrains, IList<TrainCurrentCapacityResponse> response,
        LineSlotDateFilterRequest request)
    {
        _logger.Information("Calculating current capacity for trains");

        var shipmentValidStatus = new[]
        {
        ShipmentStatusEnum.AwaitingDropOff,
        ShipmentStatusEnum.AwaitingPayment,
        ShipmentStatusEnum.PickedUp
    };

        // Get all unique shipment IDs from the trains with proper null checks
        var shipmentIds = metroTrains
            .SelectMany(t => t.ShipmentItineraries ?? new List<ShipmentItinerary>()) // Handle null ShipmentItineraries
            .Where(si =>
                si != null && // Check if ShipmentItinerary is not null
                si.Shipment != null && // Check if Shipment is not null
                si.TimeSlotId == request.TimeSlotId &&
                si.Date.HasValue &&
                si.Date.Value.Equals(request.Date) &&
                shipmentValidStatus.Contains(si.Shipment.ShipmentStatus)
            )
            .Select(si => si.ShipmentId)
            .Distinct()
            .ToList();

        // If no shipments found, return all trains with zero capacity
        if (!shipmentIds.Any())
        {
            _logger.Information("No shipments found for the given criteria, setting current capacity to zero for all trains");
            foreach (var train in response)
            {
                train.CurrentWeightKg = 0;
                train.CurrentVolumeM3 = 0;
            }
            return;
        }

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
        var shipmentLookup = shipmentData.ToDictionary(s => s.Id);

        // Calculate current weight and volume for each train
        foreach (var train in response)
        {
            var trainEntity = metroTrains.FirstOrDefault(t => t.Id == train.Id);

            if (trainEntity?.ShipmentItineraries != null)
            {
                var trainShipmentIds = trainEntity.ShipmentItineraries
                    .Where(si => si != null && si.Shipment != null) // Add null checks here too
                    .Where(si =>
                        si.TimeSlotId == request.TimeSlotId &&
                        si.Date.HasValue &&
                        si.Date.Value.Equals(request.Date) &&
                        shipmentValidStatus.Contains(si.Shipment.ShipmentStatus))
                    .Select(si => si.ShipmentId)
                    .Where(shipmentLookup.ContainsKey)
                    .ToList();

                if (trainShipmentIds.Any())
                {
                    train.CurrentWeightKg = trainShipmentIds.Sum(id => shipmentLookup[id].TotalWeightKg ?? 0);
                    train.CurrentVolumeM3 = trainShipmentIds.Sum(id => shipmentLookup[id].TotalVolumeM3 ?? 0);
                }
                else
                {
                    // Explicitly set to zero if no valid shipments found
                    train.CurrentWeightKg = 0;
                    train.CurrentVolumeM3 = 0;
                }
            }
            else
            {
                // Explicitly set to zero if no itineraries found
                train.CurrentWeightKg = 0;
                train.CurrentVolumeM3 = 0;
            }
        }
    }

    public async Task<TrainPositionResult> GetTrainPositionByTrackingCodeAsync(string trackingCode)
    {
        // 🔹 1. Lấy shipment + itinerary với đầy đủ thông tin route và stations
        var shipment = await _trainRepository.GetShipmentWithItinerariesAndRoutesAsync(trackingCode)
            ?? throw new AppException(ErrorCode.NotFound, ResponseMessageShipment.SHIPMENT_NOT_FOUND, StatusCodes.Status404NotFound);

        if (shipment.ShipmentItineraries == null || shipment.ShipmentItineraries.Count == 0)
            throw new AppException(ErrorCode.BadRequest, "Shipment has no itinerary", StatusCodes.Status400BadRequest);

        // 🔹 2. LUÔN tạo fullPath ngay từ đầu để đảm bảo có dữ liệu
        const int steps = 10;
        var fullPath = shipment.ShipmentItineraries
            .OrderBy(x => x.LegOrder)
            .Where(x => x.Route != null && x.Route.FromStation != null && x.Route.ToStation != null)
            .Select(x =>
            {
                var from = x.Route.FromStation;
                var to = x.Route.ToStation;

                var polyline = Enumerable.Range(0, steps + 1)
                    .Select(s =>
                    {
                        var p = s / (double)steps;
                        var (lat, lng) = GeoUtils.Interpolate(
                            from.Latitude ?? 0, from.Longitude ?? 0,
                            to.Latitude ?? 0, to.Longitude ?? 0,
                            p);
                        return new GeoPoint { Latitude = lat, Longitude = lng };
                    }).ToList();

                return new
                {
                    x.LegOrder,
                    From = new { Name = from.StationNameVi, from.Latitude, from.Longitude },
                    To = new { Name = to.StationNameVi, to.Latitude, to.Longitude },
                    x.IsCompleted,
                    x.Message,
                    Polyline = polyline
                };
            }).ToList();

        // 🔹 3. Cho phép tracking từ khi pickup
        if (shipment.ShipmentStatus != ShipmentStatusEnum.ApplyingSurcharge &&
            shipment.ShipmentStatus != ShipmentStatusEnum.InTransit &&
            shipment.ShipmentStatus != ShipmentStatusEnum.AwaitingDelivery)
        {
            throw new AppException(ErrorCode.BadRequest, "Shipment not ready for tracking", StatusCodes.Status400BadRequest);
        }

        // 🔹 4. Lấy itinerary gắn với train
        var itinerary = shipment.ShipmentItineraries
            .FirstOrDefault(i => i.TrainId != null);

        // Nếu chưa có train assigned, vẫn trả về thông tin itinerary
        if (itinerary?.TrainId == null)
        {
            // Trả về thông tin cơ bản với fullPath
            return new TrainPositionResult
            {
                TrainId = "not-assigned-yet",
                Latitude = fullPath.FirstOrDefault()?.From.Latitude ?? 0,
                Longitude = fullPath.FirstOrDefault()?.From.Longitude ?? 0,
                Status = ShipmentStatusEnum.AwaitingDelivery.ToString(),
                Path = fullPath.SelectMany(p => p.Polyline).ToList(),
                AdditionalData = new
                {
                    RawTrainStatus = "NotAssigned",
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
                        FullPath = fullPath
                    }
                }
            };
        }

        var trainId = itinerary.TrainId;

        // 🔹 5. Check train đã chạy chưa
        var hasSegmentIndex = await _trainStateStore.HasSegmentIndexAsync(trainId);

        // Nếu train chưa bắt đầu, vẫn trả về thông tin với fullPath
        if (!hasSegmentIndex)
        {
            return new TrainPositionResult
            {
                TrainId = trainId,
                Latitude = fullPath.FirstOrDefault()?.From.Latitude ?? 0,
                Longitude = fullPath.FirstOrDefault()?.From.Longitude ?? 0,
                Status = ShipmentStatusEnum.AwaitingDelivery.ToString(),
                Path = fullPath.SelectMany(p => p.Polyline).ToList(),
                AdditionalData = new
                {
                    RawTrainStatus = "Scheduled",
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
                        FullPath = fullPath
                    }
                }
            };
        }

        // 🔹 6. Lấy position hiện tại của tàu
        var position = await _trainStateStore.GetPositionResultAsync(trainId)
            ?? throw new AppException(ErrorCode.NotFound, "Train position not found", StatusCodes.Status404NotFound);

        var rawTrainStatus = Enum.Parse<TrainStatusEnum>(position.Status);
        var mappedShipmentStatus = MapTrainStatusToShipmentStatus(rawTrainStatus);

        // 🔹 7. Xử lý shipment itinerary
        var currentLeg = shipment.ShipmentItineraries
            .OrderBy(i => i.LegOrder)
            .FirstOrDefault(i => !i.IsCompleted);

        if (currentLeg != null)
        {
            var fromStationId = currentLeg.Route?.FromStationId;
            var toStationId = currentLeg.Route?.ToStationId;
            var fromStation = currentLeg.Route?.FromStation?.StationNameVi;
            var toStation = currentLeg.Route?.ToStation?.StationNameVi;

            // 🚨 Cảnh báo nếu tàu không đi đúng route của shipment
            if (fromStation != position.FromStation || toStation != position.ToStation)
            {
                _logger.Warning("🚨 Shipment {TrackingCode} leg not aligned: Shipment {From}->{To}, Train {From}->{To}",
                    trackingCode, fromStation, toStation, position.FromStation, position.ToStation);
            }

            // ✅ Nếu tàu đang ở ga xuất phát → Shipment bắt đầu InTransit
            /*if (itinerary?.Train?.CurrentStationId == toStationId &&
                shipment.ShipmentStatus == ShipmentStatusEnum.AwaitingDelivery)
            {
                shipment.ShipmentStatus = ShipmentStatusEnum.InTransit;

                _shipmentTrackingRepository.Add(new ShipmentTracking
                {
                    ShipmentId = shipment.Id,
                    CurrentShipmentStatus = ShipmentStatusEnum.InTransit,
                    Status = "InTransit",
                    EventTime = DateTimeOffset.UtcNow,
                    Note = $"Shipment departed from {fromStation}"
                });

                _shipmentRepository.Update(shipment);
                await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
            }*/

            // ✅ Nếu tàu tới ga đích của leg hiện tại → hoàn thành leg
            if (itinerary?.Train?.CurrentStationId == toStationId &&
                rawTrainStatus == TrainStatusEnum.ArrivedAtStation)
            {
                currentLeg.IsCompleted = true;

                var messageLine = $"[Arrived at {toStation} - {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm}]";
                currentLeg.Message = string.IsNullOrEmpty(currentLeg.Message)
                    ? messageLine
                    : $"{currentLeg.Message}\n{messageLine}";

                _shipmentItineraryRepository.Update(currentLeg);
                await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
            }
        }

        // 🔹 6. Cập nhật shipment status nếu cần
        //if (shipment.ShipmentStatus != mappedShipmentStatus)
        //{
        //    shipment.ShipmentStatus = mappedShipmentStatus;

        //    _shipmentTrackingRepository.Add(new ShipmentTracking
        //    {
        //        ShipmentId = shipment.Id,
        //        CurrentShipmentStatus = mappedShipmentStatus,
        //        Status = mappedShipmentStatus.ToString(),
        //        EventTime = DateTimeOffset.UtcNow,
        //        Note = $"Shipment moved to status: {mappedShipmentStatus}"
        //    });

        //    _shipmentRepository.Update(shipment);
        //    await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
        //}

        // 🔹 7. Nếu tất cả leg hoàn tất → shipment delivered
        //if (shipment.ShipmentItineraries.All(i => i.IsCompleted))
        //{
        //    shipment.ShipmentStatus = ShipmentStatusEnum.Delivered;

        //    _shipmentTrackingRepository.Add(new ShipmentTracking
        //    {
        //        ShipmentId = shipment.Id,
        //        CurrentShipmentStatus = ShipmentStatusEnum.Delivered,
        //        Status = "Delivered",
        //        EventTime = DateTimeOffset.UtcNow,
        //        Note = "All legs completed. Shipment delivered."
        //    });

        //    _shipmentRepository.Update(shipment);
        //    await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
        //}

        // 🔹 8. Đóng gói kết quả với fullPath
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
                FullPath = fullPath
            }
        };

        // 🔹 9. Đồng bộ Firebase shipment_tracking
        await _trainStateStore.SetShipmentTrackingAsync(
            shipment.TrackingCode!,
            trainId!,
            position
        );

        return position;
    }

    // for getting train position based on trainId
    public async Task<TrainPositionResult> GetTrainPositionAsync(string trainId)
    {
        // 🔹 Lấy Direction từ Firebase
        var direction = await _trainStateStore.GetDirectionAsync(trainId)
            ?? throw new AppException(ErrorCode.BadRequest,
                "Train direction not initialized in Firebase. Call StartOrContinueSimulationAsync first.",
                StatusCodes.Status400BadRequest);

        // 🔹 Lấy SegmentIndex từ Firebase
        var currentIndex = await _trainStateStore.GetSegmentIndexAsync(trainId)
            ?? throw new AppException(ErrorCode.BadRequest,
                "Train segment not initialized in Firebase. Call StartOrContinueSimulationAsync.",
                StatusCodes.Status400BadRequest);

        // 🔹 Lấy StartTime từ Firebase
        var startTime = await _trainStateStore.GetStartTimeAsync(trainId)
            ?? throw new AppException(ErrorCode.BadRequest,
                "Start time not initialized in Firebase. Call simulation start first.",
                StatusCodes.Status400BadRequest);

        // 🔹 Lấy train và routes
        var train = await _trainRepository.GetTrainWithRoutesAsync(trainId, direction)
            ?? throw new AppException(ErrorCode.NotFound, "Train not found", StatusCodes.Status404NotFound);

        var routes = train.Line?.Routes?
            .Where(r => r.FromStation != null && r.ToStation != null && r.Direction == direction)
            .OrderBy(r => r.SeqOrder)
            .ToList();

        if (routes == null || routes.Count == 0)
            throw new AppException(ErrorCode.NotFound, "No route data found", StatusCodes.Status404NotFound);

        if (currentIndex < 0 || currentIndex >= routes.Count)
            throw new AppException(ErrorCode.BadRequest, "Train segment index out of range.", StatusCodes.Status400BadRequest);

        var currentRoute = routes[currentIndex];
        var from = currentRoute.FromStation!;
        var to = currentRoute.ToStation!;

        // --- Tính toán progress ---
        var now = DateTimeOffset.UtcNow;
        var elapsed = (now - startTime).TotalSeconds;
        var distanceKm = (double)currentRoute.LengthKm;
        var speedKmh = 100;
        var eta = (distanceKm / speedKmh) * 3600;
        var progress = Math.Clamp(elapsed / eta, 0, 1);

        if (progress >= 1)
            progress = 1;

        // --- Nội suy vị trí ---
        var (lat, lng) = GeoUtils.Interpolate(
            from.Latitude!.Value, from.Longitude!.Value,
            to.Latitude!.Value, to.Longitude!.Value,
            progress);

        // --- Xác định status ---
        var displayStatus = train.Status;
        if (displayStatus == TrainStatusEnum.Departed || displayStatus == TrainStatusEnum.InTransit)
        {
            displayStatus = progress < 0.1
                ? TrainStatusEnum.Departed
                : TrainStatusEnum.InTransit;
        }

        // --- Current animation path ---
        var path = Enumerable.Range(0, 11).Select(i =>
        {
            var p = i / 10.0;
            var (stepLat, stepLng) = GeoUtils.Interpolate(
                from.Latitude!.Value, from.Longitude!.Value,
                to.Latitude!.Value, to.Longitude!.Value,
                p);
            return new GeoPoint { Latitude = stepLat, Longitude = stepLng };
        }).ToList();

        // --- Full polyline ---
        const int steps = 10;
        var fullPath = new List<object>();
        for (int i = 0; i < routes.Count; i++)
        {
            var r = routes[i];
            var f = r.FromStation!;
            var t = r.ToStation!;
            var polyline = Enumerable.Range(0, steps + 1).Select(s =>
            {
                var p = s / (double)steps;
                var (latStep, lngStep) = GeoUtils.Interpolate(
                    f.Latitude!.Value, f.Longitude!.Value,
                    t.Latitude!.Value, t.Longitude!.Value,
                    p
                );
                return new GeoPoint { Latitude = latStep, Longitude = lngStep };
            }).ToList();

            var isCompleted = i < currentIndex;

            fullPath.Add(new
            {
                FromStation = f.StationNameVi,
                ToStation = t.StationNameVi,
                SeqOrder = r.SeqOrder,
                Direction = r.Direction,
                Status = isCompleted,
                Polyline = polyline
            });
        }

        // --- Shipments summary ---
        var allShipmentsRaw = await _trainRepository.GetLoadedShipmentsByTrainAsync(trainId);
        var allShipments = allShipmentsRaw
            .GroupBy(s => s.Id)
            .Select(g => g.First())
            .ToList();

        var shipmentSummaries = allShipments.Select(s =>
        {
            var lastLeg = s.ShipmentItineraries?
                .Where(i => i.Route?.ToStation != null)
                .OrderBy(i => i.LegOrder)
                .LastOrDefault();

            return new
            {
                ShipmentId = s.Id,
                TrackingCode = s.TrackingCode,
                DestinationStation = lastLeg?.Route?.ToStation?.StationNameVi ?? "Unknown",
                DestinationStationId = lastLeg?.Route?.ToStationId,
                CurrentStatus = s.ShipmentStatus.ToString()
            };
        }).ToList();

        // --- Parcel summaries ---
        var parcelSummaries = allShipments
            .Where(s => s.Parcels != null && s.Parcels.Count > 0)
            .SelectMany(s =>
            {
                var lastLeg = s.ShipmentItineraries?
                    .Where(i => i.Route?.ToStation != null)
                    .OrderBy(i => i.LegOrder)
                    .LastOrDefault();

                return s.Parcels!.Select(p => new
                {
                    ParcelId = p.Id,
                    ParcelCode = p.ParcelCode,
                    Description = p.Description,
                    Weight = p.WeightKg,
                    Volume = p.VolumeCm3,
                    ShipmentId = p.ShipmentId,
                    DestinationStationId = lastLeg?.Route?.ToStationId ?? "Unknown",
                    DestinationStation = lastLeg?.Route?.ToStation?.StationNameVi ?? "Unknown"
                });
            })
            .ToList();

        // ✅ Kết quả cuối cùng
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
            Status = displayStatus.ToString(),
            Path = path,
            AdditionalData = new
            {
                FullPath = fullPath,
                Shipments = shipmentSummaries,
                Parcels = parcelSummaries
            }
        };

        // 🔹 Lưu lại vị trí hiện tại vào Firebase
        await _trainStateStore.SetPositionResultAsync(trainId, result);

        return result;
    }

    //// 🔹 Lấy vị trí tàu theo trainId(nhẹ, chỉ đọc từ Firebase)
    //public async Task<TrainPositionResult> GetTrainPositionAsync(string trainId)
    //{
    //    var position = await _trainStateStore.GetPositionResultAsync(trainId);
    //    if (position is null)
    //        throw new AppException(ErrorCode.NotFound,
    //            "Train position not found. Maybe simulation not started or job not running.",
    //            StatusCodes.Status404NotFound);

    //    return position;
    //}

    //// 🔹 Lấy vị trí shipment theo trackingCode (nhẹ, chỉ đọc từ Firebase)
    //public async Task<TrainPositionResult> GetTrainPositionByTrackingCodeAsync(string trackingCode)
    //{
    //    var result = await _trainStateStore.GetShipmentTrackingAsync(trackingCode);
    //    if (result is null)
    //        throw new AppException(ErrorCode.NotFound,
    //            "Shipment tracking not found. Maybe not loaded on train or job not running.",
    //            StatusCodes.Status404NotFound);

    //    return result;
    //}

    public async Task ConfirmTrainArrivedAsync(string trainId, string stationId)
    {
        try
        {
            var train = await _trainRepository.GetTrainWithAllRoutesAsync(trainId)
                ?? throw new AppException(ErrorCode.NotFound, "Train not found", StatusCodes.Status404NotFound);

            if (train.Line?.Routes == null || !train.Line.Routes.Any())
                throw new AppException(ErrorCode.NotFound, "No route information found", StatusCodes.Status404NotFound);

            // 1. Xác định direction từ Firebase
            var direction = await _trainStateStore.GetDirectionAsync(trainId);
            if (direction == null)
            {
                var segmentIndex = await _trainStateStore.GetSegmentIndexAsync(trainId);
                if (segmentIndex != null && segmentIndex >= 0)
                {
                    var routeFromIndex = train.Line.Routes.FirstOrDefault(r => r.SeqOrder == segmentIndex);
                    direction = routeFromIndex?.Direction
                        ?? throw new AppException(ErrorCode.BadRequest, "Cannot determine direction from segment index", StatusCodes.Status400BadRequest);

                    await _trainStateStore.SetDirectionAsync(trainId, direction.Value);
                }
                else
                {
                    direction = InferTrainDirectionFromCurrentStation(train, stationId);
                    await _trainStateStore.SetDirectionAsync(trainId, direction.Value);
                }
            }

            // 2. Lấy danh sách route theo direction
            var routes = train.Line.Routes
                .Where(r => r.Direction == direction)
                .OrderBy(r => r.SeqOrder)
                .ToList();

            if (routes.Count == 0)
                throw new AppException(ErrorCode.BadRequest, "No routes found for current direction", StatusCodes.Status400BadRequest);

            // 3. Lấy segmentIndex hiện tại từ Firebase
            var segmentIndexFirebase = await _trainStateStore.GetSegmentIndexAsync(trainId);
            int resolvedSegmentIndex;
            if (segmentIndexFirebase != null)
            {
                resolvedSegmentIndex = segmentIndexFirebase.Value;
            }
            else
            {
                int foundIndex = routes.FindIndex(r => r.FromStationId == train.CurrentStationId);
                resolvedSegmentIndex = foundIndex >= 0 ? foundIndex : 0;
            }

            if (resolvedSegmentIndex < 0 || resolvedSegmentIndex >= routes.Count)
                throw new AppException(ErrorCode.BadRequest, "Train segment index out of range", StatusCodes.Status400BadRequest);

            // 4. Chống nhảy trạm
            var expectedStationId = routes[resolvedSegmentIndex].ToStationId;
            if (!string.Equals(expectedStationId, stationId, StringComparison.OrdinalIgnoreCase))
            {
                _logger.Warning(
                    "⚠️ Unexpected station for TrainId={TrainId}. Expected: {ExpectedStationId}, Received: {ReceivedStationId}, Direction={Direction}, SegmentIndex={SegmentIndex}",
                    trainId, expectedStationId, stationId, direction, resolvedSegmentIndex);

                throw new AppException(
                    ErrorCode.BadRequest,
                    $"Unexpected station. Expected: {expectedStationId}, Received: {stationId}",
                    StatusCodes.Status400BadRequest);
            }

            var currentLeg = routes[resolvedSegmentIndex];

            // 5. Cập nhật trạng thái Train
            if (resolvedSegmentIndex == routes.Count - 1)
            {
                train.Status = TrainStatusEnum.Completed;
                await _trainStateStore.RemoveSegmentIndexAsync(trainId);
                _logger.Information("✅ Train {TrainId} completed journey at station {StationId}", trainId, stationId);
            }
            else
            {
                train.Status = TrainStatusEnum.ArrivedAtStation;
                await _trainStateStore.SetSegmentIndexAsync(trainId, resolvedSegmentIndex);
                _logger.Information("🚉 Train {TrainId} arrived at station {StationId}", trainId, stationId);
            }

            train.CurrentStationId = stationId;
            train.Latitude = currentLeg.ToStation?.Latitude;
            train.Longitude = currentLeg.ToStation?.Longitude;

            _trainRepository.Update(train);
            await _unitOfWork.SaveChangeAsync(_httpContextAccessor);

            // 6. Cập nhật shipment itineraries
            var allCandidates = await _shipmentItineraryRepository
                .GetAllWithCondition(x => x.TrainId == trainId && !x.IsCompleted && x.RouteId != null)
                .ToListAsync();

            var matchedItineraries = allCandidates
                .Where(i => train.Line.Routes.Any(r => r.Id == i.RouteId && r.ToStationId == stationId))
                .ToList();

            // ✅ Group by ShipmentId and select leg with lowest LegOrder
            var itinerariesToComplete = matchedItineraries
                .GroupBy(i => i.ShipmentId)
                .Select(g => g.OrderBy(i => i.LegOrder).First())
                .ToList();

            foreach (var itinerary in itinerariesToComplete)
            {
                var route = train.Line.Routes.FirstOrDefault(r => r.Id == itinerary.RouteId);
                var stationName = route?.ToStation?.StationNameVi ?? "Unknown";

                itinerary.IsCompleted = true;

                var messageLine = $"[Arrived at {stationName} - {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm}]";
                itinerary.Message = string.IsNullOrWhiteSpace(itinerary.Message)
                    ? messageLine
                    : $"{itinerary.Message}\n{messageLine}";

                _shipmentItineraryRepository.Update(itinerary);

                // ✅ Nếu tất cả legs của shipment đã hoàn tất → mark shipment completed
                /*var shipment = await _shipmentRepository.GetByIdAsync(itinerary.ShipmentId);
                if (shipment != null)
                {
                    if (await _shipmentItineraryRepository
                        .GetAllWithCondition(x => x.ShipmentId == shipment.Id && !x.IsCompleted)
                        .AnyAsync() == false)
                    {
                        shipment.ShipmentStatus = ShipmentStatusEnum.Delivered;

                        _shipmentTrackingRepository.Add(new ShipmentTracking
                        {
                            ShipmentId = shipment.Id,
                            CurrentShipmentStatus = ShipmentStatusEnum.Delivered,
                            Status = "Delivered",
                            EventTime = DateTimeOffset.UtcNow,
                            Note = "All legs completed. Shipment delivered."
                        });

                        _shipmentRepository.Update(shipment);
                    }
                }*/
            }

            if (itinerariesToComplete.Count > 0)
            {
                await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
                _logger.Information("✅ Updated {Count} shipment itineraries at station {StationId}", itinerariesToComplete.Count, stationId);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "❌ Error confirming train arrival: TrainId={TrainId}, StationId={StationId}", trainId, stationId);
            throw;
        }
    }

    public async Task<TrainDto> ScheduleTrainAsync(string trainIdOrCode, bool startFromEnd = false)
    {
        // 1. Lấy thông tin train + line + routes + station
        var train = await _trainRepository
            .GetAllWithCondition()
            .Include(t => t.Line).ThenInclude(l => l.Routes).ThenInclude(r => r.FromStation)
            .Include(t => t.Line).ThenInclude(l => l.Routes).ThenInclude(r => r.ToStation)
            .FirstOrDefaultAsync(t => t.Id == trainIdOrCode || t.TrainCode == trainIdOrCode);

        if (train == null)
            throw new Exception($"Không tìm thấy đoàn tàu với Id/Code: {trainIdOrCode}");

        // 2. Check trạng thái được phép reschedule
        var allowedStatuses = new[]
        {
        TrainStatusEnum.NotScheduled,
        TrainStatusEnum.Scheduled,
        TrainStatusEnum.AwaitingDeparture,
        TrainStatusEnum.Departed,
        TrainStatusEnum.InTransit,
        TrainStatusEnum.ArrivedAtStation,
    };

        if (!allowedStatuses.Contains(train.Status))
            throw new Exception($"Không thể schedule lại train {train.TrainCode} khi đang ở trạng thái {train.Status}");

        // 3. Lọc routes thuộc line này & sắp xếp theo SeqOrder
        var routes = train.Line?.Routes?
            .Where(r => r.LineId == train.LineId && r.FromStationId != null && r.ToStationId != null)
            .OrderBy(r => r.SeqOrder)
            .ToList();

        if (routes == null || routes.Count == 0)
            throw new Exception("Không tìm thấy tuyến đường (routes) hợp lệ cho đoàn tàu.");

        // 4. Lấy endpoints (đầu/cuối line)
        var (startStation, endStation) = ResolveEndpointsFromRoutes(routes);

        // 5. Chọn trạm xuất phát
        var chosen = startFromEnd ? endStation : startStation;
        if (chosen == null)
            throw new Exception("Không tìm thấy trạm xuất phát hợp lệ.");

        // 5.1 Kiểm tra endpoint đã có train khác chiếm chưa
        var otherTrainAtSameStation = await _trainRepository
            .GetAllWithCondition()
            .Where(t => t.LineId == train.LineId
                     && t.Id != train.Id
                     && t.CurrentStationId == chosen.Id)
            .AnyAsync();

        if (otherTrainAtSameStation)
            throw new Exception("Không thể schedule: đã có đoàn tàu khác đang ở đầu tuyến này, vui lòng chọn hướng ngược lại.");

        _logger.Information("🚆 Train {TrainId} scheduled to start at station {StationId}.", train.Id, chosen.Id);

        // 6. Reset trạng thái trong DB
        train.CurrentStationId = chosen.Id;
        train.Latitude = chosen.Latitude;
        train.Longitude = chosen.Longitude;
        train.Status = TrainStatusEnum.Scheduled;   // chỉ schedule, chưa chạy
        train.LastUpdatedAt = DateTimeOffset.UtcNow;

        _trainRepository.Update(train);
        await _trainRepository.SaveChangesAsync();

        // 7. Reset state trong Firebase
        await _trainStateStore.RemoveAllTrainStateAsync(train.Id);

        // Set lại Direction (để Confirm/Start có hướng đúng)
        var direction = InferTrainDirectionFromCurrentStation(train, train.CurrentStationId!);
        await _trainStateStore.SetDirectionAsync(train.Id, direction);

        // ❗ KHÔNG set SegmentIndex = 0 khi reset — phải bỏ/clear
        await _trainStateStore.RemoveSegmentIndexAsync(train.Id);

        // ❗ KHÔNG set StartTime — phải bỏ/clear
        await _trainStateStore.RemoveStartTimeAsync(train.Id);

        // ✅ Lưu PositionResult để client đọc ngay ở trạng thái Scheduled
        var position = new TrainPositionResult
        {
            TrainId = train.Id,
            Latitude = train.Latitude ?? 0,
            Longitude = train.Longitude ?? 0,
            Status = train.Status.ToString(), // "Scheduled"
            ProgressPercent = 0,
            FromStation = chosen.StationNameVi,
            ToStation = null,
            Path = new List<GeoPoint>()
            // StartTime intentionally omitted/empty in reset
        };
        await _trainStateStore.SetPositionResultAsync(train.Id, position);

        _logger.Information("🚆 Train {TrainId} rescheduled at station {StationId} and state reset in Firebase",
            train.Id, train.CurrentStationId);

        // 8. Trả DTO
        return new TrainDto
        {
            Id = train.Id,
            TrainCode = train.TrainCode,
            CurrentStationId = train.CurrentStationId,
            CurrentStationLat = train.Latitude,
            CurrentStationLng = train.Longitude,
            Status = train.Status
        };
    }

    public async Task<TrainPositionResult> GetTrainPositionAsync1(string trainId)
    {
        var direction = await _trainStateStore.GetDirectionAsync(trainId)
            ?? throw new AppException(ErrorCode.BadRequest, "Train direction not initialized.", StatusCodes.Status400BadRequest);

        var train = await _trainRepository.GetTrainWithRoutesAsync(trainId, direction);
        var segmentIndex = await _trainStateStore.GetSegmentIndexAsync(trainId);
        var startTime = await _trainStateStore.GetStartTimeAsync(trainId);

        if (segmentIndex == null || startTime == null)
            throw new AppException(ErrorCode.BadRequest, "Train state not found");

        var routes = train.Line.Routes
            .Where(r => r.Direction == direction)
            .OrderBy(r => r.SeqOrder)
            .ToList();

        var routePolylines = BuildRoutePolylines(routes, new Dictionary<string, List<GeoPoint>>());
        double avgSpeedKmH = train.TopSpeedKmH ?? 100;

        var result = TrainPositionCalculator.CalculatePosition(
            train,
            routePolylines,
            startTime.Value,
            segmentIndex.Value,
            direction,
            avgSpeedKmH
        );

        // Path = danh sách station points
        result.Path = routes.Select(r => new GeoPoint
        {
            Latitude = r.FromStation.Latitude ?? 0,
            Longitude = r.FromStation.Longitude ?? 0
        }).ToList();

        result.AdditionalData = null;
        return result;
    }

    public async Task<object> GetTrainAdditionalDataAsync(string trainId)
    {
        var direction = await _trainStateStore.GetDirectionAsync(trainId)
            ?? throw new AppException(ErrorCode.BadRequest, "Train direction not initialized.", StatusCodes.Status400BadRequest);

        var train = await _trainRepository.GetTrainWithRoutesAsync(trainId, direction);
        var segmentIndex = await _trainStateStore.GetSegmentIndexAsync(trainId);
        var startTime = await _trainStateStore.GetStartTimeAsync(trainId);

        if (segmentIndex == null || startTime == null)
            throw new AppException(ErrorCode.BadRequest, "Train state not found");

        // Lấy routes theo direction
        var routes = train.Line.Routes
            .Where(r => r.Direction == direction)
            .OrderBy(r => r.SeqOrder)
            .ToList();

        // Build polyline cho từng đoạn tuyến
        var routePolylines = BuildRoutePolylines(routes, new Dictionary<string, List<GeoPoint>>());
        var fullPath = TrainPositionCalculator.BuildFullPath(routes, routePolylines);

        // Shipments
        var shipments = await _trainRepository.GetLoadedShipmentsByTrainAsync(train.Id);

        // Trả về đúng AdditionalData
        var additionalData = new
        {
            FullPath = fullPath,
            Shipments = shipments.Select(s => new
            {
                s.Id,
                s.TrackingCode,
                s.TotalWeightKg,
                s.TotalVolumeM3,
                s.ShipmentStatus,
                Parcels = s.Parcels.Select(p => new
                {
                    p.Id,
                    p.ParcelCode,
                    p.Status
                }).ToList()
            }).ToList(),
            Parcels = shipments.SelectMany(s => s.Parcels)
                               .Select(p => new { p.Id, p.ParcelCode, p.Status })
                               .ToList()
        };

        return additionalData;
    }
    #region Helper Methods
    private DirectionEnum InferTrainDirectionFromCurrentStation(MetroTrain train, string stationId)
    {
        var forwardRoutes = train.Line!.Routes!
            .Where(r => r.Direction == DirectionEnum.Forward)
            .OrderBy(r => r.SeqOrder)
            .ToList();

        var backwardRoutes = train.Line.Routes
            .Where(r => r.Direction == DirectionEnum.Backward)
            .OrderBy(r => r.SeqOrder)
            .ToList();

        // Lấy endpoint thật
        var forwardStart = forwardRoutes.First().FromStationId;
        var forwardEnd = forwardRoutes.Last().ToStationId;
        var backwardStart = backwardRoutes.First().FromStationId;
        var backwardEnd = backwardRoutes.Last().ToStationId;

        if (stationId == forwardStart) return DirectionEnum.Forward;
        if (stationId == forwardEnd) return DirectionEnum.Backward;
        if (stationId == backwardStart) return DirectionEnum.Backward;
        if (stationId == backwardEnd) return DirectionEnum.Forward;

        // Nếu ở giữa tuyến → xác định theo chiều chứa station
        var inForward = forwardRoutes.Any(r => r.FromStationId == stationId || r.ToStationId == stationId);
        var inBackward = backwardRoutes.Any(r => r.FromStationId == stationId || r.ToStationId == stationId);

        if (inForward && !inBackward) return DirectionEnum.Forward;
        if (inBackward && !inForward) return DirectionEnum.Backward;
        if (inForward && inBackward) return DirectionEnum.Forward; // ưu tiên Forward

        throw new AppException(ErrorCode.BadRequest, $"Cannot determine direction from station {stationId}", StatusCodes.Status400BadRequest);
    }

    private bool IsSameCoordinate(double? lat1, double? lng1, double? lat2, double? lng2, double threshold = 0.0001)
    {
        return lat1.HasValue && lng1.HasValue && lat2.HasValue && lng2.HasValue &&
               Math.Abs(lat1.Value - lat2.Value) < threshold &&
               Math.Abs(lng1.Value - lng2.Value) < threshold;
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

    private static void CompleteLeg(ShipmentItinerary leg, string toStation)
    {
        leg.IsCompleted = true;
        var messageLine = $"[Arrived at {toStation} - {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm}]";
        leg.Message = string.IsNullOrEmpty(leg.Message)
            ? messageLine
            : $"{leg.Message}\n{messageLine}";
    }

    private static ShipmentTracking CreateTracking(string shipmentId, ShipmentStatusEnum status, string note = "")
    {
        return new ShipmentTracking
        {
            ShipmentId = shipmentId,
            CurrentShipmentStatus = status,
            Status = status.ToString(),
            EventTime = DateTimeOffset.UtcNow,
            Note = string.IsNullOrEmpty(note) ? $"Shipment moved to {status}" : note
        };
    }


    private (Station? start, Station? end) ResolveEndpointsFromRoutes(IReadOnlyList<Route> allRoutes)
    {
        if (allRoutes == null || allRoutes.Count == 0)
            return (null, null);

        // 1) Thử theo Direction = Forward (nếu có)
        var forward = allRoutes
            .Where(r => r.Direction == DirectionEnum.Forward
                        && r.FromStation != null && r.ToStation != null)
            .OrderBy(r => r.SeqOrder)
            .ToList();

        if (forward.Count > 0 &&
            forward.First().FromStation != null &&
            forward.Last().ToStation != null)
        {
            return (forward.First().FromStation, forward.Last().ToStation);
        }

        // 2) Fallback: tính endpoints bằng degree trong đồ thị vô hướng
        // Chuẩn hoá cạnh (a,b) => (min(a,b), max(a,b)) rồi Distinct để không đếm đôi 01-02 & 02-01
        var undirectedEdges = allRoutes
            .Where(r => r.FromStationId != null && r.ToStationId != null)
            .Select(r =>
            {
                var a = r.FromStationId!;
                var b = r.ToStationId!;
                return string.CompareOrdinal(a, b) <= 0 ? (a, b) : (b, a);
            })
            .Distinct()
            .ToList();

        // Xây adjacency
        var adj = new Dictionary<string, HashSet<string>>();
        void Add(string u, string v)
        {
            if (!adj.TryGetValue(u, out var set))
            {
                set = new HashSet<string>();
                adj[u] = set;
            }
            set.Add(v);
        }

        foreach (var (a, b) in undirectedEdges)
        {
            Add(a, b);
            Add(b, a);
        }

        // Tìm 2 đầu mút: degree == 1
        var endpointIds = adj
            .Where(kvp => kvp.Value.Count == 1)
            .Select(kvp => kvp.Key)
            .Take(2)
            .ToList();

        if (endpointIds.Count == 2)
        {
            // Map về Station
            Station? FindStation(string id) =>
                allRoutes.SelectMany(r => new[] { r.FromStation, r.ToStation })
                         .FirstOrDefault(s => s != null && s.Id == id);

            var s1 = FindStation(endpointIds[0]);
            var s2 = FindStation(endpointIds[1]);

            // Nếu có SeqOrder, cố gắng phân định "đầu nhỏ" – "đầu lớn" để nhất quán
            int MinSeqOf(string stationId) =>
                allRoutes.Where(r => r.FromStationId == stationId || r.ToStationId == stationId)
                         .Select(r => r.SeqOrder)
                         .DefaultIfEmpty(int.MaxValue)
                         .Min();

            var (start, end) = MinSeqOf(endpointIds[0]) <= MinSeqOf(endpointIds[1])
                ? (s1, s2)
                : (s2, s1);

            return (start, end);
        }

        // 3) Nếu là vòng khép kín (degree != 1 cho mọi node) → chọn theo min/max SeqOrder
        var ordered = allRoutes
            .Where(r => r.FromStation != null && r.ToStation != null)
            .OrderBy(r => r.SeqOrder)
            .ToList();

        if (ordered.Count > 0)
            return (ordered.First().FromStation, ordered.Last().ToStation);

        return (null, null);
    }

    private List<RoutePolylineDto> BuildRoutePolylines(
    List<Route> routes,
    Dictionary<string, List<GeoPoint>> polylineData)
    {
        const int steps = 10; // số bước nội suy
        var list = new List<RoutePolylineDto>();

        foreach (var r in routes)
        {
            var key = $"{r.FromStation.StationNameVi}-{r.ToStation.StationNameVi}-{r.Direction}";

            var dto = new RoutePolylineDto
            {
                FromStation = r.FromStation.StationNameVi,
                ToStation = r.ToStation.StationNameVi,
                SeqOrder = r.SeqOrder,
                Direction = r.Direction,
                Polyline = polylineData.ContainsKey(key)
                    ? polylineData[key]
                    : Enumerable.Range(0, steps + 1).Select(s =>
                    {
                        var p = s / (double)steps;
                        var (lat, lng) = GeoUtils.Interpolate(
                            r.FromStation?.Latitude ?? 0,
                            r.FromStation?.Longitude ?? 0,
                            r.ToStation?.Latitude ?? 0,
                            r.ToStation?.Longitude ?? 0,
                            p);
                        return new GeoPoint
                        {
                            Latitude = lat,
                            Longitude = lng
                        };
                    }).ToList()
            };

            list.Add(dto);
        }

        return list;
    }

    #endregion
}