using MetroShip.Utility.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MetroShip.Utility.Helpers;

namespace MetroShip.Service.ApiModels.Parcel;

public record ParcelRequest
{
    public string ParcelCategoryId { get; set; }
    public decimal WeightKg { get; set; }
    public decimal LengthCm { get; set; }
    public decimal WidthCm { get; set; }
    public decimal HeightCm { get; set; }
    public decimal? ShippingFeeVnd { get; set; } = 0;
    public decimal? InsuranceFeeVnd { get; set; } = 0;
    public decimal? ChargeableWeight { get; set; }
    public bool? IsBulk { get; set; }
    public decimal? PriceVnd { get; set; } = 0;
    public decimal? ValueVnd { get; set; } = null;
}