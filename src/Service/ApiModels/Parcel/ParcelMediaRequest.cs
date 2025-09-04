namespace MetroShip.Service.ApiModels.Parcel;

public record ParcelMediaRequest
{
    public string ParcelId { get; set; }
    public string MediaUrl { get; set; }
    public string? Description { get; set; }
}