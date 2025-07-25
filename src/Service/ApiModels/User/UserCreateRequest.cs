using System.ComponentModel.DataAnnotations;
using MetroShip.Utility.Constants;

namespace MetroShip.Service.ApiModels.User;

public sealed record UserCreateRequest
{
    public string UserName { get; set; }

    public string FullName { get; set; }

    public string Email { get; set; }

    public string PhoneNumber { get; set; }

    public string Password { get; set; }

    public string ConfirmPassword { get; set; }

    public DateTimeOffset? BirthDate { get; set; }

    [Required(ErrorMessage = ResponseMessageIdentity.ROLES_REQUIRED)]
    public int Role { get; set; }
}