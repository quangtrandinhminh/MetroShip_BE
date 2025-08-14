using MetroShip.Repository.Base;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Enums;
using MetroShip.Utility.Exceptions;
using MetroShip.Utility.Helpers;
using Microsoft.AspNetCore.Http;
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

    public async Task<Dictionary<(string, DirectionEnum), (string, string)>> GetStartAndEndStationIdsOfRouteAsync(string metroLineId)
    {
        if (string.IsNullOrEmpty(metroLineId))
            return new Dictionary<(string, DirectionEnum), (string, string)>();

        var routeData = await _context.Routes
            .Where(r => r.LineId == metroLineId && r.DeletedAt == null)
            .GroupBy(r => r.Direction) // Group by Direction only
            .Select(g => new
            {
                g.Key,
                Routes = g.OrderBy(r => r.SeqOrder)
                    .Select(r => new { r.FromStationId, r.ToStationId })
                    .ToList()
            })
            .ToListAsync();

        var result = new Dictionary<(string, DirectionEnum), (string, string)>();

        foreach (var lineGroup in routeData)
        {
            if (lineGroup.Routes.Any())
            {
                var startStationId = lineGroup.Routes.First().FromStationId;
                var endStationId = lineGroup.Routes.Last().ToStationId;
                result.Add((metroLineId, lineGroup.Key), (startStationId, endStationId));
            }
        }

        return result;
    }

    // get line by id, include route, order route by seqOrder, get the first.FromStationId and last.ToStationId
    public async Task<Dictionary<(string, DirectionEnum), (string, string)>> GetStartAndEndStationIdsOfRouteAsync(List<string> metroLineIds)
    {
        if (metroLineIds == null || !metroLineIds.Any())
            return new Dictionary<(string, DirectionEnum), (string, string)>();

        var routeData = await _context.Routes
            .Where(r => metroLineIds.Contains(r.LineId) && r.DeletedAt == null)
            .GroupBy(r => new { r.LineId, r.Direction }) // Group by both LineId and Direction
            .Select(g => new
            {
                g.Key.LineId,
                g.Key.Direction,
                Routes = g.OrderBy(r => r.SeqOrder)
                    .Select(r => new { r.FromStationId, r.ToStationId })
                    .ToList()
            })
            .ToListAsync();

        var result = new Dictionary<(string, DirectionEnum), (string, string)>();

        foreach (var lineGroup in routeData)
        {
            if (lineGroup.Routes.Any())
            {
                var startStationId = lineGroup.Routes.First().FromStationId;
                var endStationId = lineGroup.Routes.Last().ToStationId;
                result.Add((lineGroup.LineId, lineGroup.Direction), (startStationId, endStationId));
            }
        }

        return result;
    }

    public async Task<List<string>> GetAllStationsCanLoadShipmentAsync(string shipmentId)
    {
        var shipment = await _context.Shipments
            .Include(s => s.ShipmentItineraries)
            .ThenInclude(i => i.Route)
            .FirstOrDefaultAsync(s => s.Id == shipmentId);

        var stationIds = new List<string>();
        // Add departure station
        stationIds.Add(shipment.DepartureStationId);

        // Add ToStations where the shipment can transfer to different line
        var orderedItineraries = shipment.ShipmentItineraries.OrderBy(i => i.LegOrder).ToList();
        for (int i = 0; i < orderedItineraries.Count - 1; i++)
        {
            var currentItinerary = orderedItineraries[i];
            var nextItinerary = orderedItineraries[i + 1];

            // If next route is on different line, shipment can load at current ToStation
            if (currentItinerary.Route.LineId != nextItinerary.Route.LineId)
            {
                var toStationId = currentItinerary.Route.ToStationId;
                if (!string.IsNullOrEmpty(toStationId) && !stationIds.Contains(toStationId))
                    stationIds.Add(toStationId);
            }
        }

        return stationIds;
    }

    public async Task<List<string>> GetAllStationsCanUnloadShipmentAsync(string shipmentId)
    {
        var shipment = await _context.Shipments
            .Include(s => s.ShipmentItineraries)
            .ThenInclude(i => i.Route)
            .FirstOrDefaultAsync(s => s.Id == shipmentId);

        var stationIds = new List<string>();

        // Add ToStations where the shipment can transfer to different line
        var orderedItineraries = shipment.ShipmentItineraries.OrderBy(i => i.LegOrder).ToList();
        for (int i = 0; i < orderedItineraries.Count - 1; i++)
        {
            var currentItinerary = orderedItineraries[i];
            var nextItinerary = orderedItineraries[i + 1];

            // If next route is on different line, shipment can unload at current ToStation
            if (currentItinerary.Route.LineId != nextItinerary.Route.LineId)
            {
                var toStationId = currentItinerary.Route.ToStationId;
                if (!string.IsNullOrEmpty(toStationId) && !stationIds.Contains(toStationId))
                    stationIds.Add(toStationId);
            }
        }

        // Add departure station
        stationIds.Add(shipment.DestinationStationId);
        return stationIds;
    }
}