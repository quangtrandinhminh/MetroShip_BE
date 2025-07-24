using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels.Pricing;
using MetroShip.Service.BusinessModels;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Mapper;
using MetroShip.Utility.Exceptions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace MetroShip.Service.Services;

public class PricingService(IServiceProvider serviceProvider) : IPricingService
{
    private readonly IPricingRepository _pricingRepository = serviceProvider.GetRequiredService<IPricingRepository>();
    private readonly IUnitOfWork _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
    private readonly IMapperlyMapper _mapper = serviceProvider.GetRequiredService<IMapperlyMapper>();
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
    private readonly IMemoryCache _cache = serviceProvider.GetRequiredService<IMemoryCache>();
    private const int CACHE_EXPIRY_MINUTES = 30;
    private const string CACHE_KEY = "DefaultPricingConfig";

    private async Task<PricingConfig?> GetPricingConfigAsync()
    {
        var cacheKey = CACHE_KEY;
        if (_cache.TryGetValue(cacheKey, out PricingConfig? cachedTable))
        {
            _logger.Information("Retrieved pricing configuration from cache.");
            return cachedTable;
        }

        var pricingConfig = await _pricingRepository.GetSingleAsync(
            x => x.IsActive, false,
            x => x.WeightTiers, x => x.DistanceTiers
        );
        if (pricingConfig == null)
        {
            throw new AppException("No active pricing configuration found.");
        }

        _cache.Set(cacheKey, pricingConfig, TimeSpan.FromMinutes(CACHE_EXPIRY_MINUTES));
        return pricingConfig;
    }

    public async Task<PricingTableResponse> GetPricingTableAsync(string? pricingConfigId)
    {
        var pricingConfig = new PricingConfig();
        if (!string.IsNullOrEmpty(pricingConfigId))
        {
            pricingConfig = await _pricingRepository.GetSingleAsync(
                    x => x.Id == pricingConfigId,
                    false,
                    x => x.WeightTiers, x => x.DistanceTiers
            );
            if (pricingConfig == null)
            {
                throw new AppException("The specified pricing configuration is not found or is inactive.");
            }
        }
        else
        {
            pricingConfig = await GetPricingConfigAsync();
        }
        
        var response = _mapper.MapToPricingTableResponse(pricingConfig);
        return response;
    }

    public async Task<decimal> CalculatePriceAsync(decimal weightKg, decimal distanceKm)
    {
        var pricingConfig = await GetPricingConfigAsync();

        // Find the applicable weight tier
        var weightTier = pricingConfig.WeightTiers
            .FirstOrDefault(t => t.IsWeightInRange(weightKg));
        if (weightTier == null)
        {
            throw new AppException("No applicable weight tier found for the given weight.");
        }

        // Find the applicable distance tier
        var distanceTier = pricingConfig.DistanceTiers
            .FirstOrDefault(t => t.IsDistanceInRange(distanceKm));

        // Calculate the price based on the tiers
        decimal price;
        var priceByKgAndKm = CalculateKgAndKmPrice(weightTier, weightKg, distanceKm);
        var priceByKm = CalculateDistancePrice(distanceTier, distanceKm);
        price = priceByKgAndKm + priceByKm;

        return price;
    }

    private decimal CalculateDistancePrice(DistanceTier? distanceTier, decimal distanceKm)
    {
        // if the distance tier is over the max distance, return 0 ~ free for large distances
        if (distanceTier!= null && distanceTier.IsPricePerKm)
        {
            return distanceTier.BasePriceVndPerKm.GetValueOrDefault() * distanceKm;
        }
        return distanceTier?.BasePriceVnd ?? 0;
    }

    private decimal CalculateKgAndKmPrice(WeightTier weightTier, decimal weightKg, decimal distanceKm)
    {
        if (weightTier.IsPricePerKmAndKg)
        {
            return weightTier.BasePriceVndPerKmPerKg.GetValueOrDefault() * weightKg * distanceKm;
        }
        return weightTier.BasePriceVnd ?? 0;
    }
}