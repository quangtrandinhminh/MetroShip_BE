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

namespace MetroShip.Service.Services;

public class ParcelService(IServiceProvider serviceProvider) : IParcelService
{
    private readonly IBaseRepository<Parcel> _parcelRepository = serviceProvider.GetRequiredService<IBaseRepository<Parcel>>();
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
                x => x.ParcelCategory, x => x.ParcelTrackings
        );

        var parcelListResponse = _mapper.MapToParcelPaginatedList(parcels);
        return parcelListResponse;
    }

    public async Task ConfirmParcelAsync(Guid parcelId, bool isRejected)
    {
        var parcel = await _parcelRepository.GetAll()
            .Include(p => p.ParcelTrackings)
            .Include(p => p.Shipment)
                .ThenInclude(s => s.Parcels)
                    .ThenInclude(p => p.ParcelTrackings)
            .FirstOrDefaultAsync(p => p.Id == parcelId.ToString());

        if (parcel == null)
            throw new AppException(ErrorCode.NotFound, "Parcel not found", StatusCodes.Status404NotFound);

        var shipment = parcel.Shipment;

        // Ghi lại trạng thái parcel
        var newStatus = isRejected
            ? ParcelStatusEnum.Rejected.ToString()
            : ParcelStatusEnum.AwaitingPayment.ToString();

        parcel.ParcelTrackings.Add(new ParcelTracking
        {
            ParcelId = parcel.Id,
            Status = newStatus,
        });

        if (isRejected)
        {
            shipment.TotalCostVnd -= parcel.PriceVnd;
            if (shipment.TotalCostVnd < 0) shipment.TotalCostVnd = 0;
        }

        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);

        // Kiểm tra trạng thái của tất cả parcel trong shipment
        var latestParcelStatuses = shipment.Parcels
            .Select(p => p.ParcelTrackings.OrderByDescending(pt => pt.CreatedAt).First().Status)
            .ToList();

        var allRejected = latestParcelStatuses.All(s => s == ParcelStatusEnum.Rejected.ToString());
        var allAccepted = latestParcelStatuses.All(s => s == ParcelStatusEnum.AwaitingPayment.ToString());
        var anyRejected = latestParcelStatuses.Any(s => s == ParcelStatusEnum.Rejected.ToString());

        if (allAccepted)
        {
            shipment.ShipmentStatus = ShipmentStatusEnum.Accepted;
            _logger.LogInformation("Shipment {ShipmentId} transitioned to Accepted via trigger {Trigger}", shipment.Id, ShipmentTrigger.StaffConfirmAllParcels);
        }
        else if (allRejected)
        {
            shipment.ShipmentStatus = ShipmentStatusEnum.Rejected;
            _logger.LogInformation("Shipment {ShipmentId} transitioned to Rejected via trigger {Trigger}", shipment.Id, ShipmentTrigger.StaffRejectAllParcels);
        }
        else if (anyRejected)
        {
            shipment.ShipmentStatus = ShipmentStatusEnum.PartiallyConfirmed;
            _logger.LogInformation("Shipment {ShipmentId} transitioned to PartiallyConfirmed via trigger {Trigger}", shipment.Id, ShipmentTrigger.StaffConfirmSomeParcels);
        }

        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
    }
}


