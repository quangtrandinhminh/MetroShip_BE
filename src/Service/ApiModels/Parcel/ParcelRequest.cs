using MetroShip.Utility.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MetroShip.Service.ApiModels.Parcel;

public record ParcelRequest
{
    public string ParcelCategoryId { get; set; }
    public decimal WeightKg { get; set; }
    public decimal LengthCm { get; set; }
    public decimal WidthCm { get; set; }
    public decimal HeightCm { get; set; }
    public bool IsBulk { get; set; }
    public decimal PriceVnd { get; set; }
}