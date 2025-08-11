using MetroShip.Repository.Base;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using MetroShip.Utility.Enums;
using MetroShip.Utility.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;
using System.Linq;

namespace MetroShip.Service.Jobs;

public class ScheduleTrainJob(IServiceProvider serviceProvider) : IJob
{
    private readonly ITrainRepository _trainRepository = serviceProvider.GetRequiredService<ITrainRepository>();
    private readonly IMetroTimeSlotRepository _metroTimeSlotRepository = serviceProvider.GetRequiredService<IMetroTimeSlotRepository>();
    private readonly IBaseRepository<TrainSchedule> _trainScheduleRepository = serviceProvider.GetRequiredService<IBaseRepository<TrainSchedule>>();
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
    private readonly IStationRepository _stationRepository = serviceProvider.GetRequiredService<IStationRepository>();

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.Information("Starting train scheduling job at {Time}", DateTime.Now);
        var trainScheduleCount = _trainScheduleRepository.GetAll().Count();
        var timeSlots = await _metroTimeSlotRepository.GetAllWithCondition(
                x => !x.IsAbnormal && x.DeletedAt == null)
            .OrderBy(x => x.Shift)
            .ToListAsync();
        var trains = await _trainRepository.GetAllWithCondition(
                           x => x.DeletedAt == null && x.IsActive)
            .OrderBy(x => x.TrainCode)
            .ToListAsync();

        if (trainScheduleCount == 0)
        {
            _logger.Information("No train schedules found. Initializing scheduling process for all trains.");
            await InitCreateTrainSchedule(trains.ToList(), timeSlots.ToList());
        }
        else
        {
            _logger.Information("Found {Count} train schedules. Scheduling process already initialized.", trainScheduleCount);
        }
    }

    private async Task InitCreateTrainSchedule(List<MetroTrain> metroTrains, List<MetroTimeSlot> timeSlots)
    {
        var lineIds = metroTrains.Select(x => x.LineId).Distinct().ToList();
        var stationDictionary = await _stationRepository.GetStartAndEndStationIdsOfRouteAsync(lineIds);
        foreach (var train in metroTrains)
        {
            await CreateTrainScheduleForTrain(train, timeSlots, stationDictionary);
        }
        await _trainScheduleRepository.SaveChangesAsync();
    }

    // This method can be called when creating a new train to add schedules for that train only
    public async Task CreateTrainScheduleForNewTrain(MetroTrain newTrain)
    {
        var timeSlots = await _metroTimeSlotRepository.GetAllWithCondition(
                x => !x.IsAbnormal && x.DeletedAt == null)
            .OrderBy(x => x.Shift)
            .ToListAsync();

        var stationDictionary = await _stationRepository.GetStartAndEndStationIdsOfRouteAsync(newTrain.LineId);
        await CreateTrainScheduleForTrain(newTrain, timeSlots, stationDictionary);
        await _trainScheduleRepository.SaveChangesAsync();
    }

    private async Task CreateTrainScheduleForTrain(MetroTrain train, List<MetroTimeSlot> timeSlots,
        Dictionary<(string, DirectionEnum), (string, string)> stationDictionary)
    {
        foreach (var timeSlot in timeSlots)
        {
            var direction = GetDirection(train, timeSlot);
            if (!stationDictionary.TryGetValue((train.LineId, direction), out var stations))
            {
                _logger.Warning("No station data found for train {TrainCode} in direction {Direction}", 
                    train.TrainCode, direction);
                continue; // Skip if no station data available
            }

            var trainSchedule = new TrainSchedule
            {
                TrainId = train.Id,
                TimeSlotId = timeSlot.Id,
                LineId = train.LineId,
                DepartureStationId = stations.Item1,
                Direction = direction,
                DestinationStationId = stations.Item2,
            };

            await _trainScheduleRepository.AddAsync(trainSchedule);
        }
    }

    private DirectionEnum GetDirection(MetroTrain train, MetroTimeSlot timeSlot)
    {
        var isTrainOdd = !train.IsTrainCodeEven();
        var isShiftEven = timeSlot.IsShiftEven();

        if (isTrainOdd != isShiftEven)
        {
            // Trường hợp 1: Tàu lẻ (true) và Ca chẵn (true) -> false -> đi vào else
            // Trường hợp 2: Tàu chẵn (false) và Ca lẻ (false) -> false -> đi vào else
            // Trường hợp 3: Tàu lẻ (true) và Ca lẻ (false) -> true -> đi vào if
            // Trường hợp 4: Tàu chẵn (false) và Ca chẵn (true) -> true -> đi vào if
            return DirectionEnum.Forward; 
        }
        else
        {
            return DirectionEnum.Backward; 
        }
    }
}