namespace MetroShip.Service.ApiModels.Shipment;

public record ChangeItinerarySlotRequest
{
    public string ShipmentId { get; set; }
    public string FromTimeSlotId { get; set; }
    public DateOnly FromDate { get; set; }
    public string ToTimeSlotId { get; set; }
    public DateOnly ToDate { get; set; }
}