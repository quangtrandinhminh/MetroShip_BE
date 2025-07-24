using MetroShip.Repository.Models.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetroShip.Repository.Models;

public class WeightTier : BaseEntity
{
    public string PricingConfigId { get; set; }
    public int TierOrder { get; set; }
    public decimal? MinWeightKg { get; set; }
    public decimal? MaxWeightKg { get; set; }
    public decimal? BasePriceVnd { get; set; }
    public decimal? BasePriceVndPerKmPerKg { get; set; }
    public bool IsPricePerKmAndKg { get; set; }

    [ForeignKey(nameof(PricingConfigId))]
    [InverseProperty(nameof(PricingConfig.WeightTiers))]
    public virtual PricingConfig PricingConfig { get; set; }

    public bool IsWeightInRange(decimal weightKg)
    {
        return (MinWeightKg == null || weightKg > MinWeightKg) &&
               (MaxWeightKg == null || weightKg <= MaxWeightKg);
    }
}