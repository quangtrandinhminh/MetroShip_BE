using MetroShip.Repository.Models;
using MetroShip.Utility.Config;

namespace MetroShip.Service.BusinessModels;
public class PriceCalculationService
{
    private readonly PricingTable _pricingTable;

    public PriceCalculationService(PricingTable pricingTable)
    {
        _pricingTable = pricingTable;
    }

    public decimal CalculateShippingPriceDemo(decimal weight, decimal distanceKm)
    {
        // For demo purposes, we will use a simple fixed price calculation
        var weightMaxTier1 = 10; // Example max weight for tier 1
        var pricePerKgAndKmTier1 = 30; // Example price per kg and km for tier 1
        var weightMaxTier2 = 50; // Example max weight for tier 2
        var pricePerKgAndKmTier2 = 25; // Example price per kg and km for tier 2
        var weightMaxTier3 = 100; // Example max weight for tier 3
        var pricePerKgAndKmTier3 = 22; // Example price per kg and km for tier 3
        var weightMaxTier4 = 200; // Example max weight for tier 4
        var pricePerKgAndKmTier4 = 20; // Example price per kg and km for tier 4
        var maxKmTier1 = 7; // Example max km for tier 1
        var priceKmTier1 = 5000; // Example price per km for tier 1 (vnd)
        var maxKmTier2 = 100; // Example max km for tier 2
        var pricePerKmTier2 = 800; // Example price per km for tier 2 (vnd/km)

        // Shipping fee = ((price by km*km) + (price by kg and km*kg*km))
        var priceByKm = 0m;
        if (distanceKm <= maxKmTier1)
        {
            priceByKm = priceKmTier1;
        }
        else if (distanceKm <= maxKmTier2)
        {
            // Calculate price for tier 1
            priceByKm = pricePerKmTier2 * distanceKm;
        }
        else if (distanceKm > maxKmTier2)
        {
            // free for distance > maxKmTier2
            priceByKm = 0;
        }

        var priceByKmAndKg = 0m;
        if (weight <= weightMaxTier1)
        {
            priceByKmAndKg = pricePerKgAndKmTier1 * weight * distanceKm;
        }
        else if (weight > weightMaxTier1 && weight <= weightMaxTier2)
        {
            priceByKmAndKg = pricePerKgAndKmTier2 * weight * distanceKm;
        }
        else if (weight > weightMaxTier2 && weight <= weightMaxTier3)
        {
            priceByKmAndKg = pricePerKgAndKmTier3 * weight * distanceKm;
        }
        else if (weight > weightMaxTier3 && weight <= weightMaxTier4)
        {
            priceByKmAndKg = pricePerKgAndKmTier4 * weight * distanceKm;
        }

        return priceByKm + priceByKmAndKg;
    }

    public decimal CalculateShippingPrice(decimal weight, decimal distanceKm)
    {
        // Find appropriate weight tier
        var weightTier = _pricingTable.WeightTiers.FirstOrDefault(tier =>
            weight > tier.PreviousMaxWeight && weight <= tier.MaxWeight);

        if (weightTier == null)
        {
            throw new ArgumentException($"No pricing tier found for weight: {weight}kg");
        }

        // Find appropriate distance tier index
        var distanceTierIndex = 0;
        for (int i = 0; i < _pricingTable.DistanceTiers.Count; i++)
        {
            if (distanceKm <= _pricingTable.DistanceTiers[i])
            {
                distanceTierIndex = i;
                break;
            }
            distanceTierIndex = _pricingTable.DistanceTiers.Count - 1; // Use the highest tier if distance exceeds all tiers
        }

        var pricePerUnit = weightTier.PricesPerDistance[distanceTierIndex];

        // For tier 1 (<5kg), it's a fixed price, not per kg
        if (weightTier.PreviousMaxWeight == 0)
        {
            return pricePerUnit;
        }

        // For other tiers, multiply by weight
        return pricePerUnit * weight;
    }
}

