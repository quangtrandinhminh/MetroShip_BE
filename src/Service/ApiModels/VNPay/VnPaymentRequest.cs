namespace MetroShip.Service.ApiModels.VNPay;

public sealed record VnPaymentRequest
{
    public string ReturnUrl { get; set; }
}