using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels.ParcelCategory;
using MetroShip.Utility.Enums;

namespace MetroShip.Service.ApiModels.Parcel;

public class ParcelResponse
{
    public string Id { get; set; }
    public string ParcelCode { get; set; }
    public string ShipmentId { get; set; }
    public decimal VolumeCm3 { get; set; }
    public decimal ChargeableWeightKg { get; set; }
    public string? Description { get; set; }
    public string? PriceVnd { get; set; }
    public ParcelStatusEnum ParcelStatus { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? ParcelCategoryId { get; set; }
    public ParcelCategoryResponse? ParcelCategory { get; set; }
    public IList<ParcelTrackingResponse> ParcelTrackings { get; set; } = new List<ParcelTrackingResponse>();
    
}