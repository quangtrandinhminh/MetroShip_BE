using MetroShip.Service.ApiModels.StaffAssignment;

namespace MetroShip.Service.ApiModels.User;

public record UserResponse : BankInfoRequest
{
    public string Id { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? FullName { get; set; } = string.Empty;
    public string? Address { get; set; } = string.Empty;
    public string? Avatar { get; set; } = string.Empty;
    public IList<string?> Role { get; set; } = new List<string?>();
    public DateOnly? BirthDate { get; set; }
    public List<StaffAssignmentResponse>? StaffAssignments { get; set; }
}