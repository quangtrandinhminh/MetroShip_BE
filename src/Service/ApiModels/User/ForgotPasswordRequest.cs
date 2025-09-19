using System.ComponentModel.DataAnnotations;

namespace MetroShip.Service.ApiModels.User;

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}