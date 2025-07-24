using MetroShip.Utility.Enums;

namespace MetroShip.Service.ApiModels.StaffAssignment;

public record StaffAssignmentRequest
{
    public string StaffId { get; init; }
    public string StationId { get; init; }
    public AssignmentRoleEnum AssignedRole { get; init; }
    public DateTimeOffset? FromTime { get; init; }
    public DateTimeOffset? ToTime { get; init; }
    public string? Description { get; init; }
}