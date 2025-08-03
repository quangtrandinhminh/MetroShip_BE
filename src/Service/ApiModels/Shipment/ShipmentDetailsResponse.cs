using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels.Parcel;
using MetroShip.Service.ApiModels.Transaction;
using MetroShip.Service.ApiModels.User;
using MetroShip.Utility.Enums;

namespace MetroShip.Service.ApiModels.Shipment;

public record ShipmentDetailsResponse : ShipmentListResponse
{
    // Financial fields
    public decimal? TotalInsuranceFeeVnd { get; set; }
    public decimal? TotalSurchargeFeeVnd { get; set; }
    public decimal TotalShippingFeeVnd { get; set; }
    public decimal? TotalOverdueSurchargeFee { get; set; }

    // Measurement fields
    public decimal? TotalKm { get; set; }
    public decimal? TotalWeightKg { get; set; }
    public decimal? TotalVolumeM3 { get; set; }

    // Scheduling fields
    public string? TimeSlotId { get; set; }
    public ShiftEnum? ScheduledShift { get; set; }
    public string? PricingConfigId { get; set; }
    public string? PriceStructureDescriptionJSON { get; set; }
    public DateTimeOffset? PaymentDealine { get; set; }
    

    // Status tracking timestamps
    public DateTimeOffset? PickedUpAt { get; set; }
    public DateTimeOffset? RejectedAt { get; set; }
    public string? RejectionReason { get; set; } // Reason for rejection, if applicable
    public string? RejectedBy { get; set; } // User who rejected the shipment   
    public string? ConfirmedBy { get; set; }

    public string? PickedUpBy { get; set; } // User who picked up the shipment
    public DateTimeOffset? AwaitedDeliveryAt { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; }
    public DateTimeOffset? SurchargeAppliedAt { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public DateTimeOffset? RefundedAt { get; set; }

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
    public string? CurrentTrainId { get; set; }

    // Customer fields
    public string SenderId { get; set; }
    public string? RecipientId { get; set; }
    public string? RecipientEmail { get; set; }
    public string? RecipientNationalId { get; set; }

    // Feedback fields
    public byte? Rating { get; set; }
    public string? Feedback { get; set; }
    public string? FeedbackResponse { get; set; }
    public string? FeedbackResponseBy { get; set; }

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
    public IList<ShipmentMediaResponse> ShipmentMedias { get; set; } = new List<ShipmentMediaResponse>();
    public ItineraryGraphResponse ItineraryGraph { get; set; }
}
