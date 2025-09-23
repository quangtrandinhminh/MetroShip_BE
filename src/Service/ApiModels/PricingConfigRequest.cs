namespace MetroShip.Service.ApiModels;

public record PricingConfigRequest
{
    public string? Id { get; set; }
    public bool IsActive { get; set; } 
    public int? FreeStoreDays { get; set; }
    public decimal? BaseSurchargePerDayVnd { get; set; }
    public decimal? RefundRate { get; set; }
    public int? RefundForCancellationBeforeScheduledHours { get; set; }
    public string? Description { get; set; }
    public IList<WeightTierRequest> WeightTiers { get; set; } = new List<WeightTierRequest>();
    public IList<DistanceTierRequest> DistanceTiers { get; set; } = new List<DistanceTierRequest>();
}


public record WeightTierRequest
{
    public int TierOrder { get; set; }
    public decimal? MaxWeightKg { get; set; }
    public decimal? BasePriceVnd { get; set; }
    public decimal? BasePriceVndPerKmPerKg { get; set; }
    public bool IsPricePerKmAndKg { get; set; }
}

public record DistanceTierRequest
{
    public int TierOrder { get; set; }
    public decimal? MaxDistanceKm { get; set; }
    public decimal? BasePriceVnd { get; set; }
    public decimal? BasePriceVndPerKm { get; set; }
    public bool IsPricePerKm { get; set; }
}