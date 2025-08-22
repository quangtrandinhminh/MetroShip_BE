using MetroShip.Service.ApiModels.VNPay;
using Microsoft.AspNetCore.Http;

namespace MetroShip.Service.Interfaces;

public interface IVnPayService
{
    Task<string> CreatePaymentUrl(string orderId, decimal totalAmount);
    Task<VnPaymentResponse> PaymentExecute(VnPayCallbackModel model);
    public int GetVnPayWaitingTimeMinutes();
}