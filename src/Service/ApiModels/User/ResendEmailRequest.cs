using System.ComponentModel.DataAnnotations;

namespace MetroShip.Service.ApiModels.User;

public class ResendEmailRequest
{
    [Required]
    public string UserName { get; set; }
}