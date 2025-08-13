using MetroShip.Utility.Enums;
using System.ComponentModel.DataAnnotations;

namespace MetroShip.Service.ApiModels.Parcel;

public record ParcelTrackingResponse
{
    public string Id { get; set; }

    public string ParcelId { get; set; }

    public string Status { get; set; }

    public string? StationId { get; set; }

    public string? TrainId { get; set; }

    public ParcelStatusEnum CurrentParcelStatus { get; set; }
    public string CurrentParcelStatusName => CurrentParcelStatus.ToString();

    public ShipmentStatusEnum? CurrentShipmentStatus { get; set; }
    public string? CurrentShipmentStatusName => CurrentShipmentStatus?.ToString();

    public ShipmentStatusEnum TrackingForShipmentStatus { get; set; }
    public string TrackingForShipmentStatusName => TrackingForShipmentStatus.ToString();

    public DateTimeOffset EventTime { get; set; }

    public string? UpdatedBy { get; set; }

    public string? Note { get; set; }
}