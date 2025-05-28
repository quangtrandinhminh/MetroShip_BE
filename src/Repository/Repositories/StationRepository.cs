using MetroShip.Repository.Base;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using MetroShip.Utility.Helpers;
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

    public async Task<List<Station>> GetAllStationNearUser(double userLatitude,
        double userLongitude,
        int maxDistanceInMeters = 1000,
        int maxCount = 10)
    {
        return await _context.Stations
            .Where(s => s.Latitude != null && s.Longitude != null)
            .Where(s => CalculateHelper.CalculateDistanceBetweenTwoCoordinatesByHaversine(
                               userLatitude, userLongitude,
                                              s.Latitude.Value, s.Longitude.Value) <= maxDistanceInMeters)
            .Take(maxCount)
            .ToListAsync();
    }
}