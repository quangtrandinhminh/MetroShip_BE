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
using Newtonsoft.Json;
using Quartz;
using Serilog;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using MetroShip.Repository.Repositories;
using MetroShip.Service.Jobs;

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
    private readonly ITransactionRepository _transactionRepository = serviceProvider.GetRequiredService<ITransactionRepository>();
    private readonly IBaseRepository<ShipmentTracking> _shipmentTrackingRepository = serviceProvider.GetRequiredService<IBaseRepository<ShipmentTracking>>();
    private readonly ISchedulerFactory _schedulerFactory = serviceProvider.GetRequiredService<ISchedulerFactory>();
    private readonly IPricingService _pricingService = serviceProvider.GetRequiredService<IPricingService>();

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
                "Shipment not found",
                StatusCodes.Status404NotFound);
        }
        
        // Check if the shipment is in a valid state for payment
        if (shipment.ShipmentStatus != ShipmentStatusEnum.AwaitingPayment 
            && shipment.ShipmentStatus != ShipmentStatusEnum.AwaitingRefund 
            && shipment.ShipmentStatus != ShipmentStatusEnum.ApplyingSurcharge
            && shipment.ShipmentStatus != ShipmentStatusEnum.CompletedWithCompensation
            && shipment.ShipmentStatus != ShipmentStatusEnum.ToCompensate
            )
        {
            throw new AppException(
                ErrorCode.BadRequest,
                "Shipment status must be AwaitingPayment, AwaitingRefund, or ApplyingSurcharge to create a payment link.",
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
            shipment.TrackingCode, shipment.TotalCostVnd);
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

        var shipment = await _shipmentRepository.GetSingleAsync(
                       x => x.Id == vnPaymentResponse.OrderId 
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
                                                       && x.PaymentStatus == PaymentStatusEnum.Pending);
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
        return response;
    }

    public async Task<PaginatedListResponse<TransactionResponse>> GetAllAsync(PaymentStatusEnum? status, PaginatedListRequest request)
    {
        var customerId = JwtClaimUltils.GetUserId(_httpContextAccessor);
        var userRole = JwtClaimUltils.GetUserRole(_httpContextAccessor);
        _logger.Information("Fetching transactions. PaymentStatus: {status}", status);

        Expression<Func<Transaction, bool>> predicate = t => t.DeletedAt == null;

        if (status.HasValue)
        {
            predicate = predicate.And(t => t.PaymentStatus == status.Value);
        }

        if (!string.IsNullOrEmpty(customerId) && userRole.Contains(UserRoleEnum.Customer.ToString()))
        {
            predicate = predicate.And(t => t.PaidById == customerId);
        }

        var paginatedTransactions = await _transactionRepository.GetAllPaginatedQueryable(
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            predicate: predicate,
            orderBy: t => t.PaymentDate // Default sort
        );

        return _mapper.MapToTransactionPaginatedList(paginatedTransactions);
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

    // cancel ScheduleUnpaidJob
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

    // cancel apply surcharge job
    private async Task CancelApplySurchargeJob(string shipmentId)
    {
        _logger.Information("Canceling apply surcharge job for shipment ID: {@shipmentId}", shipmentId);
        var jobKey = new JobKey($"ApplySurchargeJob-{shipmentId}");
        if (await _schedulerFactory.GetScheduler().Result.CheckExists(jobKey))
        {
            await _schedulerFactory.GetScheduler().Result.DeleteJob(jobKey);
            _logger.Information("Apply surcharge job canceled for shipment ID: {@shipmentId}", shipmentId);
        }
        else
        {
            _logger.Warning("No apply surcharge job found for shipment ID: {@shipmentId}", shipmentId);
        }
    }

    private async Task HandlePaymentForShipment(Shipment shipment, string userId)
    {
        _logger.Information("Handling payment for shipment: {shipmentId}", shipment.Id);

        if (shipment.ShipmentStatus == ShipmentStatusEnum.AwaitingPayment)
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

            await CancelScheduledUnpaidJob(shipment.Id);
            await ScheduleUpdateNoDropOffJob(shipment.Id, shipment.ScheduledDateTime.Value);
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

            await CancelApplySurchargeJob(shipment.Id);
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
            await CancelApplySurchargeJob(shipment.Id);
        }
        else if (shipment.ShipmentStatus == ShipmentStatusEnum.CompletedWithCompensation
            || shipment.ShipmentStatus == ShipmentStatusEnum.ToCompensate)
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

    private async Task ScheduleUpdateNoDropOffJob(string shipmentId, DateTimeOffset scheduledDateTime)
    {
        _logger.Information("Scheduling job to update shipment status to NoDropOff for ID: {@shipmentId}", shipmentId);
        var jobData = new JobDataMap
        {
            { "NoDropOff-for-shipmentId", shipmentId }
        };

        // Schedule the job to run after 15 minutes
        var jobDetail = JobBuilder.Create<UpdateShipmentToNoDropOff>()
            .WithIdentity($"UpdateShipmentToNoDropOff-{shipmentId}")
            .UsingJobData(jobData)
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity($"Trigger-UpdateShipmentToNoDropOff-{shipmentId}")
            .StartAt(scheduledDateTime.AddMinutes(5))
            //.StartAt(DateTimeOffset.UtcNow.AddSeconds(5))
            .Build();

        await _schedulerFactory.GetScheduler().Result.ScheduleJob(jobDetail, trigger);
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

        return transaction;
    }
}