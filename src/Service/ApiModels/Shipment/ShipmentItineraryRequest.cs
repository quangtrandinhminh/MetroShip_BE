namespace MetroShip.Service.ApiModels.Shipment;

public record ShipmentItineraryRequest
{
    public string RouteId { get; set; }
    public int LegOrder { get; set; }
}