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
using Microsoft.Extensions.DependencyInjection;
using MetroShip.Repository.Repositories;
using MetroShip.Service.Jobs;
using Microsoft.Extensions.Caching.Memory;

namespace MetroShip.Service.Services;

public class TransactionService(IServiceProvider serviceProvider) : ITransactionService
{
    private readonly IVnPayService _vnPayService = serviceProvider.GetRequiredService<IVnPayService>();
    private readonly IShipmentRepository _shipmentRepository = serviceProvider.GetRequiredService<IShipmentRepository>();
    private readonly IMapperlyMapper _mapper = serviceProvider.GetRequiredService<IMapperlyMapper>();
    private readonly IHttpContextAccessor _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
    private readonly IUnitOfWork _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
    private readonly ITransactionRepository _transactionRepository = serviceProvider.GetRequiredService<ITransactionRepository>();
    private readonly IBaseRepository<ShipmentTracking> _shipmentTrackingRepository = serviceProvider.GetRequiredService<IBaseRepository<ShipmentTracking>>();
    private readonly IBackgroundJobService _backgroundJobService = serviceProvider.GetRequiredService<IBackgroundJobService>();
    private readonly IMemoryCache _memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();

    public async Task<string> CreateVnPayTransaction(TransactionRequest request)
    {
        var userId = JwtClaimUltils.GetUserId(_httpContextAccessor);
        _logger.Information("Creating payment link for: {shipment} by user {User}",
            request.ShipmentId, userId);
        TransactionValidator.ValidateTransactionRequest(request);
        var shipment = await _shipmentRepository.GetSingleAsync(
                       x => x.Id == request.ShipmentId || x.TrackingCode == request.ShipmentId);

        if (shipment == null)
        {
            throw new AppException(ErrorCode.BadRequest,
                ResponseMessageShipment.SHIPMENT_NOT_FOUND,
                StatusCodes.Status404NotFound);
        }

        ShipmentStatusEnum[] validStatus =
        {
            ShipmentStatusEnum.AwaitingPayment,
            ShipmentStatusEnum.AwaitingRefund,
            ShipmentStatusEnum.ApplyingSurcharge,
            ShipmentStatusEnum.DeliveredPartially,
            ShipmentStatusEnum.ToCompensate
        };

        // Check if the shipment is in a valid state for payment
        if (!validStatus.Contains(shipment.ShipmentStatus))
        {
            throw new AppException(
                ErrorCode.BadRequest,
                $"Đơn hàng {shipment.TrackingCode} không thể thanh toán trong trạng thái hiện tại. " +
                $"Các trạng thái hợp lệ: {string.Join(", ", validStatus)}",
                StatusCodes.Status400BadRequest);
        }

        var transactionExists = await _transactionRepository.GetSingleAsync(
                       x => (x.ShipmentId == shipment.Id || x.Shipment.TrackingCode == shipment.TrackingCode)
                                  && x.TransactionType == request.TransactionType.Value
                                             && x.PaymentStatus == PaymentStatusEnum.Pending);
        if (transactionExists != null)
        {
            return transactionExists.PaymentUrl ?? string.Empty;
        }

        /*var transaction = _mapper.MapToTransactionEntity(request);
        transaction.PaidById = userId;
        transaction.PaymentMethod = PaymentMethodEnum.VnPay;
        transaction.PaymentStatus = PaymentStatusEnum.Pending;
        transaction.ShipmentId = shipment.Id;
        transaction.TransactionType = request.TransactionType.Value;
        transaction.Description = request.ToJsonString();
        transaction.PaymentAmount = shipment.TotalCostVnd;*/

        var transaction = await HandleCreateTransaction(shipment, request);

        var paymentUrl = await _vnPayService.CreatePaymentUrl(
            transaction.Id, transaction.PaymentAmount);
        transaction.PaymentUrl = paymentUrl;
        _transactionRepository.Add(transaction);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
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

        var transaction = await _transactionRepository.GetSingleAsync(
                       x => x.Id == vnPaymentResponse.OrderId
                                       && x.PaymentMethod == PaymentMethodEnum.VnPay
                                                       && x.PaymentStatus == PaymentStatusEnum.Pending,
                                  includeProperties: x => x.Shipment);

        var shipment = transaction?.Shipment;

        /*var shipment = await _shipmentRepository.GetSingleAsync(
                       x => x.Transactions.FirstOrDefault(t => t.Id == vnPaymentResponse.OrderId) 
                       || x.TrackingCode == vnPaymentResponse.OrderId, 
                       false,
                       x => x.Transactions
                       );

        if (shipment == null)
        {
            throw new AppException(
                ErrorCode.BadRequest,
                ResponseMessageShipment.SHIPMENT_NOT_FOUND,
                StatusCodes.Status400BadRequest);
        }

        // Get only the transaction that is pending for shipment cost
        var transaction = await _transactionRepository.GetSingleAsync(
                       x => (x.ShipmentId == shipment.Id || x.Shipment.TrackingCode == shipment.TrackingCode)
                                                       && x.PaymentStatus == PaymentStatusEnum.Pending);*/
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
            await HandlePaymentForShipment(shipment, transaction.PaidById);

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
        transaction.Description = vnPaymentResponse.ToJsonString();
        transaction.PaymentDate = CoreHelper.SystemTimeNow;
        _shipmentRepository.Update(shipment);

        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
        await _backgroundJobService.CancelScheduleCancelTransactionJob(transaction.Id);
        return response;
    }

