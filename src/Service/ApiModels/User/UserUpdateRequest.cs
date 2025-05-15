using System.ComponentModel.DataAnnotations;
using MetroShip.Utility.Constants;

namespace MetroShip.Service.ApiModels.User;

public sealed record UserUpdateRequest : BankInfoRequest
{
    public int Id { get; set; }
    [Required(ErrorMessage = ResponseMessageIdentity.USERNAME_REQUIRED)]
    [MaxLength(100)]
    public string UserName { get; set; }

    [Required(ErrorMessage = ResponseMessageIdentity.NAME_REQUIRED)]
    [MaxLength(100)]
    [RegularExpression("^[^0-9]+$", ErrorMessage = "Name cannot contain number")]
    public string FullName { get; set; }
    public string? Address { get; set; }
    public string? Avatar { get; set; }
    public DateTimeOffset? BirthDate { get; set; }
}