using MetroShip.Repository.Models.Base;
using MetroShip.Utility.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetroShip.Repository.Models;

public class ShipmentTracking : BaseEntity
{
    public string ShipmentId { get; set; } // Foreign key to Shipment
    public ShipmentStatusEnum CurrentShipmentStatus { get; set; } 
    public string Status { get; set; }
    public DateTimeOffset EventTime { get; set; } // Time of the event
    public string? UpdatedBy { get; set; } // User who updated the status
    public string? Note { get; set; } // Optional note for the tracking event

    [ForeignKey(nameof(ShipmentId))]
    [InverseProperty(nameof(Shipment.ShipmentTrackings))]
    public virtual Shipment Shipment { get; set; }
}