using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Pricing;

namespace MetroShip.Service.Interfaces;

public interface IPricingService
{
    Task<PaginatedListResponse<PricingTableResponse>> GetPricingPaginatedList(PaginatedListRequest request,
        bool? isActive = null);
    Task<decimal> CalculatePriceAsync(decimal weightKg, decimal distanceKm);
    Task<PricingTableResponse> GetPricingTableAsync(string? pricingConfigId);
    Task<decimal> CalculateRefund(string pricingConfigId, decimal? totalPrice);
    Task CalculateOverdueSurcharge(Shipment shipment);
    Task<int> GetFreeStoreDaysAsync(string pricingConfigId);
    Task<int> GetRefundForCancellationBeforeScheduledHours(string pricingConfigId);
    Task<string> ChangePricingConfigAsync(PricingConfigRequest request);
    Task<string> ActivatePricingConfigAsync(string pricingConfigId);
    Task<string> DeletePricingConfigAsync(string pricingConfigId);
}