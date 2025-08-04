namespace MetroShip.Service.ApiModels.Parcel;

public record ParcelMediaRequest
{
    public string MediaUrl { get; set; }
    public string? Description { get; set; }
}