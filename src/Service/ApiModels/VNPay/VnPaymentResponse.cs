namespace MetroShip.Service.ApiModels.VNPay;

public class VnPaymentResponse
{
    public bool Success { get; set; }
    public string PaymentMethod { get; set; }
    public string OrderDescription { get; set; }
    public string OrderId { get; set; }
    public string PaymentId { get; set; }
    public string TransactionId { get; set; }
    public string Token { get; set; }
    public string VnPayResponseCode { get; set; }
    public string Message { get; set; }
}