using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels.Graph;
using MetroShip.Service.ApiModels.Parcel;
using MetroShip.Service.Interfaces;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Enums;
using MetroShip.Utility.Exceptions;
using MetroShip.Utility.Helpers;
using Microsoft.AspNetCore.Http;

namespace MetroShip.Service.BusinessModels;

public static class ParcelPriceCalculator
{
    public static void CalculateParcelPricing(
        List<ParcelRequest> parcels,
        BestPathGraphResponse pathResponse,
        IPricingService priceCalculationService,
        List<CategoryInsurance> categoryInsurances)
    {
        foreach (var parcel in parcels)
        {
            // Calculate chargeable weight
            var chargeableWeight = CalculateHelper.CalculateChargeableWeight(
                parcel.LengthCm, parcel.WidthCm, parcel.HeightCm, parcel.WeightKg);

            parcel.ChargeableWeight = chargeableWeight;
            parcel.IsBulk = parcel.ChargeableWeight > parcel.WeightKg;

            // Calculate shipping fee
            /*parcel.ShippingFeeVnd = priceCalculationService.
                CalculateShippingPrice(chargeableWeight, pathResponse.TotalKm);*/

            parcel.ShippingFeeVnd = priceCalculationService.
                CalculatePriceAsync(chargeableWeight, pathResponse.TotalKm).Result;
            parcel.PriceVnd += parcel.ShippingFeeVnd;

            // Calculate insurance if required
            CalculateInsurance(parcel, categoryInsurances);
        }
    }

    private static void CalculateInsurance(ParcelRequest parcel, List<CategoryInsurance> categoryInsurances)
    {
        var categoryInsurance = categoryInsurances.FirstOrDefault(c => c.Id == parcel.CategoryInsuranceId);
        if (categoryInsurance.ParcelCategory.IsInsuranceRequired && !parcel.ValueVnd.HasValue)
        {
            throw new AppException(
                ErrorCode.BadRequest,
                $"Loại hàng '{categoryInsurance.ParcelCategory.CategoryName}' yêu cầu bắt buộc mua bảo hiểm" +
                $"và cần giá trị hàng hóa của bưu kiện để tính phí bảo hiểm",
                StatusCodes.Status400BadRequest);
        }

        // Calculate insurance fee if any insurance configuration exists
        if (categoryInsurance.InsurancePolicy.InsuranceFeeRateOnValue != null || categoryInsurance.InsurancePolicy.BaseFeeVnd != null)
        {
            parcel.InsuranceFeeVnd = CalculateInsuranceFee(parcel, categoryInsurance.InsurancePolicy);

            // Add to price only if insurance is required
            if (categoryInsurance.ParcelCategory.IsInsuranceRequired || parcel.IsInsuranceIncluded)
            {
                parcel.PriceVnd += parcel.InsuranceFeeVnd;
                parcel.IsInsuranceIncluded = true;
            }
            else
            {
                parcel.InsuranceFeeVnd = 0;
            }
        }
    }

    private static decimal? CalculateInsuranceFee(ParcelRequest parcel, InsurancePolicy policy)
    {
        if (policy.InsuranceFeeRateOnValue != null && parcel.ValueVnd > 0)
        {
            return Math.Max((decimal)(parcel.ValueVnd * policy.InsuranceFeeRateOnValue), (decimal) policy.BaseFeeVnd!);
        }

        return policy.BaseFeeVnd;
    }

    /// <summary>
    /// Calculate compensation fee for parcels in a shipment based on their insurance policies.
    /// </summary>
    /// <param name="parcels">list of parcels with LOST status</param>
    /// <param name="categoryInsurances"></param>
    public static decimal CalculateParcelCompensation(
               List<Parcel> parcels,
                      List<CategoryInsurance> categoryInsurances, IParcelRepository parcelRepository)
    {
        decimal totalCompensation = 0;
        foreach (var parcel in parcels)
        {
            var categoryInsurance = categoryInsurances.FirstOrDefault(c => c.Id == parcel.CategoryInsuranceId);
      
            // Calculate compensation fee based on the insurance policy
            parcel.CompensationFeeVnd = CalculateCompensationFee(
                               parcel.ValueVnd, categoryInsurance.InsurancePolicy, parcel.InsuranceFeeVnd, parcel.ShippingFeeVnd);

            parcelRepository.Update(parcel);

            totalCompensation += parcel.CompensationFeeVnd ?? 0;
        }

        return totalCompensation;
    }

    private static decimal CalculateCompensationFee(
               decimal? valueVnd,
                      InsurancePolicy policy,
                             decimal? insuranceFeeVnd, decimal? shippingFeeVnd)
    {
        if (valueVnd is not > 0 || insuranceFeeVnd is not > 0)
        {
            return (decimal)(policy.MaxCompensationRateOnShippingFee * shippingFeeVnd.Value);
        }

        return policy.MaxCompensationRateOnValue != null
            ? valueVnd.Value * policy.MaxCompensationRateOnValue.Value
            : valueVnd.Value;
    }

    /*public static void CalculateParcelPricing(
        List<ParcelRequest> parcels,
        BestPathGraphResponse pathResponse,
        IPricingService priceCalculationService,
        List<ParcelCategory> categories)
    {
        foreach (var parcel in parcels)
        {
            // Calculate chargeable weight
            var chargeableWeight = CalculateHelper.CalculateChargeableWeight(
                parcel.LengthCm, parcel.WidthCm, parcel.HeightCm, parcel.WeightKg);

            parcel.ChargeableWeight = chargeableWeight;
            parcel.IsBulk = parcel.ChargeableWeight > parcel.WeightKg;

            // Calculate shipping fee
            /*parcel.ShippingFeeVnd = priceCalculationService.
                CalculateShippingPrice(chargeableWeight, pathResponse.TotalKm);#1#

            parcel.ShippingFeeVnd = priceCalculationService.
                CalculatePriceAsync(chargeableWeight, pathResponse.TotalKm).Result;
            parcel.PriceVnd += parcel.ShippingFeeVnd;

            // Calculate insurance if required
            CalculateInsurance(parcel, categories);
        }
    }

    private static void CalculateInsurance(ParcelRequest parcel, List<ParcelCategory> categories)
    {
        var category = categories.FirstOrDefault(c => c.Id == parcel.ParcelCategoryId);
        if (category.IsInsuranceRequired && !parcel.ValueVnd.HasValue)
        {
            throw new AppException(
                ErrorCode.BadRequest,
                $"Category '{category.CategoryName}' has required insurance " +
                $"and requires ValueVnd of the parcel for insurance calculation.",
                StatusCodes.Status400BadRequest);
        }

        // Calculate insurance fee if any insurance configuration exists
        if (category.InsuranceRate != null || category.InsuranceFeeVnd != null)
        {
            parcel.InsuranceFeeVnd = CalculateInsuranceFee(parcel, category);

            // Add to price only if insurance is required
            if (category.IsInsuranceRequired || parcel.IsInsuranceIncluded)
            {
                parcel.PriceVnd += parcel.InsuranceFeeVnd;
                parcel.IsInsuranceIncluded = true;
            }
        }
    }

    private static decimal? CalculateInsuranceFee(ParcelRequest parcel, ParcelCategory category)
    {
        if (category.InsuranceRate != null)
        {
            return parcel.ValueVnd * category.InsuranceRate;
        }

        return category.InsuranceFeeVnd;
    }*/
}