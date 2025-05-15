using Net.payOS.Types;

namespace MetroShip.Service.ApiModels.PayOS;

public class PayOSResponse
{
    public string? Code { get; set; }
    public string? Desc { get; set; }
    public PaymentLinkInformation? Data { get; set; }
    public string? Signature { get; set; }
}