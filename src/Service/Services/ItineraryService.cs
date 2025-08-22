using MetroShip.Repository.Base;
using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using MetroShip.Repository.Repositories;
using MetroShip.Service.ApiModels.Graph;
using MetroShip.Service.ApiModels.Shipment;
using MetroShip.Service.BusinessModels;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Mapper;
using MetroShip.Service.Validations;
using MetroShip.Utility.Config;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Enums;
using MetroShip.Utility.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace MetroShip.Service.Services;

public class ItineraryService(IServiceProvider serviceProvider) : IItineraryService
{
    private readonly IShipmentRepository _shipmentRepository = serviceProvider.GetRequiredService<IShipmentRepository>();
    private readonly IShipmentItineraryRepository _shipmentItineraryRepository = serviceProvider.GetRequiredService<IShipmentItineraryRepository>();
    private readonly IMetroTimeSlotRepository _metroTimeSlotRepository = serviceProvider.GetRequiredService<IMetroTimeSlotRepository>();
    private readonly ITrainScheduleRepository _trainScheduleRepository = serviceProvider.GetRequiredService<ITrainScheduleRepository>();
    private readonly ISystemConfigRepository _systemConfigRepository = serviceProvider.GetRequiredService<ISystemConfigRepository>();
    private readonly IMapperlyMapper _mapperlyMapper = serviceProvider.GetRequiredService<IMapperlyMapper>();
    private readonly IUnitOfWork _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
    private readonly IHttpContextAccessor _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
    private readonly IStationRepository _stationRepository = serviceProvider.GetRequiredService<IStationRepository>();
    private readonly IBaseRepository<CategoryInsurance> _categoryInsuranceRepository = serviceProvider.GetRequiredService<IBaseRepository<CategoryInsurance>>();
    private readonly IPricingService _pricingService = serviceProvider.GetRequiredService<IPricingService>();
    private readonly IMemoryCache _memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
    private MetroGraph _metroGraph;
    private const string CACHE_KEY = nameof(MetroGraph);
    private const int CACHE_EXPIRY_MINUTES = 30;

    public async Task<DateTimeOffset> CheckEstArrivalTime(BestPathGraphResponse pathResponse, string currentSlotId, DateOnly date)
    {
        _logger.Information("Checking estimated arrival time for path: {@PathResponse}", pathResponse);

        // for each line, get next time slot
        var nextTimeSlots = await _metroTimeSlotRepository.GetAllWithCondition(
                       x => !x.IsAbnormal && x.DeletedAt == null)
            .ToListAsync();

        if (nextTimeSlots == null || !nextTimeSlots.Any())
        {
            _logger.Warning("No available time slots found for estimation.");
            throw new AppException(
            ErrorCode.NotFound,
            ResponseMessageShipment.TIME_SLOT_NOT_FOUND,
            StatusCodes.Status404NotFound);
        }

        // Find the current time slot in the list
        var currentSlot = nextTimeSlots.FirstOrDefault(x => x.Id == currentSlotId);
        if (currentSlot == null)
        {
            _logger.Warning("Current time slot with ID {CurrentSlotId} not found in available time slots.",
                               currentSlotId);
            throw new AppException(
            ErrorCode.NotFound,
            ResponseMessageShipment.TIME_SLOT_NOT_FOUND,
            StatusCodes.Status404NotFound);
        }

        // -1 is eliminate if only 1 line, does not change to next slot
        for (var i = 0; i < pathResponse.TotalMetroLines - 1; i++)
        {
            (date, currentSlot) = GetNextSlot(date, currentSlot, nextTimeSlots);
        }

        // Normalize date to start of the day, gmt +7, include if for loop not run
        var result = new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.FromHours(7));

        // + hour from 0 to close time
        var hour = currentSlot.CloseTime.ToTimeSpan().TotalHours;
        // calculate how many hour from current slot
        if (currentSlot.CloseTime.ToTimeSpan().TotalHours > currentSlot.OpenTime.ToTimeSpan().TotalHours)
        {
            // date to be 00:00 and add hour
            result = result.AddHours(hour);
        }
        else
        {
            result = result.AddDays(1).AddHours(hour);
        }

