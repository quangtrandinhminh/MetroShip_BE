using System.ComponentModel.DataAnnotations;
using MetroShip.Utility.Constants;

namespace MetroShip.Service.ApiModels.User;

public sealed record UserUpdateRequest : BankInfoRequest
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public string FullName { get; set; }
    public string? Address { get; set; }
    public string? Avatar { get; set; }
    public DateTimeOffset? BirthDate { get; set; }
}