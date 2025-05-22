using MetroShip.Utility.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MetroShip.Service.ApiModels.Transaction;

public record TransactionRequest
{
    public string ShipmentId { get; set; }
    public string? ReturnUrl { get; init; } = string.Empty;
    public string? CancelUrl { get; init; } = string.Empty;
}