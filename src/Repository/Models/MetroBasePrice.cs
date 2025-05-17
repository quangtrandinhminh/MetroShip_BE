#nullable disable
using MetroShip.Repository.Models.Base;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetroShip.Repository.Models;

public partial class MetroBasePrice : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string LineId { get; set; }

    [Required]
    [StringLength(50)]
    public string TimeSlotId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal BasePriceVndPerKm { get; set; }

    public bool IsActive { get; set; }

    [ForeignKey(nameof(LineId))]
    [InverseProperty(nameof(MetroLine.MetroBasePrices))]
    public virtual MetroLine Line { get; set; }

    [ForeignKey(nameof(TimeSlotId))]
    [InverseProperty(nameof(MetroTimeSlot.MetroBasePrices))]
    public virtual MetroTimeSlot TimeSlot { get; set; }
}