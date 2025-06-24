using MetroShip.Repository.Base;
using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using Microsoft.EntityFrameworkCore;

namespace MetroShip.Repository.Repositories;

public class ShipmentItineraryRepository : BaseRepository<ShipmentItinerary>, IShipmentItineraryRepository
{
    private readonly AppDbContext _context;

    public ShipmentItineraryRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<(List<Route> routes, List<Station> stations, List<MetroLine> metroLines)> GetRoutesAndStationsAsync()
    {
        Console.WriteLine("Fetching routes, stations, and metro lines...");
        // Lấy tất cả routes, stations và metroLines một lần
        var routes = await _context.Routes
            .AsNoTracking()
            .Include(r => r.MetroLine)
            .Select(x => new Route
            {
                Id = x.Id,
                FromStationId = x.FromStationId,
                ToStationId = x.ToStationId,
                RouteNameVi = x.RouteNameVi,
                LineId = x.LineId,
                SeqOrder = x.SeqOrder,
                TravelTimeMin = x.TravelTimeMin,
                LengthKm = x.LengthKm,
            })
            .ToListAsync();

        var stationIds = routes
            .SelectMany(r => new[] { r.FromStationId, r.ToStationId })
            .Distinct()
            .ToList();

        var stations = await _context.Stations
            .AsNoTracking()
            .Where(s => stationIds.Contains(s.Id))
            .ToListAsync();

        var lineIds = routes
            .Select(r => r.LineId)
            .Distinct()
            .ToList();

        var metroLines = await _context.MetroLines
            .AsNoTracking()
            .Where(l => lineIds.Contains(l.Id))
            .ToListAsync();

        return (routes, stations, metroLines);
    }
}