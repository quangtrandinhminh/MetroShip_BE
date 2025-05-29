using MetroShip.Repository.Base;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using Microsoft.EntityFrameworkCore;
using AppDbContext = MetroShip.Repository.Infrastructure.AppDbContext;

namespace MetroShip.Repository.Repositories;

public class StationRepository : BaseRepository<Station>, IStationRepository
{
    private readonly AppDbContext _context;
    public StationRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }
    public IEnumerable<Station> GetStationsByRegion(string regionId)
    {
        return _context.Stations
                       .Where(station => station.RegionId == regionId && station.IsActive)
                       .ToList();
    }
}