        // cus timeonly from slot is +7, convert to +0
        result = result.ToUniversalTime();
        return result;
    }

    /*public async Task<List<ShipmentAvailableTimeSlotResponse>> CheckAvailableTimeSlotsAsync
        (ShipmentAvailableTimeSlotsRequest request)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(request.ShipmentId);
        if (shipment?.Parcels == null)
        {
            _logger.Warning("Shipment or its parcels are null.");
        }

        var availableSlotsRequest = new ShipmentRepository.CheckAvailableTimeSlotsRequest
        {
            ShipmentId = request.ShipmentId,
            MaxAttempts = request.MaxAttempts > 0 ? request.MaxAttempts : 3
        };

        var availableSlots = await _shipmentRepository.FindAvailableTimeSlotsAsync(availableSlotsRequest);

        var validSlots = availableSlots
            .Where(s => s.StartDate != default && s.Date != default)
            .ToList();

        if (!validSlots.Any())
        {
            _logger.Warning("No valid time slots found for shipment {ShipmentId}", request.ShipmentId);
            return new List<ShipmentAvailableTimeSlotResponse>();
        }

        var mappedSlots = validSlots.Select(slot =>
        {
            var slotDetail = new MetroTimeSlotResponse
            {
                Id = slot.TimeSlotId,
                Shift = slot.Shift,
                DayOfWeek = slot.DayOfWeek,
                SpecialDate = slot.SpecialDate,
                OpenTime = slot.OpenTime,
                CloseTime = slot.CloseTime,
                IsAbnormal = slot.IsAbnormal,
                ScheduleBeforeShiftMinutes = 30
            };

            return (
                StartDate: slot.StartDate,
                Date: slot.Date,
                SlotDetail: slotDetail,
                RemainingVolumeM3: slot.RemainingVolumeM3,
                RemainingWeightKg: slot.RemainingWeightKg,
                ShipmentStatus: ShipmentStatusEnum.AwaitingDropOff,
                ParcelIds: slot.ParcelIds ?? new List<string>()
            );
        }).ToList();

        return _mapperlyMapper.MapToAvailableTimeSlotResponseList(mappedSlots);
    }*/

    private record CalculatedItinerary
    {
        public DateOnly? Date { get; init; } = null;
        public string TimeSlotId { get; init; } = string.Empty;
        public string ShipmentId { get; init; } = string.Empty;
        public decimal? TotalWeightKg { get; init; } = null;
        public decimal? TotalVolumeM3 { get; init; } = null;
        public ShiftEnum? Shift { get; init; } = null;
        public string? LineId { get; init; } = null;
        public string? RouteId { get; init; } = null;
    }

    public async Task<List<ItineraryResponse>> CheckAvailableTimeSlotsAsync(
        string shipmentId, int maxAttempt)
    {
        _logger.Information("Checking available time slots for shipment ID: {@ShipmentId}, Max Attempts: {@MaxAttempts}",
            shipmentId, maxAttempt);

        // 1. Get shipment with itineraries and routes
        var shipment = await _shipmentRepository.GetAll()
            .Include(x => x.ShipmentItineraries.OrderBy(i => i.LegOrder))
            .ThenInclude(x => x.Route)
            .FirstOrDefaultAsync(x => x.Id == shipmentId && x.DeletedAt == null);

        if (shipment == null)
        {
            throw new AppException(
                ErrorCode.NotFound,
                ResponseMessageShipment.SHIPMENT_NOT_FOUND,
                StatusCodes.Status400BadRequest);
        }

        // 2. Get system configuration
        var maxWeightKg = decimal.Parse(_systemConfigRepository
            .GetSystemConfigValueByKey(nameof(SystemConfigSetting.Instance.MAX_CAPACITY_PER_LINE_KG)));
        var maxVolumeM3 = decimal.Parse(_systemConfigRepository
            .GetSystemConfigValueByKey(nameof(SystemConfigSetting.Instance.MAX_CAPACITY_PER_LINE_M3)));

        // 3. Get all available time slots
        var timeSlots = await _metroTimeSlotRepository.GetAllWithCondition(
                x => !x.IsAbnormal && x.DeletedAt == null)
            .OrderBy(x => x.Shift)
            .ToListAsync();

        // 4. Bulk fetch all relevant itineraries for capacity calculation
        var capacityData = await BulkFetchCapacityDataAsync(shipment, maxAttempt);

        // 5. Process slot assignment for each itinerary
        await ProcessSlotAssignmentAsync(shipment, capacityData, timeSlots, maxWeightKg, maxVolumeM3, maxAttempt);

        // 6. Assign train schedules after time slot assignment
        await AssignTrainSchedulesToItinerariesAsync(shipment);

        // 7. Save changes to database
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
        return _mapperlyMapper.MapToListShipmentItineraryResponse(shipment.ShipmentItineraries.ToList());
    }


    /// <summary>
    /// Bulk fetch all itineraries that could affect capacity calculation
    /// This is the key performance optimization
    /// </summary>
    private async Task<Dictionary<CapacityKey, List<CalculatedItinerary>>> BulkFetchCapacityDataAsync(
        Shipment shipment, int maxAttempts)
    {
        // Define valid shipment statuses for capacity calculation
        var validStatuses = new[]
        {
            ShipmentStatusEnum.AwaitingPayment,
            ShipmentStatusEnum.AwaitingDropOff,
            ShipmentStatusEnum.PickedUp,
            ShipmentStatusEnum.InTransit,
            ShipmentStatusEnum.ToReturn,
            ShipmentStatusEnum.Returning
        };

        // Calculate date range for bulk fetch
        var startDate = new DateOnly(
                shipment.ScheduledDateTime.Value.Year,
                shipment.ScheduledDateTime.Value.Month,
                shipment.ScheduledDateTime.Value.Day);
        var endDate = startDate.AddDays(4 * maxAttempts); // 4 shifts per day

        // Get all lines involved in this shipment
        var routeIds = shipment.ShipmentItineraries.Select(i => i.RouteId).Distinct().ToList();

        // Bulk fetch ALL relevant itineraries for capacity calculation
        var allRelevantItineraries = await _shipmentItineraryRepository.GetAllWithCondition(
            x => routeIds.Contains(x.RouteId) &&
                 validStatuses.Contains(x.Shipment.ShipmentStatus) &&
                 x.Date.HasValue &&
                 x.Date.Value >= startDate &&
                 x.Date.Value <= endDate &&
                 x.DeletedAt == null)
            .Include(x => x.Shipment)
            .Include(x => x.Route)
            .Include(x => x.TimeSlot)
            .Select(x => new CalculatedItinerary
            {
                Date = x.Date.Value,
                TimeSlotId = x.TimeSlotId,
                ShipmentId = x.ShipmentId,
                TotalWeightKg = x.Shipment.TotalWeightKg,
                TotalVolumeM3 = x.Shipment.TotalVolumeM3,
                Shift = x.TimeSlot.Shift,
                LineId = x.Route.LineId,
                RouteId = x.RouteId
            })
            .ToListAsync();

        // Group by (RouteId, Date, Shift) for fast lookup
        return allRelevantItineraries
            .GroupBy(x => new CapacityKey(x.RouteId, x.Date.Value, x.Shift.Value))
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    /// <summary>
    /// Process slot assignment for all itineraries in the shipment
    /// </summary>
    private async Task ProcessSlotAssignmentAsync(
        Shipment shipment,
        Dictionary<CapacityKey, List<CalculatedItinerary>> capacityData,
        List<MetroTimeSlot> timeSlots,
        decimal maxWeightKg,
        decimal maxVolumeM3,
        int maxAttempts)
    {
        // Initialize with shipment's scheduled date and shift
        var currentDate = new DateOnly(
                shipment.ScheduledDateTime.Value.Year,
                shipment.ScheduledDateTime.Value.Month,
                shipment.ScheduledDateTime.Value.Day);
        var currentSlot = timeSlots.FirstOrDefault(ts => ts.Shift == shipment.ScheduledShift);

        if (currentSlot == null)
        {
            throw new AppException(
                ErrorCode.NotFound,
                $"No time slot found for shift {shipment.ScheduledShift}",
                StatusCodes.Status400BadRequest);
        }

        string previousLineId = null;

        foreach (var itinerary in shipment.ShipmentItineraries.OrderBy(i => i.LegOrder))
        {
            var currentLineId = itinerary.Route.LineId;

            // If not first itinerary and different line, find next available slot
            if (itinerary.LegOrder > 1 && currentLineId != previousLineId)
            {
                (currentDate, currentSlot) = GetNextSlot(currentDate, currentSlot, timeSlots);
            }

            // Find available slot with capacity check
            var (assignedDate, assignedSlot) = await FindAvailableSlotWithCapacityAsync(
                currentDate, currentSlot, itinerary, shipment,
                capacityData, timeSlots, maxWeightKg, maxVolumeM3, maxAttempts);

            // Assign slot to itinerary
            itinerary.TimeSlotId = assignedSlot.Id;
            itinerary.Date = assignedDate;
            _shipmentItineraryRepository.Update(itinerary);

            // Update for next iteration
            currentDate = assignedDate;
            currentSlot = assignedSlot;
            previousLineId = currentLineId;

            _logger.Information("Assigned slot {SlotId} on {Date} to itinerary {ItineraryId} (Line: {LineId})",
                assignedSlot.Id, assignedDate, itinerary.Id, currentLineId);
        }
    }

    /// Find available slot with capacity check using pre-fetched data
    private async Task<(DateOnly, MetroTimeSlot)> FindAvailableSlotWithCapacityAsync(
        DateOnly startDate,
        MetroTimeSlot startSlot,
        ShipmentItinerary itinerary,
        Shipment shipment,
        Dictionary<CapacityKey, List<CalculatedItinerary>> capacityData,
        List<MetroTimeSlot> timeSlots,
        decimal maxWeightKg,
        decimal maxVolumeM3,
        int maxAttempts)
    {
        var currentDate = startDate;
        var currentSlot = startSlot;
        var attempts = 0;

        while (attempts < maxAttempts)
        {
            var capacityKey = new CapacityKey(itinerary.RouteId, currentDate, currentSlot.Shift);

            // Get existing capacity for this line/date/shift
            var existingItineraries = capacityData
                .GetValueOrDefault(capacityKey, new List<CalculatedItinerary>());

            var totalWeightKg = existingItineraries.Sum(i => i.TotalWeightKg ?? 0);
            var totalVolumeM3 = existingItineraries.Sum(i => i.TotalVolumeM3 ?? 0);

            // Check if adding this shipment exceeds capacity
            if (totalWeightKg + (shipment.TotalWeightKg ?? 0) <= maxWeightKg &&
                totalVolumeM3 + (shipment.TotalVolumeM3 ?? 0) <= maxVolumeM3)
            {
                // Add this shipment to capacity data for future iterations
                var newItinerary = new CalculatedItinerary
                {
                    Date = currentDate,
                    TimeSlotId = currentSlot.Id,
                    ShipmentId = shipment.Id,
                    TotalWeightKg = shipment.TotalWeightKg,
                    TotalVolumeM3 = shipment.TotalVolumeM3,
                    Shift = currentSlot.Shift,
                    LineId = itinerary.Route.LineId,
                    RouteId = itinerary.RouteId
                };

                if (!capacityData.ContainsKey(capacityKey))
                    capacityData[capacityKey] = new List<CalculatedItinerary>();
                capacityData[capacityKey].Add(newItinerary);

                return (currentDate, currentSlot);
            }

            // Try next slot
            (currentDate, currentSlot) = GetNextSlot(currentDate, currentSlot, timeSlots);
            attempts++;

            _logger.Debug("Attempt {Attempt}: Slot {SlotId} on {Date} has insufficient capacity for route {Route}",
                attempts, currentSlot.Id, currentDate, itinerary.Route.RouteNameEn);
        }

        _shipmentRepository.Delete(shipment);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
        throw new AppException(
            ErrorCode.BadRequest,
            $"No available slot found for itinerary on route {itinerary.Route.RouteNameEn} after {maxAttempts} attempts",
            StatusCodes.Status400BadRequest);
    }

    /// Get next slot
    private (DateOnly, MetroTimeSlot) GetNextSlot(
        DateOnly date,
        MetroTimeSlot currentSlot,
        List<MetroTimeSlot> timeSlots)
    {
        // Normalize date to start of the day, default to UTC+0
        //date = new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.Zero);
        var nextShift = currentSlot.Shift switch
        {
            ShiftEnum.Morning => ShiftEnum.Afternoon,
            ShiftEnum.Afternoon => ShiftEnum.Evening,
            ShiftEnum.Evening => ShiftEnum.Night,
            ShiftEnum.Night => ShiftEnum.Morning,
            _ => ShiftEnum.Morning
        };

        // If current slot is Night, move to next date's Morning
        var nextDate = currentSlot.Shift == ShiftEnum.Night ? date.AddDays(1) : date;
        var nextSlot = timeSlots.FirstOrDefault(ts => ts.Shift == nextShift);

        if (nextSlot == null)
        {
            throw new AppException(
                ErrorCode.NotFound,
                $"No time slot found for shift {nextShift}",
                StatusCodes.Status500InternalServerError);
        }

        return (nextDate, nextSlot);
    }

    private record CapacityKey(string RouteId, DateOnly Date, ShiftEnum Shift);

    /// <summary>
    /// Assign train schedules to all itineraries after time slot assignment
    /// </summary>
    private async Task AssignTrainSchedulesToItinerariesAsync(Shipment shipment)
    {
        _logger.Information("Assigning train schedules to itineraries for shipment {ShipmentId}", shipment.Id);

        // Group itineraries by LineId and Direction for efficient querying
        var itineraryGroups = shipment.ShipmentItineraries
            .GroupBy(i => new { i.Route.LineId, i.Route.Direction })
            .ToList();

        // Bulk fetch all relevant train schedules
        var allTrainSchedules = new Dictionary<(string LineId, DirectionEnum Direction), List<TrainSchedule>>();

        foreach (var group in itineraryGroups)
        {
            var lineId = group.Key.LineId;
            var direction = group.Key.Direction;

            var trainSchedules = await _trainScheduleRepository.GetAllWithCondition(
                ts => ts.LineId == lineId &&
                      ts.Direction == direction &&
                      ts.DeletedAt == null)
                .Include(ts => ts.Train)
                .Include(ts => ts.TimeSlot)
                .ToListAsync();

            allTrainSchedules[(lineId, direction)] = trainSchedules;
        }

        // Assign train schedules to each itinerary
        foreach (var itinerary in shipment.ShipmentItineraries)
        {
            var lineId = itinerary.Route.LineId;
            var direction = itinerary.Route.Direction;
            var timeSlotId = itinerary.TimeSlotId;

            if (allTrainSchedules.TryGetValue((lineId, direction), out var trainSchedules))
            {
                // Find train schedule that matches the assigned time slot
                var matchingSchedule = trainSchedules.FirstOrDefault(ts => ts.TimeSlotId == timeSlotId);

                if (matchingSchedule != null)
                {
                    itinerary.TrainId = matchingSchedule.TrainId;
                    itinerary.TrainScheduleId = matchingSchedule.Id;

                    _shipmentItineraryRepository.Update(itinerary);

                    _logger.Information("Assigned Train {TrainId} (Schedule {ScheduleId}) to itinerary {ItineraryId} on line {LineId}, direction {Direction}, timeSlot {TimeSlotId}",
                        matchingSchedule.TrainId, matchingSchedule.Id, itinerary.Id, lineId, direction, timeSlotId);
                }
                else
                {
                    _logger.Warning("No train schedule found for line {LineId}, direction {Direction}, timeSlot {TimeSlotId}",
                        lineId, direction, timeSlotId);

                    // Optional: throw exception or handle this case based on your business rules
                    /*throw new AppException(
                        ErrorCode.NotFound,
                        $"No train schedule found for line {lineId}, direction {direction}, timeSlot {timeSlotId}",
                        StatusCodes.Status400BadRequest);*/
                }
            }
            else
            {
                _logger.Warning("No train schedules found for line {LineId}, direction {Direction}", lineId, direction);
            }
        }
    }

    // change all itineraries from this slot and date to another slot and date (isComplete = false, nearest slot from now)
    public async Task ChangeItinerariesToNextSlotAsync(string shipmentId, DateOnly date, string currentSlotId)
    {
        _logger.Information("Changing itineraries for shipment {ShipmentId} to next slot on {Date} from slot {CurrentSlotId}",
                       shipmentId, date, currentSlotId);

        // Get all itineraries for the shipment
        var itineraries = await _shipmentItineraryRepository.GetAllWithCondition(
                       x => x.ShipmentId == shipmentId && x.Date == date 
                       && x.TimeSlotId == currentSlotId && x.DeletedAt == null)
            .ToListAsync();

        if (!itineraries.Any())
        {
            _logger.Warning("No itineraries found for shipment {ShipmentId} on date {Date} with slot {CurrentSlotId}",
                               shipmentId, date, currentSlotId);

            throw new AppException(
                ErrorCode.NotFound,
                ResponseMessageItinerary.ITINERARY_NOT_FOUND,
                StatusCodes.Status404NotFound);
        }

        // Get next time slot
        var nextTimeSlots = await _metroTimeSlotRepository.GetAllWithCondition(
                                  x => !x.IsAbnormal && x.DeletedAt == null)
            .ToListAsync();

        if (nextTimeSlots == null || !nextTimeSlots.Any())
        {
            _logger.Warning("No available time slots found for changing itineraries.");
            throw new AppException(
            ErrorCode.NotFound,
            ResponseMessageShipment.TIME_SLOT_NOT_FOUND,
            StatusCodes.Status404NotFound);
        }

        // Find the current time slot in the list
        var currentSlot = nextTimeSlots.FirstOrDefault(x => x.Id == currentSlotId);
        if (currentSlot == null)
        {
            _logger.Warning("Current time slot with ID {CurrentSlotId} not found in available time slots.",
                                              currentSlotId);
            throw new AppException(
            ErrorCode.NotFound,
            ResponseMessageShipment.TIME_SLOT_NOT_FOUND,
            StatusCodes.Status404NotFound);
        }

        // Get next slot
        (date, currentSlot) = GetNextSlot(date, currentSlot, nextTimeSlots);

        // Update all itineraries to the next slot and date
        foreach (var itinerary in itineraries)
        {
            itinerary.TimeSlotId = currentSlot.Id;
            itinerary.Date = date;
            itinerary.IsCompleted = false; // Set to false as per requirement

            _shipmentItineraryRepository.Update(itinerary);
        }

        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
    }

    public async Task<TotalPriceResponse> GetItineraryAndTotalPrice(TotalPriceCalcRequest request)
    {
        // Get station options
        var stationIds = await GetNearUserStations(request);

        // Find paths
        var pathResults = await FindOptimalPaths(stationIds.ToList(), request.DestinationStationId);

        // Calculate pricing
        var bestPathResponses = await CalculatePricingForPaths(pathResults, request);

        // Build response
        return BuildTotalPriceResponse(bestPathResponses, stationIds.ToList());
    }

    private async Task InitializeGraphAsync()
    {
        var cacheKey = CACHE_KEY;
        if (_memoryCache.TryGetValue(cacheKey, out MetroGraph? cachedGraph))
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
        _memoryCache.Set(cacheKey, _metroGraph, TimeSpan.FromMinutes(CACHE_EXPIRY_MINUTES));
    }

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
            nearStations.RemoveAll(s => s.StationId == request.DepartureStationId);

            // Ensure at least 2 stations are available (including departure)
            while (result.Count + nearStations.Count < 2 && maxDistanceInMeters < maxDistanceInMeters * 2)
            {
                // Extend distance by 1000 meters
                maxDistanceInMeters += 2000;
                nearStations = await _stationRepository.GetAllStationIdNearUser(
                    request.UserLatitude.Value, request.UserLongitude.Value, maxDistanceInMeters, maxStationCount);
                nearStations.RemoveAll(s => s.StationId == request.DepartureStationId);
            }

            // Add up to (maxStationCount - 1) nearest stations (excluding departure) -- max 6
            result.AddRange(nearStations.Take(maxStationCount).Select(_ => _.StationId));
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
            pathResponse.EstArrivalTime = CheckEstArrivalTime(pathResponse, request.TimeSlotId, date).Result;

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

}