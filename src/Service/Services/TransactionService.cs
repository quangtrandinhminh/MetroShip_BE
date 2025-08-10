using MetroShip.Repository.Base;
using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Transaction;
using MetroShip.Service.ApiModels.VNPay;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Mapper;
using MetroShip.Service.Utils;
using MetroShip.Service.Validations;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Enums;
using MetroShip.Utility.Exceptions;
using MetroShip.Utility.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Quartz;
using Serilog;
using System.Linq.Expressions;

namespace MetroShip.Service.Services;

public class TransactionService(IServiceProvider serviceProvider) : ITransactionService
{
    private readonly IVnPayService _vnPayService = serviceProvider.GetRequiredService<IVnPayService>();
    private readonly IShipmentRepository _shipmentRepository = serviceProvider.GetRequiredService<IShipmentRepository>();
    private readonly IMapperlyMapper _mapper = serviceProvider.GetRequiredService<IMapperlyMapper>();
    private readonly IHttpContextAccessor _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
    private readonly IUnitOfWork _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
    private readonly IBaseRepository<Parcel> _parcelRepository = serviceProvider.GetRequiredService<IBaseRepository<Parcel>>();
    private readonly IBaseRepository<Transaction> _transaction = serviceProvider.GetRequiredService<IBaseRepository<Transaction>>();
    private readonly ISchedulerFactory _schedulerFactory = serviceProvider.GetRequiredService<ISchedulerFactory>();

    public async Task<string> CreateVnPayTransaction(TransactionRequest request)
    {
        var customerId = JwtClaimUltils.GetUserId(_httpContextAccessor);
        _logger.Information("Creating payment link for: {shipment} by {CustomerId}",
            request.ShipmentId, customerId);
        TransactionValidator.ValidateTransactionRequest(request);
        var shipment = await _shipmentRepository.GetSingleAsync(
                       x => x.Id == request.ShipmentId || x.TrackingCode == request.ShipmentId,
                       includeProperties: x => x.Transactions
                       );

        if (shipment == null)
        {
            throw new AppException(ErrorCode.BadRequest,
                "Shipment not found",
                StatusCodes.Status404NotFound);
        }
        
        // Check if the shipment is in a valid state for payment
        if (shipment.ShipmentStatus is not (ShipmentStatusEnum.AwaitingPayment 
            /*or ShipmentStatusEnum.PartiallyConfirmed*/))
        {
            throw new AppException(
                ErrorCode.BadRequest,
                "Shipment is not in a valid state for payment",
                StatusCodes.Status400BadRequest);
        }

        if (shipment.Transactions.FirstOrDefault(
            t => t.TransactionType == TransactionTypeEnum.ShipmentCost
        && t.PaymentStatus == PaymentStatusEnum.Pending) is null)
        {
            var transaction = _mapper.MapToTransactionEntity(request);
            transaction.PaidById = customerId;
            transaction.PaymentMethod = PaymentMethodEnum.VnPay;
            transaction.PaymentStatus = PaymentStatusEnum.Pending;
            transaction.PaymentAmount = shipment.TotalCostVnd;
            transaction.TransactionType = TransactionTypeEnum.ShipmentCost;
            transaction.Description = request.ToJsonString();
            shipment.Transactions.Add(transaction);
            _shipmentRepository.Update(shipment);
            await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
        }

        var paymentUrl = await _vnPayService.CreatePaymentUrl(
            shipment.TrackingCode, shipment.TotalCostVnd);
        return paymentUrl;
    }

