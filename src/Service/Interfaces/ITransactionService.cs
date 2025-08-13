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
    Task<string?> ExecuteVnPayPayment(VnPayCallbackModel model);
    Task<PaginatedListResponse<TransactionResponse>> GetAllTransactionsAsync(PaginatedListRequest paginatedRequest, PaymentStatusEnum? status = null,
        string? searchKeyword = null, DateTimeOffset? createdFrom = null, DateTimeOffset? createdTo = null, OrderByRequest? orderByRequest = null);
}