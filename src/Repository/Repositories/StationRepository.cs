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

    public async Task<List<string>> GetAllStationIdNearUser(double userLatitude,
        double userLongitude,
        int maxDistanceInMeters = 1000,
        int maxCount = 10)
    {
        var earthRadius = 6371000.0; // meters

        // Calculate the distance using the Haversine formula
        // Convert latitude and longitude from decimal degrees to radians for sine and cosine calculations
        var results = await _context.Stations
            .Where(s => s.Latitude != null && s.Longitude != null && s.IsActive && s.DeletedAt == null)
            .Select(s => new
            {
                Station = s,
                Distance = earthRadius * 2 * Math.Asin(
                    Math.Sqrt(
                        (1 - Math.Cos((userLatitude - s.Latitude.Value) * Math.PI / 180) +
                        Math.Cos(userLatitude * Math.PI / 180) * Math.Cos(s.Latitude.Value * Math.PI / 180) *
                        (1 - Math.Cos((userLongitude - s.Longitude.Value) * Math.PI / 180))
                        ) / 2
                    )
                )
            })
            .Where(x => x.Distance <= maxDistanceInMeters)
            .OrderBy(x => x.Distance)
            .Take(maxCount)
            .Select(x => x.Station.Id)
            .ToListAsync();

        return results;
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

    public async Task<string?> GetStationNameByIdAsync(string stationId)
    {
        if (string.IsNullOrEmpty(stationId))
            return null;

        return await _context.Stations
            .Where(s => s.Id == stationId)
            .Select(s => s.StationNameVi)
            .FirstOrDefaultAsync();
    }
}