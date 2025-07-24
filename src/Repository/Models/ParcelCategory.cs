using System.ComponentModel.DataAnnotations;
using MetroShip.Repository.Models.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetroShip.Repository.Models;

public partial class ParcelCategory : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string CategoryName { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? WeightLimitKg { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? VolumeLimitCm3 { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? LengthLimitCm { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? WidthLimitCm { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? HeightLimitCm { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? TotalSizeLimitCm { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? InsuranceRate { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? InsuranceFeeVnd { get; set; }

    public bool IsInsuranceRequired { get; set; }

    public bool IsActive { get; set; } = true;

    [InverseProperty(nameof(Parcel.ParcelCategory))]
    public virtual ICollection<Parcel> Parcels { get; } = new List<Parcel>();
}