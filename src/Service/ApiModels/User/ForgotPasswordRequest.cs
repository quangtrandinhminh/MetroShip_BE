using System.ComponentModel.DataAnnotations;

namespace MetroShip.Service.ApiModels.User;

public class ForgotPasswordRequest
{
    [Required]
    public string UserName { get; set; }
}