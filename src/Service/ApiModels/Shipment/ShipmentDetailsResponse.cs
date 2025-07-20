using MetroShip.Service.ApiModels.Parcel;
using MetroShip.Service.ApiModels.Transaction;
using MetroShip.Service.ApiModels.User;
using MetroShip.Utility.Enums;

namespace MetroShip.Service.ApiModels.Shipment;

public record ShipmentDetailsResponse : ShipmentListResponse
{
    // Financial fields
    public decimal? InsuranceFeeVnd { get; set; }
    public decimal? SurchargeFeeVnd { get; set; }
    public decimal ShippingFeeVnd { get; set; }
    public decimal? TotalOverdueSurchargeFee { get; set; }

    // Measurement fields
    public decimal? TotalKm { get; set; }
    public decimal? TotalWeightKg { get; set; }
    public decimal? TotalVolumeM3 { get; set; }

    // Scheduling fields
    public string? TimeSlotId { get; set; }
    public ShiftEnum? ScheduledShift { get; set; }
    public string? PriceStructureDescriptionJSON { get; set; }

    // Status tracking timestamps
    public DateTimeOffset? SurchargeAppliedAt { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public DateTimeOffset? RefundedAt { get; set; }
    public DateTimeOffset? RejectedAt { get; set; }
    public DateTimeOffset? PaymentDeadline { get; set; }
    public DateTimeOffset? AwaitedDeliveryAt { get; set; }

    // Approval/Rejection fields
    public string? RejectionReason { get; set; }
    public string? RejectedBy { get; set; }
    public string? ConfirmedBy { get; set; }
    public string? PickedUpBy { get; set; }

    // Return fields
    public string? ReturnForShipmentId { get; set; }
    public DateTimeOffset? ReturnRequestedAt { get; set; }
    public DateTimeOffset? ReturnConfirmedAt { get; set; }
    public string? ReturnReason { get; set; }
    public string? ReturnConfirmedBy { get; set; }
    public DateTimeOffset? ReturnPickupAt { get; set; }
    public DateTimeOffset? ReturnDeliveredAt { get; set; }
    public DateTimeOffset? ReturnCancelledAt { get; set; }

    // Station tracking
    public string DepartureStationId { get; set; }
    public string DestinationStationId { get; set; }
    public string? CurrentStationId { get; set; }

    // Customer fields
    public string SenderId { get; set; }
    public string? RecipientId { get; set; }
    public string? RecipientEmail { get; set; }
    public string? RecipientNationalId { get; set; }

    // Image fields
    public string? PickedUpImageLink { get; set; }
    public string? DeliveredImageLink { get; set; }
    public string? NationalIdImageFrontLink { get; set; }
    public string? NationalIdImageBackLink { get; set; }

    // Feedback fields
    public byte? Rating { get; set; }
    public string? Feedback { get; set; }

    // Base Entity fields (if needed in response)
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastUpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? LastUpdatedBy { get; set; }
    public string? DeletedBy { get; set; }

    // Navigation properties
    public IList<ItineraryResponse> ShipmentItineraries { get; set; } = new List<ItineraryResponse>();
    public IList<ParcelResponse> Parcels { get; set; } = new List<ParcelResponse>();
    //public IList<TransactionResponse>? Transactions { get; set; } = new List<TransactionResponse>();
}
