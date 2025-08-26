using MetroShip.Repository.Models.Base;
using MetroShip.Utility.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetroShip.Repository.Models;

public class TrainSchedule : BaseEntity
{
    public string TrainId { get; set; } 

    public string TimeSlotId { get; set; }

    [NotMapped]
    public ShiftEnum Shift { get; set; }

    public string LineId { get; set; }

    [NotMapped]
    public string? LineName { get; set; }

    [Column(TypeName = "date")]
    public DateOnly? Date { get; set; }

    public string DepartureStationId { get; set; }

    [NotMapped]
    public string? DepartureStationName { get; set; }

    public string DestinationStationId { get; set; }

    [NotMapped]
    public string? DestinationStationName { get; set; }

    public DirectionEnum Direction { get; set; }

    public DateTimeOffset? ArrivalTime { get; set; }

    public DateTimeOffset? DepartureTime { get; set; }

    public virtual MetroTrain Train { get; set; }

    public virtual MetroTimeSlot TimeSlot { get; set; }

    /*[InverseProperty(nameof(ShipmentItinerary.TrainSchedule))]
    public virtual ICollection<ShipmentItinerary>? ShipmentItineraries { get; set; }*/
}