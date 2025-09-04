namespace MetroShip.Service.ApiModels.InsurancePolicy;

public record InsurancePolicyRequest
{
    public string? Name { get; set; }

    public string? Description { get; set; }

    public decimal? BaseFeeVnd { get; set; } = 0;

    // Giá trị hàng hóa tối đa được bảo hiểm
    public decimal? MaxParcelValueVnd { get; set; }

    // Tỷ lệ phí bảo hiểm * giá trị hàng hóa = phí bảo hiểm phải trả
    public decimal? InsuranceFeeRateOnValue { get; set; }

    // Nếu mua bảo hiểm, đền tối đa là MaxInsuranceRate * giá trị hàng hóa
    public decimal? MaxCompensationRateOnValue { get; set; }

    // Nếu mua bảo hiểm, hư hại 1 phần, đền tối đa là MaxInsuranceRate * giá trị hàng hóa
    public decimal? MinCompensationRateOnValue { get; set; }

    // Nếu không mua bảo hiểm, đền tối đa là x lần ShippingFeeVnd
    public decimal? MinCompensationRateOnShippingFee { get; set; }

    public bool IsActive { get; set; }
}