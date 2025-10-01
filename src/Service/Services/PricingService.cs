using MetroShip.Repository.Base;
using MetroShip.Repository.Extensions;
using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Pricing;
using MetroShip.Service.BusinessModels;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Mapper;
using MetroShip.Service.Validations;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Exceptions;
using MetroShip.Utility.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Linq.Expressions;

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

    public async Task<PaginatedListResponse<PricingTableResponse>> GetPricingPaginatedList(PaginatedListRequest request, bool? isActive = null)
    {
        _logger.Information("Fetching all pricing configurations with pagination: {@Request}", request);
        Expression<Func<PricingConfig, bool>> predicate = x => x.DeletedAt == null;
        if (isActive.HasValue)
        {
            predicate = predicate.And(x => x.IsActive == isActive.Value);
        }

        var pricingConfigs = await _pricingRepository.GetAllPaginatedQueryable(
                request.PageNumber,
                request.PageSize,
                predicate,
                x => x.LastUpdatedAt, false,
                x => x.WeightTiers, x => x.DistanceTiers
        );

        // order by isActive first, then by LastUpdatedAt descending
        pricingConfigs.Items = pricingConfigs.Items
            .OrderByDescending(pc => pc.IsActive)
            .ThenByDescending(pc => pc.LastUpdatedAt)
            .ToList();
        var response = _mapper.MapToPricingTablePaginatedList(pricingConfigs);
        foreach (var item in response.Items)
        {
            // order the tiers by tier order ascending
            item.WeightTiers = item.WeightTiers.OrderBy(t => t.TierOrder).ToList();
            item.DistanceTiers = item.DistanceTiers.OrderBy(t => t.TierOrder).ToList();
        }
        return response;
    }

    private async Task<PricingConfig?> GetPricingConfigAsync(string? pricingConfigId = null)
    {
        PricingConfig pricingConfig;
        if (!string.IsNullOrEmpty(pricingConfigId))
        {
            pricingConfig = await _pricingRepository.GetSingleAsync(
                x => x.Id == pricingConfigId,
                true,
                x => x.WeightTiers, x => x.DistanceTiers
                );
            if (pricingConfig == null)
            {
                throw new AppException(
                    ErrorCode.BadRequest,
                    ResponseMessagePricingConfig.PRICING_CONFIG_NOT_FOUND,
                    StatusCodes.Status400BadRequest
                );
            }

            pricingConfig.WeightTiers = pricingConfig.WeightTiers.OrderBy(t => t.TierOrder).ToList();
            pricingConfig.DistanceTiers = pricingConfig.DistanceTiers.OrderBy(t => t.TierOrder).ToList();
            return pricingConfig;
        }

        if (_cache.TryGetValue(CACHE_KEY, out PricingConfig? cachedTable))
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
            throw new AppException(
                ErrorCode.BadRequest,
                ResponseMessagePricingConfig.PRICING_CONFIG_NOT_FOUND,
                StatusCodes.Status400BadRequest
            );
        }

        pricingConfig.WeightTiers = pricingConfig.WeightTiers.OrderBy(t => t.TierOrder).ToList();
        pricingConfig.DistanceTiers = pricingConfig.DistanceTiers.OrderBy(t => t.TierOrder).ToList();
        _cache.Set(CACHE_KEY, pricingConfig, TimeSpan.FromMinutes(CACHE_EXPIRY_MINUTES));

        return pricingConfig;
    }

    public async Task<PricingTableResponse> GetPricingTableAsync(string? pricingConfigId)
    {
        var pricingConfig = await GetPricingConfigAsync(pricingConfigId);
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

        // Get min and max weights first
        var minWeight = pricingConfig.WeightTiers.MinBy(t => t.MinWeightKg)?.MinWeightKg ?? 0;
        var maxWeight = pricingConfig.WeightTiers.MaxBy(t => t.MaxWeightKg)?.MaxWeightKg ?? 0;

        // Check if weight is within bounds before searching for tier
        if (weightKg < minWeight || weightKg > maxWeight)
        {
            throw new AppException(
                ErrorCode.BadRequest,
                $"Trọng lượng {weightKg}kg nằm ngoài mức cho phép. Giới hạn: {minWeight}kg - {maxWeight}kg",
                StatusCodes.Status400BadRequest
                );
        }

        var weightTier = pricingConfig.WeightTiers
            .FirstOrDefault(t => t.IsWeightInRange(weightKg));

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

    public async Task<string> ChangePricingConfigAsync(PricingConfigRequest request)
    {
        _logger.Information("Creating or updating pricing configuration: {@Request}", request);
        PricingConfigValidator.ValidatePricingConfigRequest(request);
        string result;
        // if updating existing config, ensure it exists and is not active
        if (!string.IsNullOrEmpty(request.Id))
        {
            _logger.Information("Updating existing pricing configuration with ID: {Id}", request.Id);

            if (request.IsActive)
            {
                throw new AppException(
                ErrorCode.BadRequest,
                ResponseMessagePricingConfig.PRICING_CONFIG_CANNOT_ACTIVATE_ON_UPDATE,
                StatusCodes.Status400BadRequest
                );
            }

            var existingConfig = await _pricingRepository.GetSingleAsync(
                    x => x.Id == request.Id && x.DeletedAt == null,
                    false,
                    x => x.WeightTiers, x => x.DistanceTiers
                );

            if (existingConfig == null)
            {
                throw new AppException(
                ErrorCode.BadRequest,
                ResponseMessagePricingConfig.PRICING_CONFIG_NOT_FOUND,
                StatusCodes.Status400BadRequest
                );
            }

            if (existingConfig.IsActive)
            {
                throw new AppException(
                ErrorCode.BadRequest,
                ResponseMessagePricingConfig.PRICING_CONFIG_IN_USE,
                StatusCodes.Status400BadRequest
                );
            }

            _mapper.MapToPricingConfigEntity(request, existingConfig);
            _pricingRepository.Update(existingConfig);

            result = ResponseMessagePricingConfig.PRICING_CONFIG_UPDATE_SUCCESS;
        }
        else
        {
            _logger.Information("Creating new pricing configuration.");

            // create new config and deactivate old active config if needed
            var activeConfig = _pricingRepository.GetAllWithCondition(x => x.IsActive
                                                                           && x.DeletedAt == null);

            if (activeConfig.Any() && request.IsActive)
            {
                foreach (var config in activeConfig)
                {
                    config.IsActive = false;
                    config.EffectiveTo = CoreHelper.SystemTimeNow;
                    _pricingRepository.Update(config);
                }

                _cache.Remove(CACHE_KEY); // Clear cache after update
            }

            var pricingConfig = _mapper.MapToPricingConfigEntity(request);
            if (request.IsActive)
            {
                pricingConfig.EffectiveFrom = CoreHelper.SystemTimeNow;
            }

            _pricingRepository.Add(pricingConfig);
            result = ResponseMessagePricingConfig.PRICING_CONFIG_CREATE_SUCCESS;
        }
        
        await _unitOfWork.SaveChangeAsync();
       
        return result;
    }

    // activate config
    public async Task<string> ActivatePricingConfigAsync(string pricingConfigId)
    {
        var pricingConfig = await _pricingRepository.GetSingleAsync(
                       x => x.Id == pricingConfigId && x.DeletedAt == null);
        if (pricingConfig == null)
        {
            throw new AppException(
            ErrorCode.BadRequest,
            ResponseMessagePricingConfig.PRICING_CONFIG_NOT_FOUND,
            StatusCodes.Status400BadRequest
            );
        }

        if (pricingConfig.IsActive)
        {
            throw new AppException(
            ErrorCode.BadRequest,
            ResponseMessagePricingConfig.PRICING_CONFIG_ALREADY_ACTIVATED,
            StatusCodes.Status400BadRequest
            );
        }

        // cannot activate expired config
        if (pricingConfig.EffectiveTo != null)
        {
            throw new AppException(
            ErrorCode.BadRequest,
            ResponseMessagePricingConfig.PRICING_CONFIG_EXPIRED,
            StatusCodes.Status400BadRequest
            );
        }

        // ensure only one active config
        var activeConfigs = _pricingRepository.GetAllWithCondition(x => x.IsActive
            && x.DeletedAt == null);
        if (activeConfigs.Any())
        {
            foreach (var config in activeConfigs)
            {
                config.IsActive = false;
                config.EffectiveTo = CoreHelper.SystemTimeNow;
                _pricingRepository.Update(config);
            }

            _cache.Remove(CACHE_KEY); // Clear cache after update
        }

        pricingConfig.IsActive = true;
        pricingConfig.EffectiveFrom = CoreHelper.SystemTimeNow;
        pricingConfig.EffectiveTo = null;
        _pricingRepository.Update(pricingConfig);
       
        await _unitOfWork.SaveChangeAsync();
        return ResponseMessagePricingConfig.PRICING_CONFIG_UPDATE_SUCCESS;
    }

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
        return pricingConfig.FreeStoreDays ?? 0;
    }

    public async Task<int> GetRefundForCancellationBeforeScheduledHours (string pricingConfigId)
    {
        var pricingConfig = await GetPricingConfigAsync(pricingConfigId);
        return pricingConfig.RefundForCancellationBeforeScheduledHours ?? 0;
    }

    public async Task<decimal> CalculateRefund(string pricingConfigId, decimal? totalPrice)
    {
        var pricingConfig = await GetPricingConfigAsync(pricingConfigId);
        return (decimal)(totalPrice * (pricingConfig.RefundRate ?? 1));
    }

    // delete pricing config
    public async Task<string> DeletePricingConfigAsync(string pricingConfigId)
    {
        var pricingConfig = await _pricingRepository.GetSingleAsync(
        x => x.Id == pricingConfigId && x.DeletedAt == null,
        true,
        x => x.WeightTiers, x => x.DistanceTiers
                    );
        if (pricingConfig == null)
        {
            throw new AppException(
            ErrorCode.BadRequest,
            ResponseMessagePricingConfig.PRICING_CONFIG_NOT_FOUND,
            StatusCodes.Status400BadRequest
                );
        }

        if (pricingConfig.IsActive)
        {
            throw new AppException(
            ErrorCode.BadRequest,
            ResponseMessagePricingConfig.PRICING_CONFIG_IN_USE,
            StatusCodes.Status400BadRequest
            );
        }

        var isUsedInShipment = await _pricingRepository.IsExistAsync(
                       x => x.Shipments.Any(s => s.DeletedAt == null) && x.Id == pricingConfigId);

        if (isUsedInShipment)
        {
            _logger.Information("Soft deleting pricing configuration with ID: {PricingConfigId}", pricingConfigId);
            pricingConfig.DeletedAt = CoreHelper.SystemTimeNow;
            pricingConfig.WeightTiers?.ToList().ForEach(tier => tier.DeletedAt = pricingConfig.DeletedAt);
            pricingConfig.DistanceTiers?.ToList().ForEach(tier => tier.DeletedAt = pricingConfig.DeletedAt);

            _pricingRepository.Update(pricingConfig);
            await _unitOfWork.SaveChangeAsync();
        }
        else
        {
            _logger.Information("Permanently deleting pricing configuration with ID: {PricingConfigId}", pricingConfigId);
            _pricingRepository.Delete(pricingConfig);
            await _unitOfWork.SaveChangeAsync();
        }
        
        return ResponseMessagePricingConfig.PRICING_CONFIG_DELETE_SUCCESS;
    }
}