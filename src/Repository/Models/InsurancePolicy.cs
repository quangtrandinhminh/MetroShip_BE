using MetroShip.Repository.Models.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetroShip.Repository.Models;

public class InsurancePolicy : BaseEntity
{
    public string? Name { get; set; }

    public string? Description { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? BaseFeeVnd { get; set; } = 0;

    // Giá trị hàng hóa tối đa được bảo hiểm
    [Column(TypeName = "decimal(18, 2)")]
    public decimal? MaxParcelValueVnd { get; set; }

    // Tỷ lệ phí bảo hiểm * giá trị hàng hóa = phí bảo hiểm phải trả
    [Column(TypeName = "decimal(8, 6)")]
    public decimal? InsuranceFeeRateOnValue { get; set; }

    // Nếu không mua bảo hiểm, đền tối đa StandardCompensationValueVnd
    [Column(TypeName = "decimal(18, 2)")]
    public decimal? StandardCompensationValueVnd { get; set; }

    // Nếu mua bảo hiểm, đền tối đa là MaxInsuranceRate * giá trị hàng hóa
    [Column(TypeName = "decimal(8, 6)")]
    public decimal? MaxInsuranceRateOnValue { get; set; }

    // Nếu mua bảo hiểm, hư hại 1 phần, đền tối đa là MaxInsuranceRate * giá trị hàng hóa
    [Column(TypeName = "decimal(8, 6)")]
    public decimal? MinInsuranceRateOnValue { get; set; }

    [Column(TypeName = "date")]
    public DateOnly ValidFrom { get; set; }

    [Column(TypeName = "date")]
    public DateOnly? ValidTo { get; set; }

    public bool IsActive { get; set; }

    [InverseProperty(nameof(CategoryInsurance.InsurancePolicy))]
    public virtual ICollection<CategoryInsurance> CategoryInsurances { get; set; } = new List<CategoryInsurance>();
}