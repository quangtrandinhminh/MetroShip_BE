using MetroShip.Repository.Base;
using MetroShip.Repository.Extensions;
using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using MetroShip.Repository.Repositories;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Parcel;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Mapper;
using MetroShip.Service.Utils;
using MetroShip.Utility.Config;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Enums;
using MetroShip.Utility.Exceptions;
using MetroShip.Utility.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static MetroShip.Utility.Constants.WebApiEndpoint;

namespace MetroShip.Service.Services;

public class ParcelService(IServiceProvider serviceProvider) : IParcelService
{
    private readonly IBaseRepository<Parcel> _parcelRepository = serviceProvider.GetRequiredService<IBaseRepository<Parcel>>();
    private readonly IShipmentRepository _shipmentRepository = serviceProvider.GetRequiredService<IShipmentRepository>();   
    private readonly IBaseRepository<ParcelTracking> _parceltrackingRepository = serviceProvider.GetRequiredService<IBaseRepository<ParcelTracking>>();
    private readonly IMapperlyMapper _mapper = serviceProvider.GetRequiredService<IMapperlyMapper>();
    private readonly ILogger<ParcelService> _logger = serviceProvider.GetRequiredService<ILogger<ParcelService>>();
    private readonly IUnitOfWork _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
    private readonly ISystemConfigRepository _systemConfigRepository = serviceProvider.GetRequiredService<ISystemConfigRepository>();
    private readonly IHttpContextAccessor _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    private static readonly List<CreateParcelResponse> _parcelCache = new();

    public CreateParcelResponse CalculateParcelInfo(ParcelRequest request)
    {
        var isBulk = true;
        decimal volume = request.LengthCm * request.WidthCm * request.HeightCm;
        decimal divisor = isBulk ? 5000m : 6000m;
        decimal volumetricWeight = volume / divisor;
        decimal chargeableWeight = Math.Max(request.WeightKg, volumetricWeight);

        // System config (used internally, not returned)
        var now = CoreHelper.SystemTimeNow;
        var confirmationHour = int.Parse(_systemConfigRepository
            .GetSystemConfigValueByKey(nameof(SystemConfigSetting.CONFIRMATION_HOUR)));

        var paymentRequestHour = int.Parse(_systemConfigRepository
            .GetSystemConfigValueByKey(nameof(SystemConfigSetting.PAYMENT_REQUEST_HOUR)));

        var maxScheduleDay = int.Parse(_systemConfigRepository
            .GetSystemConfigValueByKey(nameof(SystemConfigSetting.MAX_SCHEDULE_SHIPMENT_DAY)));


        // Computed internally
        var minBookDate = now.AddHours(confirmationHour + paymentRequestHour);
        var maxBookDate = now.AddDays(maxScheduleDay);

        // Optionally store these internally in memory, cache, or db — not returned in response
        _logger.LogInformation("Parcel info valid from {MinBookDate} to {MaxBookDate}, expires at {ExpirationTime}", minBookDate, maxBookDate);

        var result = new CreateParcelResponse
        {
            Id = Guid.NewGuid(),
            VolumeCm3 = volume,
            ChargeableWeightKg = chargeableWeight
        };
        _parcelCache.Add(result);
        return result;
    }

    public decimal CalculateShippingCost(ParcelRequest request, double distanceKm, decimal pricePerKm)
    {
        var parcelInfo = CalculateParcelInfo(request);
        var cost = parcelInfo.ChargeableWeightKg * (decimal)distanceKm * pricePerKm;
        return Math.Round(cost, 0);
    }
    
    public async Task<PaginatedListResponse<ParcelResponse>> GetAllParcels(PaginatedListRequest request)
    {
        // Lấy customerId từ JWT claims
        var customerId = JwtClaimUltils.GetUserId(_httpContextAccessor);
        var role = JwtClaimUltils.GetUserRole(_httpContextAccessor);
        Expression<Func<Parcel, bool>> expression = x => x.DeletedAt == null;
        if (!string.IsNullOrEmpty(customerId) && role.Contains(UserRoleEnum.Customer.ToString()))
        {
            expression = expression.And(x => x.Shipment.SenderId == customerId);
        }

        // Ghi log yêu cầu
        _logger.LogInformation("Get all parcels with request: {@request} for customer {@customerId}", request, customerId);

        // Truy vấn các parcel liên quan đến customerId thông qua Shipment
        var parcels = await _parcelRepository.GetAllPaginatedQueryable(
                request.PageNumber, request.PageSize,
                expression,
                x => x.CreatedAt,
                true,
                x => x.ParcelTrackings, x => x.ParcelCategory
        );

        var parcelListResponse = _mapper.MapToParcelPaginatedList(parcels);
        return parcelListResponse;
    }

