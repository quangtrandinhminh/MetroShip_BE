using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MetroShip.Repository.Models.Base;
using MetroShip.Utility.Enums;
using Newtonsoft.Json;

namespace MetroShip.Repository.Models;

public class PricingConfig : BaseEntity
{
    public int? FreeStoreDays { get; set; }
    public decimal? BaseSurchargePerDayVnd { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? RefundRate { get; set; }
    public int? RefundForCancellationBeforeScheduledHours { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset? EffectiveFrom { get; set; }

    public DateTimeOffset? EffectiveTo { get; set; }

    [InverseProperty(nameof(WeightTier.PricingConfig))]
    public virtual ICollection<WeightTier> WeightTiers { get; set; } = new List<WeightTier>();

    [InverseProperty(nameof(DistanceTier.PricingConfig))]
    public virtual ICollection<DistanceTier> DistanceTiers { get; set; } = new List<DistanceTier>();

    [InverseProperty(nameof(Shipment.PricingConfig))]
    public virtual ICollection<Shipment>? Shipments { get; set; } = new List<Shipment>();
}
