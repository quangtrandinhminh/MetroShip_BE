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
using MetroShip.Utility.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.Services;

public class ParcelService(IServiceProvider serviceProvider) : IParcelService
{
    private readonly IBaseRepository<Parcel> _parcelRepository = serviceProvider.GetRequiredService<IBaseRepository<Parcel>>();
    private readonly ShipmentRepository _shipmentRepository = serviceProvider.GetRequiredService<ShipmentRepository>();
    private readonly IMapperlyMapper _mapper = serviceProvider.GetRequiredService<IMapperlyMapper>();
    private readonly ILogger<ParcelService> _logger = serviceProvider.GetRequiredService<ILogger<ParcelService>>();
    private readonly IUnitOfWork _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
    private readonly ISystemConfigRepository _systemConfigRepository = serviceProvider.GetRequiredService<ISystemConfigRepository>();
    private readonly IHttpContextAccessor _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    private static readonly List<CreateParcelResponse> _parcelCache = new();

    public CreateParcelResponse CalculateParcelInfo(ParcelRequest request)
    {
        decimal volume = request.LengthCm * request.WidthCm * request.HeightCm;
        decimal divisor = request.IsBulk ? 5000m : 6000m;
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
    
    public async Task<PaginatedListResponse<CreateParcelResponse>> GetAllParcels(PaginatedListRequest request)
    {
        // Lấy customerId từ JWT claims
        var customerId = JwtClaimUltils.GetUserId(_httpContextAccessor);

        // Ghi log yêu cầu
        _logger.LogInformation("Get all parcels with request: {@request} for customer {@customerId}", request, customerId);

        // Truy vấn các parcel liên quan đến customerId thông qua Shipment
        var query = _parcelRepository.GetAll()
    .Join(_shipmentRepository.GetAll(),
          parcel => parcel.ShipmentId,
          shipment => shipment.Id,
          (parcel, shipment) => new { Parcel = parcel, Shipment = shipment })
    .Where(joinResult => joinResult.Shipment.SenderId == customerId)
    .Select(joinResult => joinResult.Parcel)
    .OrderByDescending(p => p.CreatedAt); // Assuming 'CreatedAt' is the correct property

        // Lấy tổng số lượng parcel
        var totalCount = await query.CountAsync();

        // Áp dụng phân trang
        var parcels = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var paginatedParcels = new PaginatedList<Parcel>(parcels, totalCount, request.PageNumber, request.PageSize);

        // Map the PaginatedList<Parcel> to PaginatedListResponse<CreateParcelResponse>
        var parcelResponses = _mapper.MapToParcelPaginatedList(paginatedParcels);

        // Trả về kết quả phân trang
        return parcelResponses;
    }
}


