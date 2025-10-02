using MetroShip.Service.ApiModels.Parcel;
using MetroShip.Utility.Enums;

namespace MetroShip.Service.ApiModels.Shipment;

public record ShipmentForGuest
{
    public string TrackingCode { get; set; }
    public string DepartureStationName { get; set; }
    public string DestinationStationName { get; set; }
    public string SenderName { get; set; }
    public string RecipientName { get; set; }
    public decimal TotalCostVnd { get; set; }
    public ShipmentStatusEnum ShipmentStatus { get; set; }
    public string ShipmentStatusName => ShipmentStatus.ToString();
    public IList<ItineraryResponse> ShipmentItineraries { get; set; } = new List<ItineraryResponse>();
    public IList<ParcelResponse> Parcels { get; set; } = new List<ParcelResponse>();
    public ItineraryGraphResponse ItineraryGraph { get; set; }
}