    public async Task<string?> ExecuteVnPayPayment(VnPayCallbackModel model)
    {
        _logger.Information("Executing VnPay payment with context: {context}", model);
        string response = null;
        var vnPaymentResponse = await _vnPayService.PaymentExecute(model);
        if (vnPaymentResponse == null)
        {
            throw new AppException("Invalid payment response");
        }

        var shipment = await _shipmentRepository.GetSingleAsync(
                       x => x.Id == vnPaymentResponse.OrderId 
                       || x.TrackingCode == vnPaymentResponse.OrderId, 
                       false,
                       x => x.Transactions, x => x.Parcels
                       );

        if (shipment == null)
        {
            throw new AppException(
                ErrorCode.BadRequest,
                ResponseMessageShipment.SHIPMENT_NOT_FOUND,
                StatusCodes.Status400BadRequest);
        }

        // Get only the transaction that is pending for shipment cost
        var transaction = shipment.Transactions
            .FirstOrDefault(x => x.TransactionType == TransactionTypeEnum.ShipmentCost
            && x.PaymentStatus == PaymentStatusEnum.Pending
            );
        if (transaction == null)
        {
            throw new AppException(
                ErrorCode.BadRequest,
                ResponseMessageTransaction.TRANSACTION_NOT_FOUND,
                StatusCodes.Status400BadRequest);
        }

        // Deserialize the transaction request from the transaction description for return URLs
        var transactionRequest = JsonConvert
            .DeserializeObject<TransactionRequest>(transaction.Description);

        if (transactionRequest == null)
        {
            throw new AppException(
            ErrorCode.BadRequest,
            "Invalid transaction request data, cannot execute",
            StatusCodes.Status400BadRequest);
        }

        // Update the shipment status based on the payment response
        if (vnPaymentResponse.VnPayResponseCode == "00")
        {
            _logger.Information("Payment successful for shipment: {shipmentId}", shipment.Id);
            shipment.ShipmentStatus = ShipmentStatusEnum.AwaitingDropOff;
            shipment.PaidAt = CoreHelper.SystemTimeNow;

            transaction.PaymentStatus = PaymentStatusEnum.Paid;
            //await HandleShipmentParcelTransaction(shipment);
            response = transactionRequest.ReturnUrl;
        }
        else if (vnPaymentResponse.VnPayResponseCode == "24")
        {
            _logger.Information("Payment cancelled for shipment: {shipmentId}", shipment.Id);
            transaction.PaymentStatus = PaymentStatusEnum.Cancelled;
            response = transactionRequest.CancelUrl;
        }
        else
        {
            _logger.Error("Payment failed for shipment: {shipmentId} with response code: {responseCode}",
                               shipment.Id, vnPaymentResponse.VnPayResponseCode);
            transaction.PaymentStatus = PaymentStatusEnum.Failed;
            response = transactionRequest.CancelUrl;
        }

        transaction.PaymentTrackingId = vnPaymentResponse.TransactionId;
        transaction.PaymentTime = DateTimeOffset.ParseExact(
            vnPaymentResponse.PaymentTime,
            "yyyyMMddHHmmss",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.AssumeUniversal
        ).ToUniversalTime();
        transaction.PaymentCurrency = "VND";
        transaction.Description = JsonConvert.SerializeObject(vnPaymentResponse);
        transaction.PaymentDate = CoreHelper.SystemTimeNow;
        _shipmentRepository.Update(shipment);

        // Cancel any scheduled job for this shipment
        await CancelScheduledUnpaidJob(shipment.Id);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
        return response;
    }

