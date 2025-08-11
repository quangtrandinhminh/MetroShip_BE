using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels.ParcelCategory;
using MetroShip.Utility.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetroShip.Service.ApiModels.Parcel;

public class ParcelResponse
{
    public string Id { get; set; }
    public string ParcelCode { get; set; }
    public string ShipmentId { get; set; }

    public bool IsBulk { get; set; }
    public decimal VolumeCm3 { get; set; }
    public decimal ChargeableWeightKg { get; set; }
    public string? Description { get; set; }
    public decimal WeightKg { get; set; }
    public decimal LengthCm { get; set; }
    public decimal WidthCm { get; set; }
    public decimal HeightCm { get; set; }
    public decimal? ShippingFeeVnd { get; set; } = 0;
    public decimal? InsuranceFeeVnd { get; set; } = 0;
    public decimal? PriceVnd { get; set; }
    public ParcelStatusEnum Status { get; set; }
    public string? StatusName => Status.ToString();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastUpdatedAt { get; set; }
    public string? ParcelCategoryId { get; set; }
    public string? CategoryInsuranceId { get; set; }
    public ParcelCategoryResponse? ParcelCategory { get; set; }
    public IList<ParcelTrackingResponse> ParcelTrackings { get; set; } = new List<ParcelTrackingResponse>();
    public IList<ParcelMediaResponse> ParcelMedias { get; set; } = new List<ParcelMediaResponse>();

}