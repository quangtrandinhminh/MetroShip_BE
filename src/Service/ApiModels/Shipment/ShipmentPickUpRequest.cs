using MetroShip.Service.ApiModels.Parcel;

namespace MetroShip.Service.ApiModels.Shipment;

public sealed record ShipmentPickUpRequest
{
    public string ShipmentId { get; set; }
    public IList<ShipmentMediaRequest> PickedUpMedias { get; set; } = new List<ShipmentMediaRequest>();
    public IList<ParcelMediaRequest> parcelMediaRequests { get; set; } = new List<ParcelMediaRequest>();
}