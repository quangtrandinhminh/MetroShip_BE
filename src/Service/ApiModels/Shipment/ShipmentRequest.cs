using System.Collections;
using System.ComponentModel.DataAnnotations;
using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels.Parcel;

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

    public string RecipientNationalId { get; set; }

    public DateTimeOffset ScheduledDateTime { get; set; }

    public decimal TotalCostVnd { get; set; }

    public decimal ShippingFeeVnd { get; set; }

    public decimal? InsuranceFeeVnd { get; set; }

    public IList<ShipmentItineraryRequest> ShipmentItineraries { get; set; } = new List<ShipmentItineraryRequest>();

    public IList<ParcelRequest> Parcels { get; set; } = new List<ParcelRequest>();
}