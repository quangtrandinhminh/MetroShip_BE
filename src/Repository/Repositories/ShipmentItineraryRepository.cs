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
            .Where(r => r.MetroLine.IsActive)
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

    public async Task<List<ShipmentItinerary>> AssignTrainIdToEmptyLegsAsync(string shipmentTrackingCode, string trainId)
    {
        if (string.IsNullOrWhiteSpace(shipmentTrackingCode))
            throw new ArgumentException("Tracking code is required.", nameof(shipmentTrackingCode));
        if (string.IsNullOrWhiteSpace(trainId))
            throw new ArgumentException("TrainId is required.", nameof(trainId));

        // Tìm shipment + các leg
        var shipment = await _context.Shipments
            .Include(s => s.ShipmentItineraries)
            .FirstOrDefaultAsync(s => s.TrackingCode == shipmentTrackingCode);

        if (shipment == null)
            throw new InvalidOperationException($"Shipment with tracking code '{shipmentTrackingCode}' not found.");

        // Lấy legs chưa có TrainId
        var itinerariesToUpdate = shipment.ShipmentItineraries
            .Where(it => string.IsNullOrEmpty(it.TrainId))
            .ToList();

        if (!itinerariesToUpdate.Any())
            return new List<ShipmentItinerary>();

        // Gán đúng trainId bạn truyền vào
        foreach (var itinerary in itinerariesToUpdate)
        {
            itinerary.TrainId = trainId;
        }

        // Lưu
        await _context.SaveChangesAsync();

        // Clear cache để chắc chắn lấy từ DB mới nhất
        _context.ChangeTracker.Clear();

        // Trả ra đúng dữ liệu mới
        var updatedItineraries = await _context.ShipmentItineraries
            .Where(it => it.ShipmentId == shipment.Id)
            .Include(it => it.Train)
            .AsNoTracking()
            .OrderBy(it => it.LegOrder)
            .ToListAsync();

        return updatedItineraries;
    }
}