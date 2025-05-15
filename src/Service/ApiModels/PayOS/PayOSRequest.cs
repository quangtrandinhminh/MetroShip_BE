namespace MetroShip.Service.ApiModels.PayOS;

public class PayOSRequest
{
    public long OrderId { get; set; }
    public string CancelUrl { get; set; }
    public string ReturnUrl { get; set; }
}