/*public class PricingTableBuilder
{
    private readonly SystemConfigSetting systemConfigSetting;

    public PricingTable BuildPricingTable(List<SystemConfig> configs)
    {
        var configDict = configs.ToDictionary(c => c.ConfigKey, c => c.ConfigValue);

        // Extract distance configuration
        var distanceStep = decimal.Parse(configDict[nameof(systemConfigSetting.DISTANCE_STEP_KM)]);
        var priceStepPercent = decimal.Parse(configDict[nameof(systemConfigSetting.PRICE_STEP_PERCENT_PER_DISTANCE_TIER)]);
        var distanceTierQuantity = int.Parse(configDict[nameof(systemConfigSetting.DISTANCE_TIER_QUANTITY)]);

        // Build distance tiers (300km, 600km, 900km, etc.)
        var distanceTiers = new List<int>();
        for (int i = 1; i <= distanceTierQuantity; i++)
        {
            distanceTiers.Add((int)(distanceStep * i));
        }

        // Build weight tiers based on your new structure
        var weightTiers = new List<WeightTierPricing>();

        // Tier 1: <5kg (fixed price, not per kg)
        var tier1MaxWeight = decimal.Parse(configDict[nameof(systemConfigSetting.WEIGHT_TIER_1_MAX_KG)]);
        var tier1BasePrice = decimal.Parse(configDict[nameof(systemConfigSetting.PRICE_TIER_1_VND)]);
        var tier1 = new WeightTierPricing
        {
            TierName = $"<{tier1MaxWeight}kg",
            PreviousMaxWeight = 0,
            MaxWeight = tier1MaxWeight,
            BasePrice = tier1BasePrice,
            PricesPerDistance = CalculatePricesForDistances(tier1BasePrice, priceStepPercent, distanceTierQuantity)
        };
        weightTiers.Add(tier1);

        // Tier 2: <5-<=10kg
        var tier2MaxWeight = decimal.Parse(configDict[nameof(systemConfigSetting.WEIGHT_TIER_2_MAX_KG)]);
        var tier2PricePerKg = decimal.Parse(configDict[nameof(systemConfigSetting.PRICE_TIER_2_VND_PER_KG)]);
        var tier2 = new WeightTierPricing
        {
            TierName = $"<{tier1MaxWeight}- <={tier2MaxWeight}kg",
            PreviousMaxWeight = tier1MaxWeight,
            MaxWeight = tier2MaxWeight,
            BasePrice = tier2PricePerKg,
            PricesPerDistance = CalculatePricesForDistances(tier2PricePerKg, priceStepPercent, distanceTierQuantity)
        };
        weightTiers.Add(tier2);

        // Tier 3: <10-<=50kg
        var tier3MaxWeight = decimal.Parse(configDict[nameof(systemConfigSetting.WEIGHT_TIER_3_MAX_KG)]);
        var tier3PricePerKg = decimal.Parse(configDict[nameof(systemConfigSetting.PRICE_TIER_3_VND_PER_KG)]);
        var tier3 = new WeightTierPricing
        {
            TierName = $"<{tier2MaxWeight}- <={tier3MaxWeight}kg",
            PreviousMaxWeight = tier2MaxWeight,
            MaxWeight = tier3MaxWeight,
            BasePrice = tier3PricePerKg,
            PricesPerDistance = CalculatePricesForDistances(tier3PricePerKg, priceStepPercent, distanceTierQuantity)
        };
        weightTiers.Add(tier3);

        // Tier 4: <50-<=100kg
        var tier4MaxWeight = decimal.Parse(configDict[nameof(systemConfigSetting.WEIGHT_TIER_4_MAX_KG)]);
        var tier4PricePerKg = decimal.Parse(configDict[nameof(systemConfigSetting.PRICE_TIER_4_VND_PER_KG)]);
        var tier4 = new WeightTierPricing
        {
            TierName = $"<{tier3MaxWeight}- <={tier4MaxWeight}kg",
            PreviousMaxWeight = tier3MaxWeight,
            MaxWeight = tier4MaxWeight,
            BasePrice = tier4PricePerKg,
            PricesPerDistance = CalculatePricesForDistances(tier4PricePerKg, priceStepPercent, distanceTierQuantity)
        };
        weightTiers.Add(tier4);

        // Tier 5: >100kg
        var tier5PricePerKg = decimal.Parse(configDict[nameof(systemConfigSetting.PRICE_TIER_5_VND_PER_KG)]);
        var tier5 = new WeightTierPricing
        {
            TierName = $">{tier4MaxWeight}kg",
            PreviousMaxWeight = tier4MaxWeight,
            MaxWeight = decimal.MaxValue, // No upper limit
            BasePrice = tier5PricePerKg,
            PricesPerDistance = CalculatePricesForDistances(tier5PricePerKg, priceStepPercent, distanceTierQuantity)
        };
        weightTiers.Add(tier5);

        return new PricingTable
        {
            DistanceTiers = distanceTiers,
            WeightTiers = weightTiers,
            DistanceStepKm = distanceStep,
            PriceStepPercent = priceStepPercent
        };
    }

    private List<decimal> CalculatePricesForDistances(decimal basePrice, decimal stepPercent, int tierCount)
    {
        var prices = new List<decimal>();
        for (int i = 0; i < tierCount; i++)
        {
            var multiplier = 1 + (stepPercent / 100 * i);
            prices.Add(basePrice * multiplier);
        }
        return prices;
    }
}*/

public class PricingTable
{
    public List<int> DistanceTiers { get; set; } = new();
    public List<WeightTierPricing> WeightTiers { get; set; } = new();
    public decimal DistanceStepKm { get; set; }
    public decimal PriceStepPercent { get; set; }
}

public class WeightTierPricing
{
    public string TierName { get; set; }
    public decimal PreviousMaxWeight { get; set; }
    public decimal MaxWeight { get; set; }
    public decimal BasePrice { get; set; }
    public List<decimal> PricesPerDistance { get; set; } = new();
}