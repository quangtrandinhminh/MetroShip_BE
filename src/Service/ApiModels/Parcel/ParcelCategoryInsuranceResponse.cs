using MetroShip.Service.ApiModels.Insurance;
using MetroShip.Service.ApiModels.ParcelCategory;

namespace MetroShip.Service.ApiModels.Parcel;

public record ParcelCategoryInsuranceResponse
{
    public string Id { get; set; }
    public InsuranceResponse? InsurancePolicy { get; set; }
    public CategoryResponse? ParcelCategory { get; set; }
}