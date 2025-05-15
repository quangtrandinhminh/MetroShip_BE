using System.ComponentModel.DataAnnotations;

namespace MetroShip.Service.ApiModels.User;

public record BankInfoRequest
{
    public int? BankId { get; set; }

    [MaxLength(19)]
    public string? AccountNo { get; set; }

    [MaxLength(255)]
    public string? AccountName { get; set; }
}