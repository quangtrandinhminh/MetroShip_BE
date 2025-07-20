using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels.Parcel;
using MetroShip.Utility.Enums;

namespace MetroShip.Service.ApiModels.Shipment;

public sealed record ShipmentRequest
{
    public string DepartureStationId { get; set; }

    public string DestinationStationId { get; set; }

    public string SenderName { get; set; }

    public string SenderPhone { get; set; }

    public string? RecipientId { get; set; }

    public string RecipientName { get; set; }

    public string RecipientPhone { get; set; }

    public string? RecipientEmail { get; set; }

    public DateTimeOffset ScheduledDateTime { get; set; }
    public string TimeSlotId { get; set; } 
    public decimal TotalCostVnd { get; set; }
    public decimal TotalShippingFeeVnd { get; set; }
    public decimal? TotalInsuranceFeeVnd { get; set; } = 0;
    public decimal TotalKm { get; set; }
    public string? TrackingLink { get; set; } = "https://fe-metro-ship.vercel.app/tracking-order";
    public IList<ShipmentItineraryRequest> ShipmentItineraries { get; set; } = new List<ShipmentItineraryRequest>();
    public IList<ParcelRequest> Parcels { get; set; } = new List<ParcelRequest>();
}