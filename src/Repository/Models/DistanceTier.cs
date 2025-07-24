using System.ComponentModel.DataAnnotations.Schema;
using MetroShip.Repository.Models.Base;

namespace MetroShip.Repository.Models;

public class DistanceTier : BaseEntity
{
    public string PricingConfigId { get; set; }
    public int TierOrder { get; set; }
    public decimal? MaxDistanceKm { get; set; }
    public decimal? BasePriceVnd { get; set; }
    public decimal? BasePriceVndPerKm { get; set; }
    public bool IsPricePerKm { get; set; }
    public string? Description { get; set; }

    [ForeignKey(nameof(PricingConfigId))]
    [InverseProperty(nameof(PricingConfig.DistanceTiers))]
    public PricingConfig PricingConfig { get; set; }

    public bool IsDistanceInRange(decimal distanceKm)
    {
        return MaxDistanceKm == null || distanceKm <= MaxDistanceKm;
    }
}