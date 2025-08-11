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

}