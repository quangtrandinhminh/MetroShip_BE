namespace MetroShip.Service.ApiModels.Parcel;

public record ParcelTrackingResponse
{
    public string Status { get; set; }

    public string? StationId { get; set; }

    public DateTimeOffset EventTime { get; set; }
}