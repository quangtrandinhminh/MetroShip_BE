using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
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
using Serilog;
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
    private readonly TransactionValidator _transactionValidator;

    public TransactionService(
        IShipmentRepository shipmentRepository,
        IMapperlyMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        ILogger logger,
        IUnitOfWork unitOfWork,
        IVnPayService vnPayService)
    {
        _vnPayService = vnPayService;
        _shipmentRepository = shipmentRepository;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _transactionValidator = new TransactionValidator();
        _unitOfWork = unitOfWork;
    }

    public async Task<string> CreateVnPayTransaction(TransactionRequest request)
    {
        var customerId  = JwtClaimUltils.GetUserId(_httpContextAccessor);
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
        if (shipment.ShipmentStatus != ShipmentStatusEnum.Accepted
            || shipment.ShipmentStatus != ShipmentStatusEnum.PartiallyConfirmed)
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
            shipment.Transactions.Add(transaction);
            _shipmentRepository.Update(shipment);
            await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
        }
        
        var paymentUrl = await _vnPayService.CreatePaymentUrl(
            shipment.TrackingCode, shipment.TotalCostVnd);
        return paymentUrl;
    }

    public async Task ExecuteVnPayPayment(VnPayCallbackModel model)
    {
        _logger.Information("Executing VnPay payment with context: {context}", model);
        var vnPaymentResponse = await _vnPayService.PaymentExecute(model);
        if (vnPaymentResponse == null)
        {
            throw new AppException("Invalid payment response");
        }

        var shipment = await _shipmentRepository.GetSingleAsync(
                       x => x.Id == vnPaymentResponse.OrderId || x.TrackingCode == vnPaymentResponse.OrderId,
                       includeProperties: x => x.Transactions
                       );

        if (shipment == null)
        {
            throw new AppException(
                ErrorCode.BadRequest,
                ResponseMessageShipment.SHIPMENT_NOT_FOUND,
                StatusCodes.Status400BadRequest);
        }

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

        // Update the shipment status based on the payment response
        if (vnPaymentResponse.VnPayResponseCode == "00")
        {
            shipment.ShipmentStatus = ShipmentStatusEnum.Paid;
            transaction.PaymentStatus = PaymentStatusEnum.Paid;
            transaction.PaymentTrackingId = vnPaymentResponse.TransactionId;
            transaction.PaymentTime = DateTimeOffset.ParseExact(
                vnPaymentResponse.PaymentTime,
                "yyyyMMddHHmmss",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeUniversal
            ).ToUniversalTime();
            transaction.PaymentCurrency = "VND";
            transaction.PaymentDate = CoreHelper.SystemTimeNow;
        }

        _shipmentRepository.Update(shipment);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
    }
}