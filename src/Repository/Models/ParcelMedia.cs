using MetroShip.Utility.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MetroShip.Repository.Models.Base;

namespace MetroShip.Repository.Models;

public class ParcelMedia : BaseEntity
{
    [StringLength(50)]
    public string ParcelId { get; set; }
    public string MediaUrl { get; set; } = null!;
    public string? Description { get; set; }
    public BusinessMediaTypeEnum BusinessMediaType { get; set; }
    public MediaTypeEnum MediaType { get; set; }

    [ForeignKey(nameof(ParcelId))]
    [InverseProperty(nameof(Parcel.ParcelMedias))]
    public virtual Parcel Parcel { get; set; }
}