    public async Task<PaginatedListResponse<TransactionResponse>> GetAllTransactionsAsync(PaginatedListRequest paginatedRequest, PaymentStatusEnum? status = null,
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
            predicate = predicate.And(t => t.PaidById == customerId || t.Shipment.SenderId == customerId);

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
        var transactions = await _transactionRepository.GetAllPaginatedQueryable(
            paginatedRequest.PageNumber,
            paginatedRequest.PageSize,
            predicate,
            orderBy
        );

        var transactionResponse = _mapper.MapToTransactionPaginatedList(transactions);

        return transactionResponse;
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
                               ResponseMessageShipment.SHIPMENT_NOT_FOUND,
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

    /*private async Task HandlePaymentForShipment(Shipment shipment, string userId)
    {
        _logger.Information("Handling payment for shipment: {shipmentId}", shipment.Id);

        if (shipment.ShipmentStatus == ShipmentStatusEnum.AwaitingPayment)
        {
            if (!shipment.IsReturnShipment)
            {
                shipment.ShipmentStatus = ShipmentStatusEnum.AwaitingDropOff;
                shipment.PaidAt = CoreHelper.SystemTimeNow;
                _shipmentTrackingRepository.Add(new ShipmentTracking
                {
                    ShipmentId = shipment.Id,
                    CurrentShipmentStatus = shipment.ShipmentStatus,
                    Status = $"Người gửi đã thanh toán cho đơn hàng {shipment.TrackingCode} qua VnPay",
                    EventTime = CoreHelper.SystemTimeNow,
                    UpdatedBy = userId,
                });

                await _backgroundJobService.CancelScheduledUnpaidJob(shipment.Id);
                await _backgroundJobService.ScheduleUpdateNoDropOffJob(shipment.Id, shipment.ScheduledDateTime.Value);
            }
            else
            {
                shipment.ShipmentStatus = ShipmentStatusEnum.PickedUp;
                shipment.PaidAt = CoreHelper.SystemTimeNow;

                _shipmentTrackingRepository.Add(new ShipmentTracking
                {
                    ShipmentId = shipment.Id,
                    CurrentShipmentStatus = shipment.ShipmentStatus,
                    Status = $"Người gửi đã thanh toán cho đơn hàng {shipment.TrackingCode} qua VnPay",
                    EventTime = CoreHelper.SystemTimeNow,
                    UpdatedBy = userId,
                });
            }
        }
        else if (shipment.ShipmentStatus == ShipmentStatusEnum.AwaitingRefund)
        {
            shipment.ShipmentStatus = ShipmentStatusEnum.Refunded;
            shipment.RefundedAt = CoreHelper.SystemTimeNow;
            _shipmentTrackingRepository.Add(new ShipmentTracking
            {
                ShipmentId = shipment.Id,
                CurrentShipmentStatus = shipment.ShipmentStatus,
                Status = $"Nhân viên đã hoàn tiền cho đơn hàng {shipment.TrackingCode}",
                EventTime = CoreHelper.SystemTimeNow,
                UpdatedBy = userId,
            });
        }
        else if (shipment.ShipmentStatus == ShipmentStatusEnum.ApplyingSurcharge)
        {
            shipment.ShipmentStatus = ShipmentStatusEnum.AwaitingDelivery;
            _shipmentTrackingRepository.Add(new ShipmentTracking
            {
                ShipmentId = shipment.Id,
                CurrentShipmentStatus = shipment.ShipmentStatus,
                Status = $"Người nhận đã thanh toán phụ phí cho đơn hàng {shipment.TrackingCode}",
                EventTime = CoreHelper.SystemTimeNow,
                UpdatedBy = userId,
            });
            await _backgroundJobService.CancelScheduleApplySurchargeJob(shipment.Id);
        }
        else if (shipment.ShipmentStatus == ShipmentStatusEnum.DeliveredPartially)
        {
            shipment.ShipmentStatus = ShipmentStatusEnum.CompletedWithCompensation;
            shipment.CompensatedAt = CoreHelper.SystemTimeNow;
            _shipmentTrackingRepository.Add(new ShipmentTracking
            {
                ShipmentId = shipment.Id,
                CurrentShipmentStatus = shipment.ShipmentStatus,
                Status = $"Nhân viên đã bồi thường cho đơn hàng {shipment.TrackingCode}",
                EventTime = CoreHelper.SystemTimeNow,
                UpdatedBy = userId,
            });
        }
        else if (shipment.ShipmentStatus == ShipmentStatusEnum.ToCompensate)
        {
            shipment.ShipmentStatus = ShipmentStatusEnum.Compensated;
            shipment.CompensatedAt = CoreHelper.SystemTimeNow;
            _shipmentTrackingRepository.Add(new ShipmentTracking
            {
                ShipmentId = shipment.Id,
                CurrentShipmentStatus = shipment.ShipmentStatus,
                Status = $"Nhân viên đã bồi thường cho đơn hàng {shipment.TrackingCode}",
                EventTime = CoreHelper.SystemTimeNow,
                UpdatedBy = userId,
            });
        }
        else
        {
            throw new AppException(
                ErrorCode.BadRequest,
                "Invalid shipment status for payment handling.",
                StatusCodes.Status400BadRequest);
        }

        _shipmentRepository.Update(shipment);
    }

    private async Task<Transaction> HandleCreateTransaction(Shipment shipment, TransactionRequest request)
    {
        _logger.Information("Handling creation of transaction for shipment: {shipmentId}", shipment.Id);

        var transaction = _mapper.MapToTransactionEntity(request);
        transaction.ShipmentId = shipment.Id;
        transaction.PaidById = JwtClaimUltils.GetUserId(_httpContextAccessor);
        transaction.PaymentMethod = PaymentMethodEnum.VnPay;
        transaction.PaymentStatus = PaymentStatusEnum.Pending;
        transaction.TransactionType = request.TransactionType.Value;
        transaction.Description = request.ToJsonString();

        if (request.TransactionType == TransactionTypeEnum.ShipmentCost)
        {
            transaction.PaymentAmount = shipment.TotalCostVnd;
        }
        else if (request.TransactionType == TransactionTypeEnum.Refund)
        {
            transaction.PaymentAmount = shipment.TotalRefundedFeeVnd.Value;
        }
        else if (request.TransactionType == TransactionTypeEnum.Surcharge)
        {
            transaction.PaymentAmount = shipment.TotalSurchargeFeeVnd.Value;
        }
        else if (request.TransactionType == TransactionTypeEnum.Compensation)
        {
            transaction.PaymentAmount = shipment.TotalCompensationFeeVnd.Value;
        }
        else
        {
            throw new AppException(
            ErrorCode.BadRequest,
            "Invalid transaction type for creating a transaction.",
            StatusCodes.Status400BadRequest);
        }

        await _backgroundJobService.ScheduleCancelTransactionJob(transaction.Id);
        return transaction;
    }*/

    private async Task HandlePaymentForShipment(Shipment shipment, string userId)
    {
        _logger.Information("Handling payment for shipment: {shipmentId}", shipment.Id);

        switch (shipment.ShipmentStatus)
        {
            case ShipmentStatusEnum.AwaitingPayment:
                if (!shipment.IsReturnShipment)
                {
                    shipment.ShipmentStatus = ShipmentStatusEnum.AwaitingDropOff;
                    shipment.PaidAt = CoreHelper.SystemTimeNow;
                    _shipmentTrackingRepository.Add(new ShipmentTracking
                    {
                        ShipmentId = shipment.Id,
                        CurrentShipmentStatus = shipment.ShipmentStatus,
                        Status = $"Người gửi đã thanh toán cho đơn hàng {shipment.TrackingCode} qua VnPay",
                        EventTime = CoreHelper.SystemTimeNow,
                        UpdatedBy = userId,
                    });

                    await _backgroundJobService.CancelScheduledUnpaidJob(shipment.Id);
                    await _backgroundJobService.ScheduleUpdateNoDropOffJob(shipment.Id, shipment.ScheduledDateTime.Value);
                }
                else
                {
                    shipment.ShipmentStatus = ShipmentStatusEnum.PickedUp;
                    shipment.PaidAt = CoreHelper.SystemTimeNow;

                    _shipmentTrackingRepository.Add(new ShipmentTracking
                    {
                        ShipmentId = shipment.Id,
                        CurrentShipmentStatus = shipment.ShipmentStatus,
                        Status = $"Người gửi đã thanh toán cho đơn hàng {shipment.TrackingCode} qua VnPay",
                        EventTime = CoreHelper.SystemTimeNow,
                        UpdatedBy = userId,
                    });
                }
                break;

            case ShipmentStatusEnum.AwaitingRefund:
                shipment.ShipmentStatus = ShipmentStatusEnum.Refunded;
                shipment.RefundedAt = CoreHelper.SystemTimeNow;
                _shipmentTrackingRepository.Add(new ShipmentTracking
                {
                    ShipmentId = shipment.Id,
                    CurrentShipmentStatus = shipment.ShipmentStatus,
                    Status = $"Nhân viên đã hoàn tiền cho đơn hàng {shipment.TrackingCode}",
                    EventTime = CoreHelper.SystemTimeNow,
                    UpdatedBy = userId,
                });
                break;

            case ShipmentStatusEnum.ApplyingSurcharge:
                shipment.ShipmentStatus = ShipmentStatusEnum.AwaitingDelivery;
                _shipmentTrackingRepository.Add(new ShipmentTracking
                {
                    ShipmentId = shipment.Id,
                    CurrentShipmentStatus = shipment.ShipmentStatus,
                    Status = $"Người nhận đã thanh toán phụ phí cho đơn hàng {shipment.TrackingCode}",
                    EventTime = CoreHelper.SystemTimeNow,
                    UpdatedBy = userId,
                });
                await _backgroundJobService.CancelScheduleApplySurchargeJob(shipment.Id);
                break;

            case ShipmentStatusEnum.DeliveredPartially:
                shipment.ShipmentStatus = ShipmentStatusEnum.CompletedWithCompensation;
                shipment.CompensatedAt = CoreHelper.SystemTimeNow;
                _shipmentTrackingRepository.Add(new ShipmentTracking
                {
                    ShipmentId = shipment.Id,
                    CurrentShipmentStatus = shipment.ShipmentStatus,
                    Status = $"Nhân viên đã bồi thường cho đơn hàng {shipment.TrackingCode}",
                    EventTime = CoreHelper.SystemTimeNow,
                    UpdatedBy = userId,
                });
                break;

            case ShipmentStatusEnum.ToCompensate:
                shipment.ShipmentStatus = ShipmentStatusEnum.Compensated;
                shipment.CompensatedAt = CoreHelper.SystemTimeNow;
                _shipmentTrackingRepository.Add(new ShipmentTracking
                {
                    ShipmentId = shipment.Id,
                    CurrentShipmentStatus = shipment.ShipmentStatus,
                    Status = $"Nhân viên đã bồi thường cho đơn hàng {shipment.TrackingCode}",
                    EventTime = CoreHelper.SystemTimeNow,
                    UpdatedBy = userId,
                });
                break;

            default:
                throw new AppException(
                    ErrorCode.BadRequest,
                    "Trạng thái đơn hàng không hợp lệ để xử lý thanh toán.",
                    StatusCodes.Status400BadRequest);
        }

        _shipmentRepository.Update(shipment);
    }

    private async Task<Transaction> HandleCreateTransaction(Shipment shipment, TransactionRequest request)
    {
        _logger.Information("Handling creation of transaction for shipment: {shipmentId}", shipment.Id);

        var transaction = _mapper.MapToTransactionEntity(request);
        transaction.ShipmentId = shipment.Id;
        transaction.PaidById = JwtClaimUltils.GetUserId(_httpContextAccessor);
        transaction.PaymentMethod = PaymentMethodEnum.VnPay;
        transaction.PaymentStatus = PaymentStatusEnum.Pending;
        transaction.TransactionType = request.TransactionType.Value;
        transaction.Description = request.ToJsonString();

        transaction.PaymentAmount = request.TransactionType switch
        {
            TransactionTypeEnum.ShipmentCost => shipment.TotalCostVnd,
            TransactionTypeEnum.Refund => shipment.TotalRefundedFeeVnd.Value,
            TransactionTypeEnum.Surcharge => shipment.TotalSurchargeFeeVnd.Value,
            TransactionTypeEnum.Compensation => shipment.TotalCompensationFeeVnd.Value,
            _ => throw new AppException(
                ErrorCode.BadRequest,
                "Loại giao dịch không hợp lệ để tạo giao dịch.",
                StatusCodes.Status400BadRequest)
        };

        await _backgroundJobService.ScheduleCancelTransactionJob(transaction.Id, shipment.PaymentDealine.Value);
        return transaction;
    }

    // cancel transaction by transactionId
    public async Task CancelTransactionAsync(string transactionId)
    {
        _logger.Information("Cancelling transaction with ID: {transactionId}", transactionId);
        var transaction = await _transactionRepository.GetSingleAsync(
                       x => x.Id == transactionId && x.PaymentStatus == PaymentStatusEnum.Pending);

        if (transaction == null)
        {
            throw new AppException(ErrorCode.BadRequest,
                               "Transaction not found or already processed",
                                              StatusCodes.Status404NotFound);
        }

        transaction.PaymentStatus = PaymentStatusEnum.Cancelled;
        _transactionRepository.Update(transaction);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
    }

    public async Task<List<VietQrBankDetail>> GetBanksFromVietQr()
    {
        _logger.Information("Fetching banks from VietQr");

        // get from cache
        var cacheKey = "VietQrBanks";
        if (_memoryCache.TryGetValue(cacheKey, out List<VietQrBankDetail> cachedBanks))
        {
            _logger.Information("Returning cached banks from VietQr");
            return cachedBanks;
        }

        HttpClient client = new HttpClient();
        var baseUrl = " https://api.vietqr.io/v2/banks";
        var response = await client.GetAsync(baseUrl);

        if (!response.IsSuccessStatusCode)
        {
            throw new AppException(ErrorCode.BadRequest,
            "Failed to fetch banks from VietQr",
            StatusCodes.Status503ServiceUnavailable);
        }

        var responseContent = await response.Content.ReadAsStringAsync();

        // convert to VietQrBankResponse
        var banks = JsonConvert.DeserializeObject<VietQrBankResponse>(responseContent).Data;
        _memoryCache.Set(cacheKey, banks, TimeSpan.FromHours(12));
        return banks;
    }

    public async Task<string> GenerateBankQrLink (int bankId, string accountNo, decimal? amount)
    {
        _logger.Information("Generating bank QR link for bankId: {bankId}, AccountNo: {AccountNo}", bankId, accountNo);

        var bankShortName = GetBanksFromVietQr().Result.FirstOrDefault(b => b.Id == bankId).ShortName;
        var qrLink = $"https://img.vietqr.io/image/{bankShortName}-{accountNo}-compact2.png";

        if (amount.HasValue && amount > 0)
        {
            var roundedAmount = Math.Round(amount.Value, 0);
            qrLink = $"{qrLink}?amount={roundedAmount}";
        }
        return qrLink;
    }
}