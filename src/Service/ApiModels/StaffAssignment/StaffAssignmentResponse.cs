namespace MetroShip.Service.ApiModels.StaffAssignment;

public record StaffAssignmentResponse
{
    public string Id { get; init; } = string.Empty;
    public string StaffId { get; init; } = string.Empty;
    public string? StationId { get; init; }
    public string? TrainId { get; init; }
    public string? TrainCode { get; set; }
    public string? StationName { get; set; } = string.Empty;
    public string AssignedRole { get; init; } = string.Empty;
    public DateTimeOffset? FromTime { get; init; }
    public DateTimeOffset? ToTime { get; init; }
    public bool IsActive { get; init; }
    public string? Description { get; init; }
}