namespace MetroShip.Service.ApiModels.Shipment;

public record ShipmentRejectRequest
{
    public string ShipmentId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty; 
}