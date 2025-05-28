using MetroShip.Repository.Models;
using MetroShip.Utility.Enums;

namespace MetroShip.Service.ApiModels.Parcel;

public class ParcelResponse
{
    public string ParcelCode { get; set; }
    public decimal VolumeCm3 { get; set; }
    public decimal ChargeableWeightKg { get; set; }
    public string? Description { get; set; }
    public string? PriceVnd { get; set; }
    public ParcelStatusEnum Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public IList<ParcelTracking> ParcelTrackings { get; set; } = new List<ParcelTracking>();
}