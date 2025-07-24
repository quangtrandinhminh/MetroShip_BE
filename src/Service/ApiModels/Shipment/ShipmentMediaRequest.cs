using MetroShip.Utility.Enums;

namespace MetroShip.Service.ApiModels.Shipment;

public sealed record ShipmentMediaRequest
{
    public string MediaUrl { get; set; }
    public string? Description { get; set; }
}