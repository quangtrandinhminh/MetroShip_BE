using MetroShip.Repository.Base;
using MetroShip.Repository.Extensions;
using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using MetroShip.Repository.Repositories;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Parcel;
using MetroShip.Service.BusinessModels;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Jobs;
using MetroShip.Service.Mapper;
using MetroShip.Service.Utils;
using MetroShip.Utility.Config;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Enums;
using MetroShip.Utility.Exceptions;
using MetroShip.Utility.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
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
    private readonly IHttpContextAccessor _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    private readonly IMapperlyMapper _mapperlyMapper = serviceProvider.GetRequiredService<IMapperlyMapper>();
    private readonly IParcelRepository _parcelRepository = serviceProvider.GetRequiredService<IParcelRepository>();
    private readonly IStationRepository _stationRepository = serviceProvider.GetRequiredService<IStationRepository>();
    private readonly IBaseRepository<ParcelMedia> _parcelMediaRepository = serviceProvider.GetRequiredService<IBaseRepository<ParcelMedia>>();
    private readonly IBaseRepository<ParcelTracking> _parcelTrackingRepository = serviceProvider.GetRequiredService<IBaseRepository<ParcelTracking>>();
    private readonly ITrainRepository _trainRepository = serviceProvider.GetRequiredService<ITrainRepository>();
    private readonly ISchedulerFactory _schedulerFactory = serviceProvider.GetRequiredService<ISchedulerFactory>();
    private readonly IPricingService _pricingService = serviceProvider.GetRequiredService<IPricingService>();
    private readonly IMemoryCache _parcelCache = serviceProvider.GetRequiredService<IMemoryCache>();
    private readonly IShipmentTrackingRepository _shipmentTrackingRepository = serviceProvider.GetRequiredService<IShipmentTrackingRepository>();
    private readonly IBaseRepository<CategoryInsurance> _categoryInsuranceRepository = serviceProvider.GetRequiredService<IBaseRepository<CategoryInsurance>>();


    /*public CreateParcelResponse CalculateParcelInfo(ParcelRequest request)
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
    }*/

    /*public decimal CalculateShippingCost(ParcelRequest request, double distanceKm, decimal pricePerKm)
    {
        var parcelInfo = CalculateParcelInfo(request);
        var cost = parcelInfo.ChargeableWeightKg * (decimal)distanceKm * pricePerKm;
        return Math.Round(cost, 0);
    }*/

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
                isAscending: true);

        var parcelListResponse = _mapper.MapToParcelPaginatedList(parcels);
        return parcelListResponse;
    }

    public async Task<ParcelResponse?> GetParcelByParcelCodeAsync(string parcelCode)
    {
        _logger.Information("Get parcel by ID: {ParcelId}", parcelCode);

        // Truy vấn parcel theo ID và kiểm tra quyền truy cập
        var parcel = await _parcelRepository.GetAll()
            .Include(p => p.CategoryInsurance)
            .ThenInclude(p => p.ParcelCategory)
            .Include(p => p.CategoryInsurance)
            .ThenInclude(p => p.InsurancePolicy)
            .Include(p => p.ParcelTrackings)
            .Include(p => p.ParcelMedias)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.DeletedAt == null && p.ParcelCode == parcelCode);

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
            .FirstOrDefaultAsync();

        if (parcel == null)
            throw new AppException(ErrorCode.NotFound, "Parcel not found", StatusCodes.Status404NotFound);

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

        // Check if pick up in time range
        var now = CoreHelper.SystemTimeNow;
        if (now < shipment.StartReceiveAt || now > shipment.ScheduledDateTime)
        {
            throw new AppException(
            ErrorCode.BadRequest,
            "Parcel confirmation is outside the allowed pickup time range.",
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
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);

        // Check if the shipment is ready for the next status: all parcels confirmed for pickup
        if (IsReadyForNextShipmentStatus(parcel.ShipmentId, ShipmentStatusEnum.PickedUp))
        {
            // Update shipment status and timestamps
            shipment.ShipmentStatus = ShipmentStatusEnum.PickedUp;
            shipment.CurrentStationId = shipment.DepartureStationId;
            shipment.PickedUpAt = CoreHelper.SystemTimeNow;
            _shipmentRepository.Update(shipment);

            shipment.ShipmentTrackings.Add(new ShipmentTracking
            {
                ShipmentId = shipment.Id,
                CurrentShipmentStatus = shipment.ShipmentStatus,
                Status = $"Đơn hàng đã được nhân viên nhận tại Ga {stationName}",
                EventTime = shipment.PickedUpAt.Value,
                UpdatedBy = JwtClaimUltils.GetUserId(_httpContextAccessor),
            });
            await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
        }    
    }

    // load parcel into train update parcel for shipment InTransit, validate station, train and parcel tracking
    public async Task<string> LoadParcelOnTrainAsync(string parcelCode, string trainCode, bool isLost = false)
    {
        _logger.Information("Loading parcel {ParcelCode} on train {Train}", parcelCode, trainCode);
        var stationId = JwtClaimUltils.GetUserStation(_httpContextAccessor);
        var staffId = JwtClaimUltils.GetUserId(_httpContextAccessor);
        if (string.IsNullOrEmpty(stationId))
            throw new AppException(ErrorCode.UnAuthorized, "User's station not found", StatusCodes.Status401Unauthorized);

        var parcel = await _parcelRepository.GetSingleAsync(
            p => p.ParcelCode == parcelCode,
            includeProperties: p => p.Shipment);

        if (parcel == null)
            throw new AppException(ErrorCode.NotFound, "Parcel not found", StatusCodes.Status404NotFound);

        // Only allow update for parcels in PickedUp or WaitingForNextTrain status
        var shipment = parcel.Shipment;
        if (shipment.ShipmentStatus != ShipmentStatusEnum.PickedUp
            && shipment.ShipmentStatus != ShipmentStatusEnum.WaitingForNextTrain
            )
        {
            throw new AppException(
                ErrorCode.BadRequest,
                "Shipment must be in 'PickedUp' or 'WaitingForNextTrain' status",
                StatusCodes.Status400BadRequest);
        }

        var train = await _trainRepository.GetSingleAsync(
                       t => t.TrainCode == trainCode && t.DeletedAt == null);
        if (train == null)
            throw new AppException(ErrorCode.NotFound, "Train not found", StatusCodes.Status404NotFound);

        var validStationId = await _stationRepository.GetAllStationsCanLoadShipmentAsync(shipment.Id);
        if (!validStationId.Contains(stationId))
        {
            var validStationNames = await _stationRepository.GetAll()
                .Where(s => validStationId.Contains(s.Id))
                .Select(s => s.StationNameEn)
                .ToListAsync();

            throw new AppException(
            ErrorCode.BadRequest,
            $"Parcel {parcelCode} cannot be loaded at this station. Valid stations: {string.Join(", ", validStationNames)}",
            StatusCodes.Status400BadRequest);
        }

        // retrieve the parcel tracking 
        var isParcelTrackingExists = await _parcelTrackingRepository.IsExistAsync(
            pt => pt.ParcelId == parcel.Id &&
                  pt.TrackingForShipmentStatus == ShipmentStatusEnum.InTransit &&
                  pt.StationId == stationId &&
                  pt.DeletedAt == null);

        if (isParcelTrackingExists)
        {
            throw new AppException(
            ErrorCode.BadRequest,
            "Parcel is already loaded on a train at this station.",
            StatusCodes.Status400BadRequest);
        }

        string result;
        var stationName = await _stationRepository.GetStationNameByIdAsync(stationId);
        if (isLost)
        {
            result = await ReportLostParcelAsync(parcelCode, ShipmentStatusEnum.InTransit);
        }
        else
        {
            // Update parcel status to InTransit
            result = $"Kiện hàng {parcelCode} đã lên tàu {trainCode} tại Ga {stationName}";
            var parcelTracking = new ParcelTracking
            {
                ParcelId = parcel.Id,
                Status = result,
                CurrentParcelStatus = parcel.Status,
                CurrentShipmentStatus = parcel.Shipment.ShipmentStatus,
                TrackingForShipmentStatus = ShipmentStatusEnum.InTransit,
                StationId = stationId,
                TrainId = train.Id,
                EventTime = CoreHelper.SystemTimeNow,
                UpdatedBy = staffId,
            };
            _parcelTrackingRepository.Add(parcelTracking);
            await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
        }

        // Check if the shipment is ready for the next status: all parcels confirmed for InTransit
        if (IsReadyForNextShipmentStatus(parcel.ShipmentId, ShipmentStatusEnum.InTransit))
        {
            // Update shipment status and timestamps
            shipment.ShipmentStatus = ShipmentStatusEnum.InTransit;
            shipment.CurrentTrainId = train.Id; // Update current train ID
            _shipmentRepository.Update(shipment);

            shipment.ShipmentTrackings.Add(new ShipmentTracking
            {
                ShipmentId = shipment.Id,
                CurrentShipmentStatus = shipment.ShipmentStatus,
                Status = $"Đơn hàng {shipment.TrackingCode} đã lên tàu {trainCode} tại Ga {stationName}",
                EventTime = CoreHelper.SystemTimeNow,
                UpdatedBy = staffId,
            });
            await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
        }

        return result;
    }

    // unload parcel from train and update parcel for shipment WaitForNextTrain Or Stored, validate station, train and parcel tracking
    public async Task<string> UnloadParcelFromTrain(string parcelCode, string trainCode, bool isLost = false)
    {
        _logger.Information("Unloading parcel {ParcelCode} from train {Train}", parcelCode, trainCode);
        var stationId = JwtClaimUltils.GetUserStation(_httpContextAccessor);
        var staffId = JwtClaimUltils.GetUserId(_httpContextAccessor);
        if (string.IsNullOrEmpty(stationId))
            throw new AppException(ErrorCode.UnAuthorized, "User's station not found", StatusCodes.Status401Unauthorized);

        var parcel = await _parcelRepository.GetSingleAsync(
                       p => p.ParcelCode == parcelCode,
                                  includeProperties: p => p.Shipment);

        if (parcel == null)
            throw new AppException(ErrorCode.NotFound, "Parcel not found", StatusCodes.Status404NotFound);

        // Only allow update for parcels in shipment status InTransit
        var shipment = parcel.Shipment;
        if (shipment.ShipmentStatus != ShipmentStatusEnum.InTransit)
        {
            throw new AppException(
                ErrorCode.BadRequest,
                "Shipment must be in 'InTransit' status",
                StatusCodes.Status400BadRequest);
        }

        var train = await _trainRepository.GetSingleAsync(
                                  t => t.TrainCode == trainCode && t.DeletedAt == null);
        if (train == null)
            throw new AppException(ErrorCode.NotFound, "Train not found", StatusCodes.Status404NotFound);

        var validStationId = await _stationRepository.GetAllStationsCanUnloadShipmentAsync(shipment.Id);
        if (!validStationId.Contains(stationId))
        {
            var validStationNames = await _stationRepository.GetAll()
                .Where(s => validStationId.Contains(s.Id))
                .Select(s => s.StationNameEn)
                .ToListAsync();

            throw new AppException(
            ErrorCode.BadRequest,
            $"Parcel cannot be unloaded at this station. Valid stations: {string.Join(", ", validStationNames)}",
            StatusCodes.Status400BadRequest);
        }

        // retrieve the parcel tracking 
        var isParcelTrackingExists = await _parcelTrackingRepository.IsExistAsync(
            pt => pt.ParcelId == parcel.Id &&
                  (pt.TrackingForShipmentStatus == ShipmentStatusEnum.Arrived 
                  || pt.TrackingForShipmentStatus == ShipmentStatusEnum.WaitingForNextTrain) &&
                  pt.StationId == stationId &&
                  pt.DeletedAt == null);

        if (isParcelTrackingExists)
        {
            throw new AppException(
                ErrorCode.BadRequest,
                $"Parcel {parcelCode} is already unloaded from a train at this station.",
                StatusCodes.Status400BadRequest);
        }

        // Update parcel status to WaitingForNextTrain or Arrived
        string result;
        var stationName = await _stationRepository.GetStationNameByIdAsync(stationId);
        if (!stationId.Equals(shipment.DestinationStationId))
        {
            if (isLost)
            {
                result = await ReportLostParcelAsync(parcelCode, ShipmentStatusEnum.WaitingForNextTrain);
            }
            else
            {
                result = $"Kiện hàng {parcelCode} đã xuống tàu {trainCode} tại Ga {stationName}. Chờ trung chuyển";
                var parcelTracking = new ParcelTracking
                {
                    ParcelId = parcel.Id,
                    Status = result,
                    CurrentShipmentStatus = parcel.Shipment.ShipmentStatus,
                    TrackingForShipmentStatus = ShipmentStatusEnum.WaitingForNextTrain,
                    StationId = stationId,
                    TrainId = train.Id,
                    EventTime = CoreHelper.SystemTimeNow,
                    UpdatedBy = staffId,
                };
                _parcelTrackingRepository.Add(parcelTracking);
                await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
            }

            // Check if the shipment is ready for the next status: all parcels confirmed for WaitingForNextTrain
            if (IsReadyForNextShipmentStatus(parcel.ShipmentId, ShipmentStatusEnum.WaitingForNextTrain))
            {
                // Update shipment status and timestamps
                shipment.ShipmentStatus = ShipmentStatusEnum.WaitingForNextTrain;
                shipment.CurrentStationId = stationId;
                shipment.CurrentTrainId = null; // Clear current train ID since it's unloaded
                _shipmentRepository.Update(shipment);

                shipment.ShipmentTrackings.Add(new ShipmentTracking
                {
                    ShipmentId = shipment.Id,
                    CurrentShipmentStatus = shipment.ShipmentStatus,
                    Status = $"Đơn hàng {shipment.TrackingCode} đã xuống tàu {trainCode} tại Ga {stationName}. Chờ trung chuyển",
                    EventTime = CoreHelper.SystemTimeNow,
                    UpdatedBy = staffId,
                });
                await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
            }
        }
        else
        {
            if (isLost)
            {
                result = await ReportLostParcelAsync(parcelCode, ShipmentStatusEnum.Arrived);
            }
            else
            {
                // Can update to AwaitingDelivery if wanna a shorter flow
                result = $"Kiện hàng {parcelCode} đã xuống tàu {trainCode} tại Ga {stationName}. Chờ nhập kho";
                var parcelTracking = new ParcelTracking
                {
                    ParcelId = parcel.Id,
                    Status = result,
                    CurrentShipmentStatus = parcel.Shipment.ShipmentStatus,
                    TrackingForShipmentStatus = ShipmentStatusEnum.Arrived,
                    StationId = stationId,
                    TrainId = train.Id,
                    EventTime = CoreHelper.SystemTimeNow,
                    UpdatedBy = staffId,
                };
                _parcelTrackingRepository.Add(parcelTracking);
                await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
            }

            if (IsReadyForNextShipmentStatus(parcel.ShipmentId, ShipmentStatusEnum.Arrived))
            {
                // Update shipment status and timestamps
                shipment.ShipmentStatus = ShipmentStatusEnum.Arrived;
                shipment.CurrentStationId = stationId;
                _shipmentRepository.Update(shipment);

                shipment.ShipmentTrackings.Add(new ShipmentTracking
                {
                    ShipmentId = shipment.Id,
                    CurrentShipmentStatus = shipment.ShipmentStatus,
                    Status = $"Đơn hàng {shipment.TrackingCode} đã đã xuống tàu {trainCode} tại trạm đích: Ga {stationName}. Chờ sắp xếp hàng",
                    EventTime = CoreHelper.SystemTimeNow,
                    UpdatedBy = staffId,
                });
                await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
            }
        }

        return result;
    }

    // update parcel for shipment AwaitingDelivery, destination station only
    public async Task<string> UpdateParcelForAwaitingDeliveryAsync(string parcelCode, bool isLost = false)
    {
        _logger.Information("Updating parcel {ParcelCode} for AwaitingDelivery", parcelCode);
        var stationId = JwtClaimUltils.GetUserStation(_httpContextAccessor);
        var staffId = JwtClaimUltils.GetUserId(_httpContextAccessor);

        if (string.IsNullOrEmpty(stationId))
            throw new AppException(ErrorCode.UnAuthorized, "User's station not found", StatusCodes.Status401Unauthorized);

        var parcel = await _parcelRepository.GetSingleAsync(
            p => p.ParcelCode == parcelCode,
            includeProperties: p => p.Shipment);

        if (parcel == null)
            throw new AppException(ErrorCode.NotFound, "Parcel not found", StatusCodes.Status404NotFound);

        // Only allow update for parcels in shipment status Stored
        var shipment = parcel.Shipment;
        if (shipment.ShipmentStatus != ShipmentStatusEnum.Arrived)
        {
            throw new AppException(
            ErrorCode.BadRequest,
            "Shipment must be in 'Arrived' status",
            StatusCodes.Status400BadRequest);
        }

        var stationName = await _stationRepository.GetStationNameByIdAsync(shipment.DestinationStationId);
        if (!stationId.Equals(shipment.DestinationStationId))
            throw new AppException(
            ErrorCode.BadRequest,
            $"Parcel must be at the destination station to update for AwaitingDelivery: Ga {stationName}",
            StatusCodes.Status400BadRequest);

        // Check if the parcel is already confirmed for AwaitingDelivery
        var isParcelTrackingExists = await _parcelTrackingRepository.IsExistAsync(
                pt => pt.ParcelId == parcel.Id &&
                pt.TrackingForShipmentStatus == ShipmentStatusEnum.AwaitingDelivery &&
                pt.StationId == stationId &&
                pt.DeletedAt == null);

        if (isParcelTrackingExists)
        {
            throw new AppException(
            ErrorCode.BadRequest,
            $"Parcel {parcelCode} has already been updated for AwaitingDelivery.",
                StatusCodes.Status400BadRequest);
        }

        // Update parcel status to AwaitingDelivery
        string result;
        if (isLost)
        {
            result = await ReportLostParcelAsync(parcelCode, ShipmentStatusEnum.AwaitingDelivery);
        }
        else
        {
            result = $"Kiện hàng {parcelCode} đã xuất kho để chờ giao hàng ở Ga {stationName}";
            var parcelTracking = new ParcelTracking
            {
                ParcelId = parcel.Id,
                Status = result,
                CurrentShipmentStatus = parcel.Shipment.ShipmentStatus,
                TrackingForShipmentStatus = ShipmentStatusEnum.AwaitingDelivery,
                StationId = stationId,
                EventTime = CoreHelper.SystemTimeNow,
                UpdatedBy = staffId,
            };
            _parcelTrackingRepository.Add(parcelTracking);
            await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
        }

        // Check if the shipment is ready for the next status: all parcels confirmed for AwaitingDelivery
        if (IsReadyForNextShipmentStatus(parcel.ShipmentId, ShipmentStatusEnum.AwaitingDelivery))
        {
            // Update shipment status and timestamps
            shipment.ShipmentStatus = ShipmentStatusEnum.AwaitingDelivery;
            _shipmentRepository.Update(shipment);

            shipment.ShipmentTrackings.Add(new ShipmentTracking
            {
                ShipmentId = shipment.Id,
                CurrentShipmentStatus = shipment.ShipmentStatus,
                Status = $"Đơn hàng {shipment.TrackingCode} đã xuất kho để chờ giao hàng ở Ga {stationName}",
                EventTime = CoreHelper.SystemTimeNow,
                UpdatedBy = staffId,
            });
            await _unitOfWork.SaveChangeAsync(_httpContextAccessor);

            // Schedule job to apply surcharge after delivery
            await ScheduleApplySurchargeJob(parcel.ShipmentId);
        }

        return result;
    }

    // lost report parcel
    private async Task<string> ReportLostParcelAsync(string parcelCode, ShipmentStatusEnum trackingForShipmentStatus)
    {
        _logger.Information("Reporting lost parcel {ParcelCode}", parcelCode);
        var stationId = JwtClaimUltils.GetUserStation(_httpContextAccessor);
        var staffId = JwtClaimUltils.GetUserId(_httpContextAccessor);

        if (string.IsNullOrEmpty(stationId))
            throw new AppException(ErrorCode.UnAuthorized, "User's station not found", StatusCodes.Status401Unauthorized);

        var parcel = await _parcelRepository.GetSingleAsync(
            p => p.ParcelCode == parcelCode,
            includeProperties: p => p.Shipment);

        if (parcel == null)
            throw new AppException(ErrorCode.NotFound, "Parcel not found", StatusCodes.Status404NotFound);

        // Check if the parcel is already reported as Lost
        var isParcelTrackingExists = await _parcelTrackingRepository.IsExistAsync(
                pt => pt.ParcelId == parcel.Id &&
                pt.CurrentParcelStatus == ParcelStatusEnum.Lost &&
                pt.StationId == stationId &&
                    pt.DeletedAt == null);
        if (isParcelTrackingExists)
        {
            throw new AppException(
            ErrorCode.BadRequest,
            $"Parcel {parcelCode} has already been reported as Lost.",
            StatusCodes.Status400BadRequest);
        }

        // Update parcel status to Lost
        var stationName = await _stationRepository.GetStationNameByIdAsync(stationId);
        parcel.Status = ParcelStatusEnum.Lost;
        var result = $"Kiện hàng {parcelCode} đã được báo mất tại Ga {stationName}";
        var parcelTracking = new ParcelTracking
        {
            ParcelId = parcel.Id,
            Status = result,
            CurrentParcelStatus = parcel.Status,
            CurrentShipmentStatus = parcel.Shipment.ShipmentStatus,
            TrackingForShipmentStatus = trackingForShipmentStatus,
            StationId = stationId,
            EventTime = CoreHelper.SystemTimeNow,
            UpdatedBy = staffId,
        };
        _parcelTrackingRepository.Add(parcelTracking);
        _parcelRepository.Update(parcel);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
        await CheckShipmentForLostParcelsAsync(parcel.ShipmentId, staffId);
        return result;
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
        var normalParcelCount = _shipmentRepository
            .GetAll()
            .Where(x => x.Id == shipmentId && x.DeletedAt == null)
            .SelectMany(x => x.Parcels)
            .Count(x => x.DeletedAt == null && x.Status == ParcelStatusEnum.Normal);

        // count parcelTracking have TrackingForShipmentStatus == nextShipmentStatus
        var parcelTrackingCount = _parcelRepository.GetAll()
            .Where(x => x.ShipmentId == shipmentId && x.DeletedAt == null && x.Status == ParcelStatusEnum.Normal)
            .SelectMany(p => p.ParcelTrackings)
            .Count(pt => pt.TrackingForShipmentStatus == nextShipmentStatus && pt.DeletedAt == null);

        _logger.Information("Normal parcel count: {ParcelCount}, Tracking count for status {NextStatus}: {TrackingCount}",
            normalParcelCount, nextShipmentStatus, parcelTrackingCount);

        // Check if all parcels have the next status
        if (normalParcelCount == parcelTrackingCount)
        {
            _logger.Information("Shipment {ShipmentId} is ready for status {NextStatus}",
                shipmentId, nextShipmentStatus);
            return true;
        }

        _logger.Information("Shipment {ShipmentId} is NOT ready for status {NextStatus}",
            shipmentId, nextShipmentStatus);
        return false;
    }

    // check if all parcels in shipment was lost, end the shipment 
    private async Task CheckShipmentForLostParcelsAsync(string shipmentId, string userId)
    {
        _logger.Information("Checking shipment {ShipmentId} for lost parcels", shipmentId);
        var shipment = await _shipmentRepository.GetSingleAsync(
                       s => s.Id == shipmentId && s.DeletedAt == null,
                                  includeProperties: s => s.Parcels);

        if (shipment == null)
        {
            throw new AppException(ErrorCode.NotFound, "Shipment not found", StatusCodes.Status404NotFound);
        }

        // Check if all parcels are lost
        var nonDeletedParcels = shipment.Parcels.Where(p => p.DeletedAt == null).ToList();
        var allParcelsLost = nonDeletedParcels.Any() && nonDeletedParcels.All(p => p.Status == ParcelStatusEnum.Lost);

        if (allParcelsLost)
        {
            shipment.ShipmentStatus = ShipmentStatusEnum.ToCompensate;
            _shipmentTrackingRepository.Add(new ShipmentTracking
            {
                ShipmentId = shipment.Id,
                CurrentShipmentStatus = shipment.ShipmentStatus,
                Status = $"Tất cả kiện hàng đã được báo mất. Đơn hàng {shipment.TrackingCode} đã chuyển sang trạng thái chờ bồi thường.",
                EventTime = CoreHelper.SystemTimeNow,
                UpdatedBy = userId
            });

            // calculate compensation amount
            var categoryInsurances = await _categoryInsuranceRepository.GetAll()
                .Where(ci => nonDeletedParcels.Select(p => p.CategoryInsuranceId).Contains(ci.Id))
                .ToListAsync();

            shipment.TotalCompensationFeeVnd = ParcelPriceCalculator.CalculateParcelCompensation(
                nonDeletedParcels, categoryInsurances, _parcelRepository);
            _shipmentRepository.Update(shipment);

            await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
            _logger.Information("Shipment {ShipmentId} marked as ToCompensate - all parcels lost", shipmentId);
        }
    }

    private async Task ScheduleApplySurchargeJob(string shipmentId)
    {
        _logger.Information("Scheduling job to apply surcharge for shipment ID: {@shipmentId}", shipmentId);
        var jobData = new JobDataMap
        {
            { "ApplySurcharge-for-shipmentId", shipmentId }
        };

        var freeStoreDays = await _pricingService.GetFreeStoreDaysAsync();

        // Schedule the job to run after 15 minutes
        var jobDetail = JobBuilder.Create<ApplySurchargeJob>()
            .WithIdentity($"ApplySurchargeJob-{shipmentId}")
            .UsingJobData(jobData)
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity($"Trigger-ApplySurchargeJob-{shipmentId}")
            .StartAt(DateTimeOffset.UtcNow.AddDays(freeStoreDays))
            //.StartAt(DateTimeOffset.UtcNow.AddSeconds(5))
            // Repeat every 24 hours
            .WithSimpleSchedule(x => x
                .WithIntervalInHours(24)
            .RepeatForever())
        .Build();

        await _schedulerFactory.GetScheduler().Result.ScheduleJob(jobDetail, trigger);
    }
}


