using MetroShip.Utility.Enums;

namespace MetroShip.Service.ApiModels.Shipment;

public sealed record ShipmentFilterRequest
{
    public string? TrackingCode { get; init; }
    public ShipmentStatusEnum? ShipmentStatus { get; init; }
    public DateTimeOffset? FromScheduleDateTime { get; init; }
    public DateTimeOffset? ToScheduleDateTime { get; init; }
    public string? DepartureStationId { get; init; }
    public string? DestinationStationId { get; init; }

    public string? SenderId { get; init; }
    public string? SenderName { get; init; }
    public string? SenderPhone { get; init; }

    public string? RecipientName { get; init; }
    public string? RecipientPhone { get; init; }
    public string? RecipientEmail { get; init; }

    public string? RegionCode { get; init; }
    public string? TimeSlotId { get; init; }
    public string? LineId { get; init; }
    public string? ItineraryIncludeStationId { get; init; }
    public bool? IsAwaitingNextTrain { get; init; }
}