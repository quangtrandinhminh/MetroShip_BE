using MetroShip.Service.ApiModels.Parcel;

namespace MetroShip.Service.ApiModels.Shipment;

public record TotalPriceCalcRequest
{
    public string DepartureStationId { get; set; }
    public string DestinationStationId { get; set; }
    public double? UserLatitude { get; set; }
    public double? UserLongitude { get; set; }
    public DateTimeOffset ScheduleShipmentDate { get; set; }
    public IList<ParcelRequest> Parcels { get; set; } = new List<ParcelRequest>();
}