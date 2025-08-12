using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels.Pricing;

namespace MetroShip.Service.Interfaces;

public interface IPricingService
{
    Task<decimal> CalculatePriceAsync(decimal weightKg, decimal distanceKm);
    Task<PricingTableResponse> GetPricingTableAsync(string? pricingConfigId);
    Task<decimal> CalculateRefund(decimal? totalPrice);
    Task CalculateOverdueSurcharge(Shipment shipment);
    Task<int> GetFreeStoreDaysAsync();
}