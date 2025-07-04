namespace MetroShip.Service.ApiModels.Shipment;

public sealed record ShipmentPickUpRequest
{
    public string ShipmentId { get; set; } = string.Empty;
    public string PickedUpImageLink { get; set; } = string.Empty;
}