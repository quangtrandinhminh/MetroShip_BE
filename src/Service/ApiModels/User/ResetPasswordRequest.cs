using System.ComponentModel.DataAnnotations;
using MetroShip.Utility.Constants;

namespace MetroShip.Service.ApiModels.User;

public class ResetPasswordRequest
{
    [Required]
    public string OTP { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [MinLength(8)]
    public string Password { get; set; }

    [Required]
    [Compare(nameof(Password), ErrorMessage = ResponseMessageIdentity.PASSWORD_NOT_MATCH)]
    public string ConfirmPassword { get; set; }

    [Required]
    public string UserName { get; set; }
}