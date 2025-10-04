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
    public decimal? TotalRefundedFeeVnd { get; set; }
    public decimal? TotalCompensationFeeVnd { get; set; } 

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
    public DateTimeOffset? EstArrivalTime { get; set; }

    // Status tracking timestamps
    public string? RejectionReason { get; set; } // Reason for rejection, if applicable
    public string? RejectedBy { get; set; } // User who rejected the shipment   
    public string? ConfirmedBy { get; set; }
    public string? PickedUpBy { get; set; } // User who picked up the shipment
    public DateTimeOffset? BookedAt { get; set; }
    public DateTimeOffset? PickedUpAt { get; set; }
    public DateTimeOffset? RejectedAt { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; }
    public DateTimeOffset? AwaitedDeliveryAt { get; set; }
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
    public string? CurrentStationName { get; set; }
    public string? CurrentTrainId { get; set; }
    public string? CurrentTrainCode { get; set; }
    public string? WaitingForTrainCode { get; set; } 

    // Customer fields
    public string SenderId { get; set; }
    public string? RecipientId { get; set; }
    public string? RecipientEmail { get; set; }
    public string? RecipientNationalId { get; set; }

    // Feedback fields
    public DateTimeOffset? FeedbackAt { get; set; }
    public string? FeedbackResponse { get; set; }
    public string? FeedbackResponseBy { get; set; }

    // Base Entity fields (if needed in response)
    public DateTimeOffset? DeletedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? LastUpdatedBy { get; set; }
    public string? DeletedBy { get; set; }

    // Navigation properties
    public IList<ItineraryResponse> ShipmentItineraries { get; set; } = new List<ItineraryResponse>();
    public IList<ParcelResponse> Parcels { get; set; } = new List<ParcelResponse>();
    //public IList<TransactionResponse>? Transactions { get; set; } = new List<TransactionResponse>();
    public IList<ShipmentMediaResponse> ShipmentMedias { get; set; } = new List<ShipmentMediaResponse>();

    public IList<ShipmentTrackingResponse> ShipmentTrackings { get; set; } = new List<ShipmentTrackingResponse>();
    public ItineraryGraphResponse ItineraryGraph { get; set; }

    public IList<ItinerarySummary> ItinerarySummaries
    {
        // group by RouteId, TrainId, TimeSlotId, Date
        // if any group has any IsCompleted = false, then the whole group is IsCompleted = false
        // legOrder is the min of the group
        get
        {
            return ShipmentItineraries
                .GroupBy(i => new { i.MetroRouteName, i.TimeSlotId, i.Date })
                .Select(g => new ItinerarySummary
                {
                    LegOrder = g.Min(i => i.LegOrder),
                    MetroRouteCode = g.First().MetroRouteId,
                    MetroRouteName = g.First().MetroRouteName,
                    DirectionName = g.First().DirectionName,
                    TrainId = g.First().TrainId,
                    TrainCode = g.First().TrainCode,
                    TimeSlotId = g.Key.TimeSlotId,
                    ShiftName = g.First().ShiftName,
                    Date = g.Key.Date,
                    IsCompleted = g.All(i => i.IsCompleted)
                })
                .OrderBy(s => s.LegOrder)
                .ToList();
        }
    }
}

public record ItinerarySummary
{
    public int LegOrder { get; set; }
    public string? MetroRouteCode { get; set; } = null;
    public string? MetroRouteName { get; set; } = null;
    public string? TrainId { get; set; } = null;
    public string? DirectionName { get; set; } = null;
    public string? TrainCode { get; set; } = null;
    public string? TimeSlotId { get; set; } = null;
    public string? ShiftName { get; set; } = null;
    public DateOnly? Date { get; set; } = null;
    public bool IsCompleted { get; set; }
}
