using MetroShip.Service.ApiModels.Parcel;

namespace MetroShip.Service.ApiModels.Shipment;

public record ShipmentDetailsResponse : ShipmentListResponse
{
    public decimal? InsuranceFeeVnd { get; set; }

    public decimal? SurchargeFeeVnd { get; set; }

    public decimal ShippingFeeVnd { get; set; }

    public DateTimeOffset? ApprovedAt { get; set; }

    public DateTimeOffset? PaidAt { get; set; }

    public DateTimeOffset? PickupAt { get; set; }

    public DateTimeOffset? DeliveredAt { get; set; }

    public DateTimeOffset? SurchargeAppliedAt { get; set; }

    public DateTimeOffset? CancelledAt { get; set; }

    public DateTimeOffset? RefundedAt { get; set; }

    public string SenderId { get; set; }

    public string? RecipientId { get; set; }

    public string? RecipientEmail { get; set; }

    public string RecipientNationalId { get; set; }

    public IList<ItineraryResponse> ShipmentItineraries { get; set; } = new List<ItineraryResponse>();
    public IList<ParcelResponse> Parcels { get; set; } = new List<ParcelResponse>();
}
