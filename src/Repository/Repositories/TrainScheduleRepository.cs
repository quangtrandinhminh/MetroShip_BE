using MetroShip.Repository.Base;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using MetroShip.Utility.Enums;
using Microsoft.EntityFrameworkCore;
using AppDbContext = MetroShip.Repository.Infrastructure.AppDbContext;

namespace MetroShip.Repository.Repositories;

public class TrainScheduleRepository : BaseRepository<TrainSchedule>, ITrainScheduleRepository
{
    private readonly AppDbContext _context;
    public TrainScheduleRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IList<TrainSchedule>> GetTrainSchedulesByTrainListAsync(IList<string> trainIds)
    {
        if (trainIds == null || !trainIds.Any())
        {
            return new List<TrainSchedule>();
        }

        return await _context.TrainSchedule
            .Where(ts => trainIds.Contains(ts.TrainId))
            .Select(ts => new TrainSchedule
            {
                Id = ts.Id,
                TrainId = ts.TrainId,
                TimeSlotId = ts.TimeSlotId,
                LineId = ts.LineId,
                LineName = ts.Train.Line.LineNameVi,
                DepartureStationId = ts.DepartureStationId,
                DepartureStationName = ts.Train.Line.Routes
                    .FirstOrDefault(r => r.FromStationId == ts.DepartureStationId).FromStation.StationNameVi,
                DestinationStationId = ts.DestinationStationId,
                DestinationStationName = ts.Train.Line.Routes
                    .FirstOrDefault(r => r.FromStationId == ts.DestinationStationId).FromStation.StationNameVi,
                Direction = ts.Direction,
                ArrivalTime = ts.ArrivalTime,
                DepartureTime = ts.DepartureTime,
                Date = ts.Date,
                Shift = ts.TimeSlot.Shift,
            })
            .ToListAsync();
    }

    public async Task<DirectionEnum?> GetTrainDirectionByTrainIdAsync(string trainId)
    {
        if (string.IsNullOrWhiteSpace(trainId))
            return null;

        return await _context.TrainSchedule
            .Where(x => x.TrainId == trainId)
            .OrderByDescending(x => x.Date) // lấy chuyến mới nhất
            .Select(x => (DirectionEnum?)x.Direction)
            .FirstOrDefaultAsync();
    }
}