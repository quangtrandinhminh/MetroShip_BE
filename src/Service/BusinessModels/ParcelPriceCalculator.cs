using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels.Graph;
using MetroShip.Service.ApiModels.Parcel;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Exceptions;
using MetroShip.Utility.Helpers;
using Microsoft.AspNetCore.Http;

namespace MetroShip.Service.BusinessModels;

public static class ParcelPriceCalculator
{
    public static void CalculateParcelPricing(
        List<ParcelRequest> parcels,
        BestPathGraphResponse pathResponse,
        PriceCalculationService priceCalculationService,
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
            parcel.ShippingFeeVnd = priceCalculationService.
                CalculateShippingPrice(chargeableWeight, pathResponse.TotalKm);
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
            if (category.IsInsuranceRequired)
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
    }

    // old from shipment service
    /*private void CalculateParcelPricing(
        List<ParcelRequest> parcels,
        BestPathGraphResponse pathResponse,
        PriceCalculationService priceCalculationService,
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
            parcel.ShippingFeeVnd = priceCalculationService.
                CalculateShippingPrice(chargeableWeight, pathResponse.TotalKm);
            parcel.PriceVnd += parcel.ShippingFeeVnd;

            // Calculate insurance if required
            CalculateInsurance(parcel, categories);
        }
    }

    private void CalculateInsurance(ParcelRequest parcel, List<ParcelCategory> categories)
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
            if (category.IsInsuranceRequired)
            {
                parcel.PriceVnd += parcel.InsuranceFeeVnd;
                parcel.IsInsuranceIncluded = true;
            }
        }
    }

    private decimal? CalculateInsuranceFee(ParcelRequest parcel, ParcelCategory category)
    {
        if (category.InsuranceRate != null)
        {
            return parcel.ValueVnd * category.InsuranceRate;
        }

        return category.InsuranceFeeVnd;
    }*/
}