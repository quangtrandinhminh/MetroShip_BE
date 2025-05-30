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

    public async Task<List<Station>> GetAllStationNearUser(
        double userLatitude,
        double userLongitude,
        int maxDistanceInMeters = 1000,
        int maxCount = 10)
    {
        var earthRadius = 6371000.0; // meters

        return await _context.Stations
            .Where(s => s.Latitude != null && s.Longitude != null)
            .Select(s => new
            {
                Station = s,
                Distance = earthRadius * 2 * Math.Asin(
                    Math.Sqrt(
                        Math.Pow(Math.Sin((Math.PI / 180) * (s.Latitude.Value - userLatitude) / 2), 2) +
                        Math.Cos((Math.PI / 180) * userLatitude) *
                        Math.Cos((Math.PI / 180) * s.Latitude.Value) *
                        Math.Pow(Math.Sin((Math.PI / 180) * (s.Longitude.Value - userLongitude) / 2), 2)
                    )
                )
            })
            .Where(x => x.Distance <= maxDistanceInMeters)
            .OrderBy(x => x.Distance)
            .Take(maxCount)
            .Select(x => x.Station)
            .ToListAsync();
    }

    public IEnumerable<Station> GetStationsByRegion(string regionId)
    {
        return _context.Stations
                       .Where(station => station.RegionId == regionId && station.IsActive)
                       .ToList();
    }

    // Checks if two stations are on the same metro line
    public bool AreStationsInSameMetroLine(string departureStationId, string destinationStationId)
    {
        return _context.Routes
            .Where(r => r.FromStationId == departureStationId || r.ToStationId == departureStationId)
            .Select(r => r.LineId)
            .Intersect(
                _context.Routes
                    .Where(r => r.FromStationId == destinationStationId || r.ToStationId == destinationStationId)
                    .Select(r => r.LineId)
            )
            .Any();
    }
}