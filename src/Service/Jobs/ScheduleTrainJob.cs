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
            // No daily continuation needed since we removed date dependency
        }
    }

    private async Task InitCreateTrainSchedule(List<MetroTrain> metroTrains, List<MetroTimeSlot> timeSlots)
    {
        foreach (var train in metroTrains)
        {
            await CreateTrainScheduleForTrain(train, timeSlots);
        }
    }

    // This method can be called when creating a new train to add schedules for that train only
    public async Task CreateTrainScheduleForNewTrain(MetroTrain newTrain)
    {
        var timeSlots = await _metroTimeSlotRepository.GetAllWithCondition(
                x => !x.IsAbnormal && x.DeletedAt == null)
            .OrderBy(x => x.Shift)
            .ToListAsync();

        await CreateTrainScheduleForTrain(newTrain, timeSlots);
    }

    private async Task CreateTrainScheduleForTrain(MetroTrain train, List<MetroTimeSlot> timeSlots)
    {
        foreach (var timeSlot in timeSlots)
        {
            var direction = GetDirection(train, timeSlot);

            var trainSchedule = new TrainSchedule
            {
                TrainId = train.Id,
                TimeSlotId = timeSlot.Id,
                DepartureStationId = train.CurrentStationId,
                Direction = direction,
            };

            await _trainScheduleRepository.AddAsync(trainSchedule);
        }
    }

    private DirectionEnum GetDirection(MetroTrain train, MetroTimeSlot timeSlot)
    {
        var isTrainOdd = !train.IsTrainCodeEven(); // Odd train code
        var isShiftEven = timeSlot.IsShiftEven();

        // Odd train in odd shift (non-even shift) -> Direction 0 (Forward)
        // Even train in even shift -> Direction 1 (Backward)
        if (isTrainOdd && !isShiftEven)
        {
            return DirectionEnum.Forward; // Direction 0
        }
        else if (!isTrainOdd && isShiftEven)
        {
            return DirectionEnum.Backward; // Direction 1
        }
        else
        {
            // Default case for other combinations
            return DirectionEnum.Forward;
        }
    }
}