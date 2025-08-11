using MetroShip.Repository.Models.Base;
using MetroShip.Utility.Helpers;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetroShip.Repository.Models;

public class CategoryInsurance : BaseEntity
{
    public string ParcelCategoryId { get; set; }
    public string InsurancePolicyId { get; set; }
    public bool IsActive { get; set; }

    [ForeignKey(nameof(ParcelCategoryId))]
    [InverseProperty(nameof(ParcelCategory.CategoryInsurances))]
    public virtual ParcelCategory ParcelCategory { get; set; } 

    [ForeignKey(nameof(InsurancePolicyId))]
    [InverseProperty(nameof(InsurancePolicy.CategoryInsurances))]
    public virtual InsurancePolicy InsurancePolicy { get; set; }

    [InverseProperty(nameof(Parcel.CategoryInsurance))]
    public virtual ICollection<Parcel> Parcels { get; set; } = new List<Parcel>();
}