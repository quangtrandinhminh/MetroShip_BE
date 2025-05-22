namespace MetroShip.Service.ApiModels.Shipment;

public record ShipmentDetailsResponse : ShipmentListResponse
{
    public decimal? InsuranceFeeVnd { get; set; }

    public decimal? SurchargeFeeVnd { get; set; }

    public ShippingInformation ShippingInformation { get; set; }

    public ShipmentTrackingResponse ShipmentTracking { get; set; }
}


public record ShippingInformation
{
    public string SenderId { get; set; }

    public string SenderName { get; set; }

    public string SenderPhone { get; set; }

    public string? RecipientId { get; set; }

    public string RecipientName { get; set; }

    public string RecipientPhone { get; set; }

    public string? RecipientEmail { get; set; }

    public string RecipientNationalId { get; set; }
}

public record ShipmentTrackingResponse
{
    public DateTimeOffset? ScheduledDateTime { get; set; }

    public DateTimeOffset? BookedAt { get; set; }

    public DateTimeOffset? ApprovedAt { get; set; }

    public DateTimeOffset? PaidAt { get; set; }

    public DateTimeOffset? PickupAt { get; set; }

    public DateTimeOffset? DeliveredAt { get; set; }

    public DateTimeOffset? SurchargeAppliedAt { get; set; }

    public DateTimeOffset? CancelledAt { get; set; }

    public DateTimeOffset? RefundedAt { get; set; }
}