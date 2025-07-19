namespace MetroShip.Service.ApiModels.Shipment;

public sealed record ShipmentPickUpRequest
{
    public string ShipmentId { get; set; }
    public IList<ShipmentMediaRequest> PickedUpMedias { get; set; } = new List<ShipmentMediaRequest>();
}