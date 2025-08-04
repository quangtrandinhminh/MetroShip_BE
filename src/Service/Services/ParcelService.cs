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
using Serilog;
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
    private readonly IShipmentRepository _shipmentRepository = serviceProvider.GetRequiredService<IShipmentRepository>();   
    private readonly IMapperlyMapper _mapper = serviceProvider.GetRequiredService<IMapperlyMapper>();
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
    private readonly IUnitOfWork _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
    private readonly ISystemConfigRepository _systemConfigRepository = serviceProvider.GetRequiredService<ISystemConfigRepository>();
    private readonly IHttpContextAccessor _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    private readonly IMapperlyMapper _mapperlyMapper = serviceProvider.GetRequiredService<IMapperlyMapper>();
    private readonly IParcelRepository _parcelRepository = serviceProvider.GetRequiredService<IParcelRepository>();
    private readonly IStationRepository _stationRepository = serviceProvider.GetRequiredService<IStationRepository>();
    private readonly IBaseRepository<ParcelMedia> _parcelMediaRepository = serviceProvider.GetRequiredService<IBaseRepository<ParcelMedia>>();
    private readonly IBaseRepository<ParcelTracking> _parcelTrackingRepository = serviceProvider.GetRequiredService<IBaseRepository<ParcelTracking>>();
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
        _logger.Information("Parcel info valid from {MinBookDate} to {MaxBookDate}, expires at {ExpirationTime}", minBookDate, maxBookDate);

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
        _logger.Information("Get all parcels with request: {@request} for customer {@customerId}", request, customerId);

        // Truy vấn các parcel liên quan đến customerId thông qua Shipment
        var parcels = await _parcelRepository.GetAllPaginatedQueryable(
                request.PageNumber, request.PageSize,
                expression,
                orderBy: x => x.CreatedAt,
                isAscending: true,
                includeProperties: x => x.ParcelCategory);

        var parcelListResponse = _mapper.MapToParcelPaginatedList(parcels);
        return parcelListResponse;
    }

    public async Task<ParcelResponse?> GetParcelByParcelCodeAsync(string parcelCode)
    {
        _logger.Information("Get parcel by ID: {ParcelId}", parcelCode);

        // Truy vấn parcel theo ID và kiểm tra quyền truy cập
        var parcel = await _parcelRepository.GetSingleAsync(
                       p => p.ParcelCode == parcelCode && p.DeletedAt == null, false,
                                   p => p.ParcelCategory,
                                   p => p.ParcelMedias,
                                   p =>  p.ParcelTrackings);

        if (parcel == null)
        {
            throw new AppException(ErrorCode.NotFound, "Parcel not found", StatusCodes.Status400BadRequest);
        }

        return _mapper.MapToParcelResponse(parcel);
    }

    public async Task ConfirmParcelAsync (ParcelConfirmRequest request)
    {
        _logger.Information("Confirming parcel with code: {ParcelCode}", request.ParcelCode);
        var parcel = await _parcelRepository.GetAll()
            .Where(p => p.DeletedAt == null && p.ParcelCode == request.ParcelCode)
            .Include(p => p.Shipment)
            .Include(p => p.ParcelTrackings)
            .Include(p => p.ParcelMedias)
            .FirstOrDefaultAsync();

        if (parcel == null)
            throw new AppException(ErrorCode.NotFound, "Parcel not found", StatusCodes.Status404NotFound);

        // Only allow confirmation for parcels in AwaitingConfirmation status
        /*if (parcel.ParcelStatus != ParcelStatusEnum.AwaitingConfirmation)
            throw new AppException(ErrorCode.BadRequest, "Parcel is not in AwaitingConfirmation status",
                StatusCodes.Status400BadRequest);

        parcel.ParcelStatus = ParcelStatusEnum.AwaitingPayment;*/

        var shipment = parcel.Shipment;
        if (shipment.ShipmentStatus != ShipmentStatusEnum.AwaitingDropOff)
        {
            throw new AppException(
                ErrorCode.BadRequest,
                ResponseMessageShipment.SHIPMENT_ALREADY_CONFIRMED,
                StatusCodes.Status400BadRequest);
        }

        // Check if the parcel is already confirmed
        if (parcel.ParcelTrackings.Any(pt => pt.TrackingForShipmentStatus == ShipmentStatusEnum.PickedUp))
        {
            throw new AppException(
            ErrorCode.BadRequest,
            "Parcel has already been confirmed for pickup.",
            StatusCodes.Status400BadRequest);
        }

        var stationName = await _stationRepository.GetStationNameByIdAsync(shipment.DepartureStationId);
        var parcelTracking = new ParcelTracking
        {
            ParcelId = parcel.Id,
            Status = $"Kiện hàng đã được nhận tại Ga {stationName}",
            CurrentShipmentStatus = shipment.ShipmentStatus,
            TrackingForShipmentStatus = ShipmentStatusEnum.PickedUp,
            StationId = shipment.DepartureStationId,
            EventTime = CoreHelper.SystemTimeNow,
            UpdatedBy = JwtClaimUltils.GetUserId(_httpContextAccessor),
        };
        _parcelTrackingRepository.Add(parcelTracking);

        foreach (var media in request.ConfirmedMedias)
        {
            var mediaEntity = _mapperlyMapper.MapToParcelMediaEntity(media);
            mediaEntity.ParcelId = parcel.Id;
            mediaEntity.BusinessMediaType = BusinessMediaTypeEnum.Pickup;
            mediaEntity.MediaType = DataHelper.IsImage(mediaEntity.MediaUrl);
            _parcelMediaRepository.Add(mediaEntity);
        }

        // Check if the shipment is ready for the next status: all parcels confirmed for pickup
        if (IsReadyForNextShipmentStatus(parcel.ShipmentId, ShipmentStatusEnum.PickedUp))
        {
            // Update shipment status and timestamps
            shipment.ShipmentStatus = ShipmentStatusEnum.PickedUp;
            shipment.PickedUpBy = JwtClaimUltils.GetUserId(_httpContextAccessor);
            shipment.PickedUpAt = CoreHelper.SystemTimeNow;
            _shipmentRepository.Update(shipment);
        }    
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);

        // Kiểm tra trạng thái của tất cả parcel trong shipment
        //await HandleShipmentStatusByConfirmation(parcel.ShipmentId);
    }

    /*public async Task RejectParcelAsync(ParcelRejectRequest request)
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

        _shipmentRepository.Update(parcel.Shipment);
        _parcelRepository.Update(parcel);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);

        // Kiểm tra trạng thái toàn bộ parcel trong shipment
        await HandleShipmentStatusByConfirmation(parcel.ShipmentId);
    }*/

    /*private async Task HandleShipmentStatusByConfirmation (string shipmentId)
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
                _logger.Information("Shipment {ShipmentId} rejected.", shipment.Id);
            }
            else if (statuses.All(s => s == ParcelStatusEnum.AwaitingPayment))
            {
                shipment.ShipmentStatus = ShipmentStatusEnum.Accepted;
                shipment.ApprovedAt = CoreHelper.SystemTimeNow;
                _logger.Information("Shipment {ShipmentId} accepted.", shipment.Id);
            }
            /*else
            {
                shipment.ShipmentStatus = ShipmentStatusEnum.PartiallyConfirmed;
                shipment.ApprovedAt = CoreHelper.SystemTimeNow;
                _logger.LogInformation("Shipment {ShipmentId} partially confirmed.", shipment.Id);
            }#1#

            _shipmentRepository.Update(shipment);
            await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
        }
    }*/

    private bool IsReadyForNextShipmentStatus(string shipmentId, ShipmentStatusEnum nextShipmentStatus)
    {
        _logger.Information("Checking if shipment {ShipmentId} is ready for status {NextStatus}",
            shipmentId, nextShipmentStatus);

        // Count parcels in the shipment
        var parcelCount = _shipmentRepository.GetAll()
            .Where(x => x.Id == shipmentId && x.DeletedAt == null)
            .SelectMany(x => x.Parcels)
            .Count();

        // count parcelTracking have TrackingForShipmentStatus == nextShipmentStatus
        var parcelTrackingCount = _parcelRepository.GetAll()
            .Where(x => x.ShipmentId == shipmentId && x.DeletedAt == null)
            .SelectMany(p => p.ParcelTrackings)
            .Count(pt => pt.TrackingForShipmentStatus == nextShipmentStatus) + 1; // +1 for the current parcel request

        _logger.Information("Parcel count: {ParcelCount}, Tracking count for status {NextStatus}: {TrackingCount}",
            parcelCount, nextShipmentStatus, parcelTrackingCount);

        // Check if all parcels have the next status
        if (parcelCount == parcelTrackingCount)
        {
            _logger.Information("Shipment {ShipmentId} is ready for status {NextStatus}",
                shipmentId, nextShipmentStatus);
            return true;
        }

        _logger.Information("Shipment {ShipmentId} is NOT ready for status {NextStatus}",
            shipmentId, nextShipmentStatus);
        return false;
    }
}


