using MetroShip.Service.ApiModels.Station;

namespace MetroShip.Service.ApiModels.Shipment;

public record ShipmentItineraryResponse
{
    public string RouteId { get; set; }
    public int LegOrder { get; set; }
    public int? EstMinutes { get; set; }
    public StationResponse? FromStation { get; set; }
    public StationResponse? ToStation { get; set; }
}