using MetroShip.Repository.Base;
using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Transaction;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Mapper;
using MetroShip.Utility.Enums;
using MetroShip.Utility.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace MetroShip.Service.Services;

public class TransactionService(IServiceProvider serviceProvider) : ITransactionService
{
    private readonly IBaseRepository<Transaction> _transactionRepository = serviceProvider.GetRequiredService<IBaseRepository<Transaction>>();
    private readonly IMapperlyMapper _mapper = serviceProvider.GetRequiredService<IMapperlyMapper>();
    private readonly ILogger<TransactionService> _logger = serviceProvider.GetRequiredService<ILogger<TransactionService>>();
    private readonly IUnitOfWork _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();

    public async Task<PaginatedListResponse<TransactionResponse>> GetAllAsync(PaymentStatusEnum? status, PaginatedListRequest request)
    {
        _logger.LogInformation("Fetching transactions. PaymentStatus: {status}", status);

        Expression<Func<Transaction, bool>> predicate = t => t.DeletedAt == null;

        if (status.HasValue)
        {
            predicate = predicate.And(t => t.PaymentStatus == status.Value);
        }

        var paginatedTransactions = await _transactionRepository.GetAllPaginatedQueryable(
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            predicate: predicate,
            orderBy: t => t.PaymentDate // Default sort
        );

        return _mapper.MapToTransactionPaginatedList(paginatedTransactions);
    }
}