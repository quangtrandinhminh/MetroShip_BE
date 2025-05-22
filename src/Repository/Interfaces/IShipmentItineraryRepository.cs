using MetroShip.Repository.Base;
using MetroShip.Repository.Models;

namespace MetroShip.Repository.Interfaces;

public interface IShipmentItineraryRepository : IBaseRepository<ShipmentItinerary>
{
    Task<(List<Route> routes, List<Station> stations, List<MetroLine> metroLines)> GetRoutesAndStationsAsync();
}