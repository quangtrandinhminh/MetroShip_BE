using MetroShip.Utility.Enums;

namespace MetroShip.Service.ApiModels.Shipment;

public record ShipmentTrackingResponse
{
    public string Id { get; set; }

    public string ShipmentId { get; set; }

    public string Status { get; set; }

    public ShipmentStatusEnum? CurrentShipmentStatus { get; set; }
    public string? CurrentShipmentStatusName => CurrentShipmentStatus?.ToString();

    public ShipmentStatusEnum TrackingForShipmentStatus { get; set; }
    public string TrackingForShipmentStatusName => TrackingForShipmentStatus.ToString();

    public DateTimeOffset EventTime { get; set; }

    public string? UpdatedBy { get; set; }

    public string? Note { get; set; }
}