    public async Task ConfirmParcelAsync(Guid parcelId)
    {
        var parcel = await _parcelRepository.GetAll()
            .Include(p => p.Shipment)
            .FirstOrDefaultAsync(p => p.Id == parcelId.ToString());

        if (parcel == null)
            throw new AppException(ErrorCode.NotFound, "Parcel not found", StatusCodes.Status404NotFound);

        // Only allow confirmation for parcels in AwaitingConfirmation status
        if (parcel.ParcelStatus != ParcelStatusEnum.AwaitingConfirmation)
            throw new AppException(ErrorCode.BadRequest, "Parcel is not in AwaitingConfirmation status",
                StatusCodes.Status400BadRequest);

        parcel.ParcelStatus = ParcelStatusEnum.AwaitingPayment;

        _parceltrackingRepository.Add(new ParcelTracking
        {
            ParcelId = parcel.Id,
            Status = parcel.ParcelStatus.ToString(),
            EventTime = CoreHelper.SystemTimeNow,
        });

        _shipmentRepository.Update(parcel.Shipment);
        _parcelRepository.Update(parcel);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);

        // Kiểm tra trạng thái của tất cả parcel trong shipment
        await HandleShipmentStatusByConfirmation(parcel.ShipmentId);
    }

    public async Task RejectParcelAsync(ParcelRejectRequest request)
    {
        var parcel = await _parcelRepository.GetAll()
            .Include(p => p.Shipment)
            .FirstOrDefaultAsync(p => p.Id == request.ParcelId.ToString());

        if (parcel == null)
            throw new AppException(ErrorCode.NotFound, "Parcel not found", StatusCodes.Status404NotFound);

        if (parcel.ParcelStatus != ParcelStatusEnum.AwaitingConfirmation)
            throw new AppException(ErrorCode.BadRequest, "Parcel is not in AwaitingConfirmation status", StatusCodes.Status400BadRequest);

        parcel.ParcelStatus = ParcelStatusEnum.Rejected;

        // Trừ tiền khỏi shipment nếu từ chối
        parcel.Shipment.TotalCostVnd -= parcel.PriceVnd;
        if (parcel.Shipment.TotalCostVnd < 0) parcel.Shipment.TotalCostVnd = 0;

        // Ghi lại tracking với lý do từ chối ở Note
        _parceltrackingRepository.Update(new ParcelTracking
        {
            ParcelId = parcel.Id,
            Status = parcel.ParcelStatus.ToString(),
            EventTime = CoreHelper.SystemTimeNow,
            Note = request.RejectReason
        });

        _shipmentRepository.Update(parcel.Shipment);
        _parcelRepository.Update(parcel);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);

        // Kiểm tra trạng thái toàn bộ parcel trong shipment
        await HandleShipmentStatusByConfirmation(parcel.ShipmentId);
    }

    private async Task HandleShipmentStatusByConfirmation (string shipmentId)
    {
        var shipment = await _shipmentRepository.GetSingleAsync(
            s => s.Id == shipmentId,
            includeProperties: s => s.Parcels
        );

        var statuses = shipment.Parcels.Select(p => p.ParcelStatus).ToList();

        if (statuses.All(s => s != ParcelStatusEnum.AwaitingConfirmation))
        {
            if (statuses.All(s => s == ParcelStatusEnum.Rejected))
            {
                shipment.ShipmentStatus = ShipmentStatusEnum.Rejected;
                shipment.RejectedAt = CoreHelper.SystemTimeNow;
                _logger.LogInformation("Shipment {ShipmentId} rejected.", shipment.Id);
            }
            else if (statuses.All(s => s == ParcelStatusEnum.AwaitingPayment))
            {
                shipment.ShipmentStatus = ShipmentStatusEnum.Accepted;
                shipment.ApprovedAt = CoreHelper.SystemTimeNow;
                _logger.LogInformation("Shipment {ShipmentId} accepted.", shipment.Id);
            }
            else
            {
                shipment.ShipmentStatus = ShipmentStatusEnum.PartiallyConfirmed;
                shipment.ApprovedAt = CoreHelper.SystemTimeNow;
                _logger.LogInformation("Shipment {ShipmentId} partially confirmed.", shipment.Id);
            }

            _shipmentRepository.Update(shipment);
            await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
        }
    }
}


