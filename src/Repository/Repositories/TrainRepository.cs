using System.Linq.Expressions;
using MetroShip.Repository.Base;
using MetroShip.Repository.Extensions;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using Microsoft.EntityFrameworkCore;
using AppDbContext = MetroShip.Repository.Infrastructure.AppDbContext;

namespace MetroShip.Repository.Repositories;

public class TrainRepository : BaseRepository<MetroTrain>, ITrainRepository
{
    private readonly AppDbContext _context;

    public TrainRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    // Returns all Shipments associated with a given trainId
    public async Task<IList<Shipment>> GetShipmentsByTrainAsync(string trainId)
    {
        return await _context.ShipmentItineraries
            .Where(si => si.TrainId == trainId)
            .Include(si => si.Shipment)
            .Select(si => si.Shipment)
            .Where(s => s != null)
            .ToListAsync();
    }

    // Updates the train's location and station
    public async Task SaveTrainLocationAsync(string trainId, double lat, double lng, string stationId)
    {
        var train = await _context.MetroTrains.FirstOrDefaultAsync(t => t.Id == trainId);
        if (train != null)
        {
            train.Latitude = lat;
            train.Longitude = lng;
            // If you have a StationId property, set it here. Otherwise, add logic as needed.
            // train.StationId = stationId;
            _context.MetroTrains.Update(train);
            await _context.SaveChangesAsync();
        }
    }
    public async Task<MetroTrain?> GetTrainWithItineraryAndStationsAsync(string trainId)
    {
        return await _context.MetroTrains
            .Include(t => t.ShipmentItineraries
                .Where(si => si.TrainId == trainId && si.Route != null))
                .ThenInclude(si => si.Route)
                    .ThenInclude(r => r.FromStation)
            .Include(t => t.ShipmentItineraries)
                .ThenInclude(si => si.Route)
                    .ThenInclude(r => r.ToStation)
            .FirstOrDefaultAsync(t => t.Id == trainId);
    }

    public async Task<Shipment?> GetShipmentWithTrainAsync(string trackingCode)
    {
        return await _context.Shipments
            .Include(s => s.ShipmentItineraries)
                .ThenInclude(i => i.Train)
            .FirstOrDefaultAsync(s => s.TrackingCode == trackingCode);
    }
}
