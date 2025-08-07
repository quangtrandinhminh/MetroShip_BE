namespace MetroShip.Service.ApiModels.Shipment;

public record ShipmentFeedbackRequest
{
    public string ShipmentId { get; set; }
    public string? Feedback { get; set; }
    public int Rating { get; set; }
}