    public async Task<TransactionListWithStatsResponse> GetAllTransactionsAsync(PaginatedListRequest paginatedRequest, PaymentStatusEnum? status = null,
        string? searchKeyword = null,DateTimeOffset? createdFrom = null, DateTimeOffset? createdTo = null, OrderByRequest? orderByRequest = null)
    {
        var customerId = JwtClaimUltils.GetUserId(_httpContextAccessor);
        var userRole = JwtClaimUltils.GetUserRole(_httpContextAccessor);

        _logger.Information(
            $"Get all transactions, status: {status}, search: '{searchKeyword}', order by: '{orderByRequest?.OrderBy}' {(orderByRequest?.IsDesc == true ? "desc" : "asc")}");

        // ===== FILTER =====
        Expression<Func<Transaction, bool>> predicate = t => t.DeletedAt == null;

        if (status.HasValue)
            predicate = predicate.And(t => t.PaymentStatus == status.Value);

        if (!string.IsNullOrEmpty(customerId) && userRole.Contains(UserRoleEnum.Customer.ToString()))
            predicate = predicate.And(t => t.PaidById == customerId);

        // Search keyword
        if (!string.IsNullOrWhiteSpace(searchKeyword))
        {
            var keywordLower = searchKeyword.Trim().ToLower();
            predicate = predicate.And(t =>
                (t.PaymentTrackingId != null && t.PaymentTrackingId.ToLower().Contains(keywordLower)) ||
                (t.PaymentCurrency != null && t.PaymentCurrency.ToLower().Contains(keywordLower))
            );
        }

        // Created date range
        if (createdFrom.HasValue)
            predicate = predicate.And(t => t.CreatedAt >= createdFrom.Value);
        if (createdTo.HasValue)
            predicate = predicate.And(t => t.CreatedAt <= createdTo.Value);

        // ===== SORT =====
        Expression<Func<Transaction, object>>? orderBy = orderByRequest?.OrderBy?.ToLower() switch
        {
            "paymenttrackingid" => t => t.PaymentTrackingId!,
            "paymentdate" => t => t.PaymentDate,
            "paymentamount" => t => t.PaymentAmount,
            _ => t => t.CreatedAt
        };

        // ===== GET LIST =====
        var transactions = await _transaction.GetAllPaginatedQueryable(
            paginatedRequest.PageNumber,
            paginatedRequest.PageSize,
            predicate,
            orderBy
        );

        var transactionResponse = _mapper.MapToTransactionPaginatedList(transactions);

        // ===== STATS =====
        var query = _transaction.GetAllWithCondition().Where(t => t.DeletedAt == null);

        var totalTransactions = await query.CountAsync();

        var todayVietnamTime = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)).Date;
        var todayUtc = todayVietnamTime.ToUniversalTime();

        var newTransactionsCount = await query.CountAsync(t => t.CreatedAt >= todayUtc);
        var percentageNewTransactions = totalTransactions > 0
            ? Math.Round((double)newTransactionsCount / totalTransactions * 100, 2)
            : 0;

        // Completed (Paid)
        var totalCompletedTransactions = await query.CountAsync(t => t.PaymentStatus == PaymentStatusEnum.Paid);
        var totalCompletedAmount = await query
            .Where(t => t.PaymentStatus == PaymentStatusEnum.Paid)
            .SumAsync(t => t.PaymentAmount);

        var newCompletedTransactionsCount = await query.CountAsync(
            t => t.PaymentStatus == PaymentStatusEnum.Paid && t.CreatedAt >= todayUtc);
        var percentageNewCompletedTransactions = totalCompletedTransactions > 0
            ? Math.Round((double)newCompletedTransactionsCount / totalCompletedTransactions * 100, 2)
            : 0;

        // Unpaid
        var totalUnpaidTransactions = await query.CountAsync(t => t.PaymentStatus == PaymentStatusEnum.Failed);
        var percentageUnpaidTransactions = totalTransactions > 0
            ? Math.Round((double)totalUnpaidTransactions / totalTransactions * 100, 2)
            : 0;

        // Pending
        var totalPendingTransactions = await query.CountAsync(t => t.PaymentStatus == PaymentStatusEnum.Pending);
        var percentagePendingTransactions = totalTransactions > 0
            ? Math.Round((double)totalPendingTransactions / totalTransactions * 100, 2)
            : 0;

        // Cancelled
        var totalCancelledTransactions = await query.CountAsync(t => t.PaymentStatus == PaymentStatusEnum.Cancelled);
        var percentageCancelledTransactions = totalTransactions > 0
            ? Math.Round((double)totalCancelledTransactions / totalTransactions * 100, 2)
            : 0;

        return new TransactionListWithStatsResponse
        {
            Transactions = transactionResponse,
            TotalTransactions = totalTransactions,
            PercentageNewTransactions = percentageNewTransactions,

            TotalPaidTransactions = totalCompletedTransactions,
            PercentageNewPaidTransactions = percentageNewCompletedTransactions,
            TotalPaiddAmount = totalCompletedAmount,

            TotalUnpaidTransactions = totalUnpaidTransactions,
            PercentageUnpaidTransactions = percentageUnpaidTransactions,

            TotalPendingTransactions = totalPendingTransactions,
            PercentagePendingTransactions = percentagePendingTransactions,

            TotalCancelledTransactions = totalCancelledTransactions,
            PercentageCancelledTransactions = percentageCancelledTransactions
        };
    }

    // tìm transaction loại refund theo shipmentId, cộng amount vào ví ảo của user
    public async Task RefundTransactionAsync(string shipmentId, decimal amount)
    {
        var staffId = JwtClaimUltils.GetUserId(_httpContextAccessor);
        _logger.Information("Processing refund for shipment: {shipmentId} by customer: {staffId}", 
            shipmentId, staffId);

        var shipment = await _shipmentRepository.GetSingleAsync(
                       x => x.Id == shipmentId || x.TrackingCode == shipmentId,
                                  includeProperties: x => x.Transactions
                                         );

        if (shipment == null)
        {
            throw new AppException(ErrorCode.BadRequest,
                               "Shipment not found",
                                              StatusCodes.Status404NotFound);
        }

        // Check if the shipment has a transaction of type Refund
        var refundTransaction = shipment.Transactions.FirstOrDefault(
                       t => t.TransactionType == TransactionTypeEnum.Refund && t.PaymentStatus == PaymentStatusEnum.Pending);

        if (refundTransaction == null)
        {
            throw new AppException(ErrorCode.BadRequest,
                               "No pending refund transaction found for this shipment",
                                              StatusCodes.Status400BadRequest);
        }

        // Update the refund transaction
        refundTransaction.PaymentStatus = PaymentStatusEnum.Paid;
        refundTransaction.PaymentAmount += amount;
        refundTransaction.PaidById = staffId;

        // Update the user's virtual wallet balance
        // Assuming you have a method to update the user's wallet balance
        //await _parcelRepository.UpdateUserWalletBalanceAsync(staffId, amount);

        _shipmentRepository.Update(shipment);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
    }

    // Cancel ScheduleUnpaidJob
    public async Task CancelScheduledUnpaidJob(string shipmentId)
    {
        _logger.Information("Cancelling scheduled job to update shipment status to unpaid for ID: {@shipmentId}", shipmentId);
        var jobKey = new JobKey($"UpdateShipmentToUnpaid-{shipmentId}");
        var scheduler = await _schedulerFactory.GetScheduler();
        if (await scheduler.CheckExists(jobKey))
        {
            await scheduler.DeleteJob(jobKey);
            _logger.Information("Cancelled scheduled job for shipment ID: {@shipmentId}", shipmentId);
        }
        else
        {
            _logger.Warning("No scheduled job found for shipment ID: {@shipmentId}", shipmentId);
        }
    }
}