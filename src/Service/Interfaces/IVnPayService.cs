using MetroShip.Service.ApiModels.VNPay;
using Microsoft.AspNetCore.Http;

namespace MetroShip.Service.Interfaces;

public interface IVnPayService
{
    Task<string> CreatePaymentUrl(HttpContext context, string orderId, int totalAmount);
    Task<VnPaymentResponse> PaymentExecute(HttpContext context);
}