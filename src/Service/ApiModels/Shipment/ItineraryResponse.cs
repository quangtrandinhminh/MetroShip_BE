using MetroShip.Service.ApiModels.Route;
using MetroShip.Service.ApiModels.Station;

namespace MetroShip.Service.ApiModels.Shipment;

public sealed record ItineraryResponse
{
    public string Id { get; set; }
    public int LegOrder { get; set; }
    public decimal BasePriceVndPerKm { get; set; }
    public RouteResponse Route { get; set; }
}