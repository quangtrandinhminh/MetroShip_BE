using MetroShip.Service.ApiModels.Transaction;
using MetroShip.Service.ApiModels.VNPay;
using Microsoft.AspNetCore.Http;
﻿using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Transaction;
using MetroShip.Utility.Enums;

namespace MetroShip.Service.Interfaces;

public interface ITransactionService
{
    Task<string> CreateVnPayTransaction(TransactionRequest request);
    Task ExecuteVnPayPayment(VnPayCallbackModel model);
    Task<PaginatedListResponse<TransactionResponse>> GetAllAsync(PaymentStatusEnum? status, PaginatedListRequest request);
}