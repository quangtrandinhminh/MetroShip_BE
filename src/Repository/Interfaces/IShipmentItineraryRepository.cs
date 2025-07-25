using MetroShip.Repository.Base;
using MetroShip.Repository.Models;
using MetroShip.Repository.Repositories;

namespace MetroShip.Repository.Interfaces;

public interface IShipmentItineraryRepository : IBaseRepository<ShipmentItinerary>
{
    Task<(List<Route> routes, List<Station> stations, List<MetroLine> metroLines)> GetRoutesAndStationsAsync();
    Task<List<ShipmentItinerary>> AssignTrainIdToEmptyLegsAsync(string shipmentTrackingCode, string trainId);
}