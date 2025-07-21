using System.ComponentModel.DataAnnotations;
using MetroShip.Repository.Models.Base;
using Newtonsoft.Json;

namespace MetroShip.Repository.Models;

public class PricingConfig : BaseEntity
{
    public bool IsActive { get; set; } = true;

    public DateTimeOffset? EffectiveFrom { get; set; }

    public DateTimeOffset? EffectiveTo { get; set; }

    public List<WeightTier> WeightTiers { get; set; } = new();
    public List<DistanceTier> DistanceTiers { get; set; } = new();
}
