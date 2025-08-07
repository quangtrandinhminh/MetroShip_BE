namespace MetroShip.Service.ApiModels.Parcel;

public record ParcelConfirmRequest
{
    public string ParcelCode { get; set; }
    public IList<ParcelMediaRequest> ConfirmedMedias { get; set; } = new List<ParcelMediaRequest>();
}