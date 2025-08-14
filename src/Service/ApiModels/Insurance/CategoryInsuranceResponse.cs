using MetroShip.Service.ApiModels.Parcel;
using MetroShip.Service.ApiModels.ParcelCategory;
using MetroShip.Utility.Helpers;

namespace MetroShip.Service.ApiModels.Insurance;

public record CategoryInsuranceResponse
{
    public string Id { get; set; }
    public InsuranceResponse? InsurancePolicy { get; set; }
}

public record InsuranceResponse
{
    public string Id { get; set; }
    public string? Name { get; set; }

    public string? Description { get; set; }

    public decimal? BaseFeeVnd { get; set; } = 0;

    // Giá trị hàng hóa tối đa được bảo hiểm
    public decimal? MaxParcelValueVnd { get; set; }

    // Tỷ lệ phí bảo hiểm * giá trị hàng hóa = phí bảo hiểm phải trả
    public decimal? InsuranceFeeRateOnValue { get; set; }

    // Nếu không mua bảo hiểm, đền tối đa StandardCompensationValueVnd
    public decimal? StandardCompensationValueVnd { get; set; }

    // Nếu mua bảo hiểm, đền tối đa là MaxInsuranceRate * giá trị hàng hóa
    public decimal? MaxInsuranceRateOnValue { get; set; }

    // Nếu mua bảo hiểm, hư hại 1 phần, đền tối đa là MaxInsuranceRate * giá trị hàng hóa
    public decimal? MinInsuranceRateOnValue { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }

    public bool IsActive { get; set; }
}