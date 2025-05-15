using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetroShip.Repository.Models;

[PrimaryKey("LineId", "TimeSlotId")]
public partial class MetroSchedule 
{
    [Key]
    public string? LineId { get; set; }

    [Key]
    public string? TimeSlotId { get; set; }

    [ForeignKey(nameof(LineId))]
    public virtual MetroLine? Line { get; set; }

    [ForeignKey(nameof(TimeSlotId))]
    public virtual MetroTimeSlot? TimeSlot { get; set; }
}