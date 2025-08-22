using MetroShip.Utility.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MetroShip.Service.ApiModels.Shipment;

public record ShipmentListResponse
{
    public string Id { get; set; }
    public string TrackingCode { get; set; }
    public string DepartureStationName { get; set; }
    public string DestinationStationName { get; set; }
    public string? CurrentStationName { get; set; }
    public string SenderName { get; set; }
    public string SenderPhone { get; set; }
    public string RecipientName { get; set; }
    public string RecipientPhone { get; set; }
    public decimal TotalCostVnd { get; set; }
    public bool IsCompensationRequested { get; set; }
    public bool IsReturnShipment { get; set; }
    public byte? Rating { get; set; }
    public string? Feedback { get; set; }
    public DateTimeOffset? StartReceiveAt { get; set; } 
    public DateTimeOffset ScheduledDateTime { get; set; }
    public ShipmentStatusEnum ShipmentStatus { get; set; }
    public string ShipmentStatusName => ShipmentStatus.ToString();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastUpdatedAt { get; set; }
}