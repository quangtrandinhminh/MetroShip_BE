using MetroShip.Repository.Extensions;
using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Pricing;
using MetroShip.Service.BusinessModels;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Mapper;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Exceptions;
using MetroShip.Utility.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace MetroShip.Service.Services;

public class PricingService(IServiceProvider serviceProvider) : IPricingService
{
    private readonly IPricingRepository _pricingRepository = serviceProvider.GetRequiredService<IPricingRepository>();
    private readonly IParcelRepository _parcelRepository = serviceProvider.GetRequiredService<IParcelRepository>();
    private readonly IUnitOfWork _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
    private readonly IMapperlyMapper _mapper = serviceProvider.GetRequiredService<IMapperlyMapper>();
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
    private readonly IMemoryCache _cache = serviceProvider.GetRequiredService<IMemoryCache>();
    private const int CACHE_EXPIRY_MINUTES = 30;
    private const string CACHE_KEY = "DefaultPricingConfig";

    public async Task<PaginatedListResponse<PricingTableResponse>> GetPricingPaginatedList(PaginatedListRequest request)
    {
        _logger.Information("Fetching all pricing configurations with pagination: {@Request}", request);

        var pricingConfigs = await _pricingRepository.GetAllPaginatedQueryable(
                request.PageNumber,
                request.PageSize,
                x => x.DeletedAt == null,
                x => x.LastUpdatedAt, false,
                x => x.WeightTiers, x => x.DistanceTiers
        );

        var response = _mapper.MapToPricingTablePaginatedList(pricingConfigs);
        return response;
    }


    private async Task<PricingConfig?> GetPricingConfigAsync(string? pricingConfigId = null)
    {
        PricingConfig pricingConfig;
        if (!string.IsNullOrEmpty(pricingConfigId))
        {
            pricingConfig = await _pricingRepository.GetSingleAsync(
                x => x.Id == pricingConfigId && x.IsActive,
                false,
                x => x.WeightTiers, x => x.DistanceTiers
                );
            if (pricingConfig == null)
            {
                throw new AppException(
                ErrorCode.BadRequest,
                "The specified pricing configuration is not found or is inactive.",
                StatusCodes.Status400BadRequest
                );
            }
            return pricingConfig;
        }

        var cacheKey = CACHE_KEY;
        if (_cache.TryGetValue(cacheKey, out PricingConfig? cachedTable))
        {
            _logger.Information("Retrieved pricing configuration from cache.");
            return cachedTable;
        }

        pricingConfig = await _pricingRepository.GetSingleAsync(
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
                throw new AppException(
                    ErrorCode.BadRequest,
                    "The specified pricing configuration is not found or is inactive.",
                StatusCodes.Status400BadRequest
                    );
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
        if (weightKg <= 0 || distanceKm <= 0)
        {
            throw new AppException(
                ErrorCode.BadRequest,
                "Weight and distance must be greater than zero.",
                StatusCodes.Status400BadRequest
            );
        }

        var pricingConfig = await GetPricingConfigAsync();

        // Find the applicable weight tier
        var weightTier = pricingConfig.WeightTiers
            .FirstOrDefault(t => t.IsWeightInRange(weightKg));
        if (weightTier == null)
        {
            throw new AppException(
                ErrorCode.BadRequest,
                "No applicable weight tier found for the given weight.",
                StatusCodes.Status400BadRequest
                );
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

    /*public async Task<PricingConfig> CreateOrUpdatePricingConfigAsync(PricingConfigRequest request)
    {
        if (request == null)
        {
            throw new AppException(
            ErrorCode.BadRequest,
            "Pricing configuration request cannot be null.",
            StatusCodes.Status400BadRequest
            );
        }

        var pricingConfig = _mapper.MapToPricingConfig(request);
        await _pricingRepository.AddAsync(pricingConfig);
        await _unitOfWork.SaveChangeAsync();
        _cache.Remove(CACHE_KEY); // Clear cache after update

        return pricingConfig;
    }*/

    public async Task CalculateOverdueSurcharge (Shipment shipment)
    {
        if (shipment == null)
        {
            throw new AppException(
            ErrorCode.BadRequest,
            "Shipment cannot be null.",
            StatusCodes.Status400BadRequest
            );
        }

        if (!shipment.Parcels.Any())
        {
            throw new AppException(
            ErrorCode.BadRequest,
            "Shipment must have at least one parcel to calculate overdue surcharge.",
            StatusCodes.Status400BadRequest
            );
        }

        var pricingConfig = await GetPricingConfigAsync(shipment.PricingConfigId);
        if (pricingConfig.BaseSurchargePerDayVnd == null || pricingConfig.FreeStoreDays == null)
        {
            throw new AppException(
            ErrorCode.BadRequest,
            "Base surcharge per day or free store days are not configured.",
            StatusCodes.Status400BadRequest
            );
        }

        var overdueDays = shipment.SurchargeAppliedAt.HasValue
            ? (CoreHelper.SystemTimeNow - shipment.SurchargeAppliedAt.Value).Days
            : 0;

        foreach (var parcel in shipment.Parcels)
        {
            var surchargeDays = overdueDays;
            var surchargeAmount = pricingConfig.BaseSurchargePerDayVnd.Value * surchargeDays;
            parcel.OverdueSurchangeFeeVnd = Math.Round(surchargeAmount, 2); // Round to 2 decimal places
            _parcelRepository.Update(parcel);
        }

        shipment.TotalSurchargeFeeVnd = shipment.Parcels.Sum(p => p.OverdueSurchangeFeeVnd ?? 0);
    }

    public async Task<int> GetFreeStoreDaysAsync(string pricingConfigId)
    {
        var pricingConfig = await GetPricingConfigAsync(pricingConfigId);
        if (pricingConfig == null || !pricingConfig.IsActive)
        {
            throw new AppException(
            ErrorCode.BadRequest,
            "No active pricing configuration found.",
            StatusCodes.Status400BadRequest
            );
        }

        return pricingConfig.FreeStoreDays ?? 0;
    }

    public async Task<int> GetRefundForCancellationBeforeScheduledHours (string pricingConfigId)
    {
        var pricingConfig = await GetPricingConfigAsync(pricingConfigId);
        if (pricingConfig == null || !pricingConfig.IsActive)
        {
            throw new AppException
            (
                ErrorCode.BadRequest,
                "No active pricing configuration found.",
                StatusCodes.Status400BadRequest
            );
        }

        return pricingConfig.RefundForCancellationBeforeScheduledHours ?? 0;
    }

    public async Task<decimal> CalculateRefund(string pricingConfigId, decimal? totalPrice)
    {
        var pricingConfig = await GetPricingConfigAsync(pricingConfigId);
        if (pricingConfig == null || !pricingConfig.IsActive)
        {
            throw new AppException(
            ErrorCode.BadRequest,
            "No active pricing configuration found.",
            StatusCodes.Status400BadRequest
            );
        }

        return (decimal)(totalPrice * (pricingConfig.RefundRate ?? 1));
    }
}