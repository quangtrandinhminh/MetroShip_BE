using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Transaction;
using MetroShip.Utility.Enums;

namespace MetroShip.Service.Interfaces;

public interface ITransactionService
{
    Task<PaginatedListResponse<TransactionResponse>> GetAllAsync(PaymentStatusEnum? status, PaginatedListRequest request);
}