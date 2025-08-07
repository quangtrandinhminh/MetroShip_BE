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
        var today = DateOnly.FromDateTime(DateTime.Now);
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
            _logger.Information("No train schedules found for today. Initializing scheduling process.");
            // Should call initial scheduling
            await IntiCreateTrainSchedule(trains.ToList(), timeSlots.ToList());
        }
        else
        {
            _logger.Information("Found {Count} train schedules for today. Proceeding with daily continuation scheduling.", trainScheduleCount);
            // Should call daily continuation scheduling  
            await ScheduleEveryday(trains.ToList(), timeSlots.ToList());
        }
    }

    private async Task IntiCreateTrainSchedule(List<MetroTrain> metroTrains, List<MetroTimeSlot> timeSlots)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);

        foreach (var train in metroTrains)
        {
            foreach (var timeSlot in timeSlots)
            {
                var direction = GetInitialDirection(train, timeSlot.Shift);

                var trainSchedule = new TrainSchedule
                {
                    TrainId = train.Id,
                    TimeSlotId = timeSlot.Id,
                    Date = today,
                    DepartureStationId = train.CurrentStationId,
                    Direction = direction,
                };

                await _trainScheduleRepository.AddAsync(trainSchedule);
            }
        }
    }

    private DirectionEnum GetInitialDirection(MetroTrain train, ShiftEnum shift)
    {
        var isEven = train.IsTrainCodeEven();

        return shift switch
        {
            ShiftEnum.Morning => isEven ? DirectionEnum.Backward : DirectionEnum.Forward,  // T02:1, T01:0
            ShiftEnum.Afternoon => isEven ? DirectionEnum.Forward : DirectionEnum.Backward, // T02:0, T01:1
            ShiftEnum.Evening => isEven ? DirectionEnum.Backward : DirectionEnum.Forward,   // T02:1, T01:0
            ShiftEnum.Night => isEven ? DirectionEnum.Forward : DirectionEnum.Backward,     // T02:0, T01:1
            _ => DirectionEnum.Forward
        };
    }

    private async Task ScheduleEveryday(List<MetroTrain> metroTrains, List<MetroTimeSlot> timeSlots)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var yesterday = today.AddDays(-1);

        // Get yesterday's night schedules to check direction conflicts
        var yesterdayNightSchedules = await _trainScheduleRepository.GetAllAsync();
        var lastNightSchedules = yesterdayNightSchedules
            .Where(ts => ts.Date == yesterday)
            .Join(timeSlots.Where(ts => ts.Shift == ShiftEnum.Night),
                  schedule => schedule.TimeSlotId,
                  timeSlot => timeSlot.Id,
                  (schedule, timeSlot) => schedule)
            .ToList();

        var newSchedules = new List<TrainSchedule>();

        foreach (var train in metroTrains)
        {
            foreach (var timeSlot in timeSlots)
            {
                var direction = GetContinuationDirection(train, timeSlot.Shift, lastNightSchedules);

                var trainSchedule = new TrainSchedule
                {
                    TrainId = train.Id,
                    TimeSlotId = timeSlot.Id,
                    Date = today,
                    DepartureStationId = train.CurrentStationId,
                    Direction = direction,
                };

                newSchedules.Add(trainSchedule);
            }
        }

        // Save all new schedules
        foreach (var schedule in newSchedules)
        {
            await _trainScheduleRepository.AddAsync(schedule);
        }
    }

    private DirectionEnum GetContinuationDirection(MetroTrain train, ShiftEnum currentShift, List<TrainSchedule> lastNightSchedules)
    {
        var lastNightDirection = lastNightSchedules
            .FirstOrDefault(s => s.TrainId == train.Id)?.Direction;

        if (currentShift == ShiftEnum.Morning && lastNightDirection.HasValue)
        {
            // If train ran direction X last night, it cannot run direction X this morning
            return lastNightDirection.Value == DirectionEnum.Forward
                ? DirectionEnum.Backward
                : DirectionEnum.Forward;
        }

        // For other shifts, follow the regular pattern
        return GetInitialDirection(train, currentShift);
    }
}