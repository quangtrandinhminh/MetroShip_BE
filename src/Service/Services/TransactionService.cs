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
using Serilog;
using System.Linq.Expressions;
using System.Net;

namespace MetroShip.Service.Services;

public class TransactionService : ITransactionService
{
    private readonly IVnPayService _vnPayService;
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IMapperlyMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBaseRepository<Parcel> _parcelRepository;
    private readonly TransactionValidator _transactionValidator;

    private readonly IBaseRepository<Transaction> _transaction;

    public TransactionService(
        IShipmentRepository shipmentRepository,
        IMapperlyMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        ILogger logger,
        IUnitOfWork unitOfWork,
        IVnPayService vnPayService,
        IBaseRepository<Parcel> parcelRepository,
        IBaseRepository<Transaction> transactionRepository) 
    {
        _vnPayService = vnPayService;
        _shipmentRepository = shipmentRepository;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _transactionValidator = new TransactionValidator();
        _unitOfWork = unitOfWork;
        _parcelRepository = parcelRepository;
        _transaction = transactionRepository;
    }

    public async Task<string> CreateVnPayTransaction(TransactionRequest request)
    {
        var customerId = JwtClaimUltils.GetUserId(_httpContextAccessor);
        _logger.Information("Creating payment link for: {shipment} by {CustomerId}",
            request.ShipmentId, customerId);
        _transactionValidator.ValidateTransactionRequest(request);
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

        if (shipment.Transactions.FirstOrDefault(t => t.TransactionType == TransactionTypeEnum.ShipmentCost
        && t.PaymentStatus == PaymentStatusEnum.Pending) is null)
        {
            var transaction = _mapper.MapToTransactionEntity(request);
            transaction.PaidById = customerId;
            transaction.PaymentMethod = PaymentMethodEnum.VnPay;
            transaction.PaymentStatus = PaymentStatusEnum.Pending;
            transaction.PaymentAmount = shipment.TotalCostVnd;
            transaction.TransactionType = TransactionTypeEnum.ShipmentCost;
            transaction.Description = JsonConvert.SerializeObject(request);
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
                       x => x.Id == vnPaymentResponse.OrderId || x.TrackingCode == vnPaymentResponse.OrderId, 
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

        var paginatedTransactions = await _transaction.GetAllPaginatedQueryable(
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            predicate: predicate,
            orderBy: t => t.PaymentDate // Default sort
        );

        return _mapper.MapToTransactionPaginatedList(paginatedTransactions);
    }

    /*private async Task HandleShipmentParcelTransaction(
               Shipment shipment)
    {
        foreach (var parcel in shipment.Parcels)
        {
            if (parcel.ParcelStatus == ParcelStatusEnum.AwaitingPayment)
            {
                parcel.ParcelStatus = ParcelStatusEnum.AwaitingDropOff;
                _parcelRepository.Update(parcel);
            }
        }
    }*/
}