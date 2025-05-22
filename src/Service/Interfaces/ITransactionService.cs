using MetroShip.Service.ApiModels.Transaction;
using MetroShip.Service.ApiModels.VNPay;
using Microsoft.AspNetCore.Http;

namespace MetroShip.Service.Interfaces;

public interface ITransactionService
{
    Task<string> CreateVnPayTransaction(TransactionRequest request);
    Task ExecuteVnPayPayment(VnPayCallbackModel model);
}