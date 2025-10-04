#nullable disable
using MetroShip.Repository.Models.Base;
using MetroShip.Utility.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MetroShip.Repository.Models.Identity;

namespace MetroShip.Repository.Models;

public partial class StaffAssignment : BaseEntity
{
    [StringLength(50)]
    public string StaffId { get; set; }

    [StringLength(50)]
    public string? StationId { get; set; }
    public string? TrainId { get; set; }

    public AssignmentRoleEnum AssignedRole { get; set; }

    public DateTimeOffset? FromTime { get; set; }
    public DateTimeOffset? ToTime { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    [ForeignKey(nameof(StaffId))]
    [InverseProperty(nameof(UserEntity.StaffAssignments))]
    public virtual UserEntity Staff { get; set; }

    [ForeignKey(nameof(StationId))]
    [InverseProperty(nameof(Station.Staffs))]
    public virtual Station? Station { get; set; }

    [ForeignKey(nameof(TrainId))]
    [InverseProperty(nameof(MetroTrain.StaffAssignments))]
    public virtual MetroTrain? Train { get; set; }
}