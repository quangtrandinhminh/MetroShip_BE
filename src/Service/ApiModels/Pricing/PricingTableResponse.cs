using System.ComponentModel.DataAnnotations.Schema;

namespace MetroShip.Service.ApiModels.Pricing;

public record PricingTableResponse
{
    public string Id { get; set; }
    public DateTimeOffset? EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
    public int? FreeStoreDays { get; set; }
    public decimal? BaseSurchargePerDayVnd { get; set; }
    public decimal? RefundRate { get; set; }
    public int? RefundForCancellationBeforeScheduledHours { get; set; }
    public string? Description { get; set; }
    public IList<WeightTierResponse> WeightTiers { get; set; } = new List<WeightTierResponse>();
    public IList<DistanceTierResponse> DistanceTiers { get; set; } = new List<DistanceTierResponse>();
}

public record WeightTierResponse
{
    public int TierOrder { get; set; }
    public decimal? MaxWeightKg { get; set; }
    public decimal? BasePriceVnd { get; set; }
    public decimal? BasePriceVndPerKmPerKg { get; set; }
    public bool IsPricePerKmAndKg { get; set; }

    public string Units
    {
        get
        {
            if (IsPricePerKmAndKg)
                return "VND/km/kg";
            return "VND";
        }
    }
}

public record DistanceTierResponse
{
    public int TierOrder { get; set; }
    public decimal? MaxDistanceKm { get; set; }
    public decimal? BasePriceVnd { get; set; }
    public decimal? BasePriceVndPerKm { get; set; }
    public bool IsPricePerKm { get; set; }

    public string Units
    {
        get
        {
            if (IsPricePerKm)
                return "VND/km";
            return "VND";
        }
    }
}