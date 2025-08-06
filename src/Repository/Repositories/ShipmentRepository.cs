using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using MetroShip.Repository.Base;
using MetroShip.Repository.Extensions;
using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using MetroShip.Utility.Enums;
using System.Runtime.CompilerServices;
using MetroShip.Utility.Helpers;


namespace MetroShip.Repository.Repositories;

public class ShipmentRepository : BaseRepository<Shipment>, IShipmentRepository
{
    private readonly AppDbContext _context;

    public ShipmentRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    /*public class ShipmentDto : Shipment
    {
        public string DepartureStationName { get; set; }
        public string DestinationStationName { get; set; }
        public string? CurrentStationName { get; set; } 
    }*/

    /*public class RouteDto : Route
    {
        public string LineName { get; set; }
    }*/

    public async Task<PaginatedList<Shipment>>
        GetPaginatedListForListResponseAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<Shipment, bool>> predicate = null,
            Expression<Func<Shipment, object>> orderBy = null,
            bool? isDesc = false)
    {
        // Base query
        IQueryable<Shipment> q = _context.Shipments
            .AsNoTracking();

        // Apply filtering
        if (predicate is not null)
            q = q.Where(predicate);

        // Apply ordering
        q = orderBy is not null
            ? isDesc.HasValue && isDesc.Value
                ? q.OrderByDescending(orderBy)
                : q.OrderBy(orderBy)
            : q.OrderByDescending(s => s.CreatedAt);

        // Project into your DTO, grabbing only the first & last itinerary
        var projected = q.Select(s => new Shipment
        {
            Id = s.Id,
            TrackingCode = s.TrackingCode,
            DestinationStationName = _context.Stations
                .Where(station => station.Id == s.DestinationStationId)
                .Select(station => station.StationNameVi)
                .FirstOrDefault(),

            DepartureStationName = _context.Stations
                .Where(station => station.Id == s.DepartureStationId)
                .Select(station => station.StationNameVi)
                .FirstOrDefault(),

            CurrentStationName = _context.Stations
                .Where(station => station.Id == s.CurrentStationId)
                .Select(station => station.StationNameVi)
                .FirstOrDefault() /*??
            s.ShipmentItineraries
                                     .Where(i => i.IsCompleted)
                                     .OrderBy(i => i.LegOrder)
                                     .LastOrDefault().Route.ToStation.StationNameVi ??
                                 s.ShipmentItineraries
                                     .OrderBy(i => i.LegOrder)
                                     .FirstOrDefault().Route.FromStation.StationNameVi*/,

            SenderName = s.SenderName,
            SenderPhone = s.SenderPhone,
            RecipientName = s.RecipientName,
            RecipientPhone = s.RecipientPhone,

            ShipmentStatus = s.ShipmentStatus,
            //BookedAt = s.BookedAt.Value,
            TotalCostVnd = s.TotalCostVnd,
            ScheduledDateTime = s.ScheduledDateTime,
            TotalVolumeM3 = s.TotalVolumeM3,
            TotalWeightKg = s.TotalWeightKg,
            TotalKm = s.TotalKm,

            Rating = s.Rating,
            Feedback = s.Feedback,
        });

        // Use your existing paging helper on the projection
        return await PaginatedList<Shipment>
            .CreateAsync(projected, pageNumber, pageSize);
    }

    // get shipment by trackingCode with all information in ShipmentDto
    public async Task<Shipment?> GetShipmentByTrackingCodeAsync(string trackingCode)
    {
        var shipment = _context.Shipments.AsNoTracking();

        var shipmentDto = await shipment.Select(s => new Shipment
        {
            // Base Entity fields
            Id = s.Id,

            // Basic shipment info
            TrackingCode = s.TrackingCode,
            DepartureStationId = s.DepartureStationId,
            DestinationStationId = s.DestinationStationId,
            ReturnForShipmentId = s.ReturnForShipmentId,
            CurrentStationId = s.CurrentStationId,
            CurrentTrainId = s.CurrentTrainId,
            ShipmentStatus = s.ShipmentStatus,

            // Financial fields
            TotalCostVnd = s.TotalCostVnd,
            TotalShippingFeeVnd = s.TotalShippingFeeVnd,
            TotalInsuranceFeeVnd = s.TotalInsuranceFeeVnd,
            TotalSurchargeFeeVnd = s.TotalSurchargeFeeVnd,
            TotalOverdueSurchargeFee = s.TotalOverdueSurchargeFee,

            // Measurement fields
            TotalKm = s.TotalKm,
            TotalWeightKg = s.TotalWeightKg,
            TotalVolumeM3 = s.TotalVolumeM3,

            // Scheduling fields
            TimeSlotId = s.TimeSlotId,
            StartReceiveAt = s.StartReceiveAt,
            ScheduledDateTime = s.ScheduledDateTime,
            ScheduledShift = s.ScheduledShift,
            PriceStructureDescriptionJSON = s.PriceStructureDescriptionJSON,

            // Status tracking timestamps
            BookedAt = s.BookedAt,
            ApprovedAt = s.ApprovedAt,
            RejectedAt = s.RejectedAt,
            RejectionReason = s.RejectionReason,
            RejectedBy = s.RejectedBy,
            ConfirmedBy = s.ConfirmedBy,
            PaymentDealine = s.PaymentDealine,
            PaidAt = s.PaidAt,
            PickedUpAt = s.PickedUpAt,
            PickedUpBy = s.PickedUpBy,
            AwaitedDeliveryAt = s.AwaitedDeliveryAt,
            DeliveredAt = s.DeliveredAt,
            SurchargeAppliedAt = s.SurchargeAppliedAt,
            CancelledAt = s.CancelledAt,
            RefundedAt = s.RefundedAt,

            // Return fields
            ReturnRequestedAt = s.ReturnRequestedAt,
            ReturnConfirmedAt = s.ReturnConfirmedAt,
            ReturnReason = s.ReturnReason,
            ReturnConfirmedBy = s.ReturnConfirmedBy,
            ReturnPickupAt = s.ReturnPickupAt,
            ReturnDeliveredAt = s.ReturnDeliveredAt,
            ReturnCancelledAt = s.ReturnCancelledAt,

            // Customer fields - Sender
            SenderId = s.SenderId,
            SenderName = s.SenderName,
            SenderPhone = s.SenderPhone,

            // Customer fields - Recipient
            RecipientId = s.RecipientId,
            RecipientName = s.RecipientName,
            RecipientPhone = s.RecipientPhone,
            RecipientEmail = s.RecipientEmail,
            RecipientNationalId = s.RecipientNationalId,

            // Feedback fields
            Rating = s.Rating,
            Feedback = s.Feedback,
            FeedbackAt = s.FeedbackAt,
            FeedbackRespondedAt = s.FeedbackRespondedAt,
            FeedbackResponse = s.FeedbackResponse,
            FeedbackResponseBy = s.FeedbackResponseBy,

            // Station tracking
            DestinationStationName = _context.Stations
                .Where(station => station.Id == s.DestinationStationId)
                .Select(station => station.StationNameVi)
                .FirstOrDefault(),

            DepartureStationName = _context.Stations
                .Where(station => station.Id == s.DepartureStationId)
                .Select(station => station.StationNameVi)
                .FirstOrDefault(),

            CurrentStationName = _context.Stations
                .Where(station => station.Id == s.CurrentStationId)
                .Select(station => station.StationNameVi)
                .FirstOrDefault() /*??
            s.ShipmentItineraries
                                     .Where(i => i.IsCompleted)
                                     .OrderBy(i => i.LegOrder)
                                     .LastOrDefault().Route.ToStation.StationNameVi ??
                                 s.ShipmentItineraries
                                     .OrderBy(i => i.LegOrder)
                                     .FirstOrDefault().Route.FromStation.StationNameVi*/,

            ShipmentItineraries = s.ShipmentItineraries.Select(itinerary => new ShipmentItinerary
            {
                Id = itinerary.Id,
                RouteId = itinerary.RouteId,
                LegOrder = itinerary.LegOrder,
                Route = new Route
                {
                    Id = itinerary.Route.Id,
                    LineId = itinerary.Route.LineId,
                    LineName = itinerary.Route.MetroLine.LineNameVi,
                    FromStationId = itinerary.Route.FromStationId,
                    ToStationId = itinerary.Route.ToStationId,
                    RouteNameVi = itinerary.Route.RouteNameVi,
                    Direction = itinerary.Route.Direction,
                    TravelTimeMin = itinerary.Route.TravelTimeMin,
                    LengthKm = itinerary.Route.LengthKm,
                },
                TrainId = itinerary.TrainId,
                TimeSlotId = itinerary.TimeSlotId,
                Date = itinerary.Date,
                IsCompleted = itinerary.IsCompleted,
            }).OrderBy(itinerary => itinerary.LegOrder).ToList(),
            
            Parcels = s.Parcels
            .Select(parcel => new Parcel
            {
                Id = parcel.Id,
                ParcelCode = parcel.ParcelCode,
                ShipmentId = parcel.ShipmentId,
                WeightKg = parcel.WeightKg,
                LengthCm = parcel.LengthCm,
                WidthCm = parcel.WidthCm,
                HeightCm = parcel.HeightCm,
                Description = parcel.Description,
                PriceVnd = parcel.PriceVnd,
                ParcelCategoryId = parcel.ParcelCategoryId,
                ShippingFeeVnd = parcel.ShippingFeeVnd,
                InsuranceFeeVnd = parcel.InsuranceFeeVnd,
                ParcelCategory = new ParcelCategory
                {
                    Id = parcel.ParcelCategory.Id,
                    CategoryName = parcel.ParcelCategory.CategoryName,
                    Description = parcel.ParcelCategory.Description,
                    InsuranceRate = parcel.ParcelCategory.InsuranceRate,
                    InsuranceFeeVnd = parcel.ParcelCategory.InsuranceFeeVnd,
                    IsInsuranceRequired = parcel.ParcelCategory.IsInsuranceRequired,
                },
            }).OrderBy(p => p.ParcelCode).ToList(),

            ShipmentMedias = s.ShipmentMedias.Select(media => new ShipmentMedia
            {
                Id = media.Id,
                ShipmentId = media.ShipmentId,
                MediaType = media.MediaType,
                MediaUrl = media.MediaUrl,
                CreatedAt = media.CreatedAt,
                CreatedBy = media.CreatedBy,
                LastUpdatedAt = media.LastUpdatedAt,
                LastUpdatedBy = media.LastUpdatedBy,
            }).ToList(),

            ShipmentTrackings = s.ShipmentTrackings.Select(tracking => new ShipmentTracking
            {
                Id = tracking.Id,
                ShipmentId = tracking.ShipmentId,
                Status = tracking.Status,
                UpdatedBy = tracking.UpdatedBy,
                EventTime = tracking.EventTime,
                Note = tracking.Note,
            }).OrderByDescending(st => st.EventTime).ToList(),
            //Transactions = shipment.Transactions.ToList(),
        }).AsSplitQuery().FirstOrDefaultAsync(x => x.TrackingCode == trackingCode);

        if (shipmentDto == null)
        {
            return null;
        }

        return shipmentDto;
    }

    // get all shipments by lineId and date
    /*public async Task<PaginatedList<ShipmentDto>> GetShipmentsByLineIdAndDateAsync(
    int pageNumber, int pageSize, string lineId, DateTimeOffset date, string? regionId, ShiftEnum? shift)
    {
        var shipments = _context.Shipments
            .AsNoTracking()
            .AsSingleQuery()
            .Where(s => s.ShipmentItineraries.Any(i => i.Route.LineId == lineId)
                        && s.ApprovedAt.HasValue && s.ScheduledDateTime.HasValue
                        && s.ScheduledDateTime.Value.Date == date.Date);

        if (!string.IsNullOrEmpty(regionId))
        {
            shipments = shipments.Where(s => s.TrackingCode.Contains(regionId));
        }

        if (shift != null)
        {
            var shiftSlot = await _context.MetroBasePrices
                .AsNoTracking()
                .Include(s => s.TimeSlot)
                .Where(s => s.LineId == lineId && s.TimeSlot.Shift == shift)
                .Select(s => s.TimeSlot)
                .FirstOrDefaultAsync();

            if (shiftSlot != null)
            {
                var open = shiftSlot.OpenTime.ToTimeSpan();
                var close = shiftSlot.CloseTime.ToTimeSpan();
                shipments = shipments.Where(s =>
                    s.ScheduledDateTime.HasValue &&
                    s.ScheduledDateTime.Value.TimeOfDay >= open &&
                    s.ScheduledDateTime.Value.TimeOfDay <= close);
            }
        }

        var shipmentDtos = shipments
            .OrderBy(s => s.ApprovedAt)
            .Select(s => new ShipmentDto
            {
                Id = s.Id,
                TrackingCode = s.TrackingCode,
                DepartureStationName = s.ShipmentItineraries
                    .OrderBy(i => i.LegOrder)
                    .Select(i => i.Route.FromStation.StationNameVi)
                    .FirstOrDefault(),
                DestinationStationName = s.ShipmentItineraries
                    .OrderBy(i => i.LegOrder)
                    .Select(i => i.Route.ToStation.StationNameVi)
                    .LastOrDefault(),
                SenderName = s.SenderName,
                SenderPhone = s.SenderPhone,
                RecipientName = s.RecipientName,
                RecipientPhone = s.RecipientPhone,
                ShipmentStatus = s.ShipmentStatus,
                BookedAt = s.BookedAt.Value,
                TotalCostVnd = s.TotalCostVnd,
                ScheduledDateTime = s.ScheduledDateTime
            });

        return await PaginatedList<ShipmentDto>.CreateAsync(shipmentDtos, pageNumber, pageSize);
    }*/

    /*public class CheckAvailableTimeSlotsRequest
    {
        public string ShipmentId { get; set; }
        public int? MaxAttempts { get; set; } = 3; // số ca thử dời tối đa
    }*/

    /*public class AvailableTimeSlotDto
    {
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset Date { get; set; }
        public string TimeSlotId { get; set; }
        public string TimeSlotName { get; set; }
        public decimal RemainingWeightKg { get; set; }
        public decimal RemainingVolumeM3 { get; set; }
        public List<string>? ParcelIds { get; set; }

        // 🆕 Thêm chi tiết MetroTimeSlot
        public DayOfWeekEnum? DayOfWeek { get; set; }
        public DateOnly? SpecialDate { get; set; }
        public TimeOnly OpenTime { get; set; }
        public TimeOnly CloseTime { get; set; }
        public ShiftEnum Shift { get; set; }
        public bool IsAbnormal { get; set; }
        public int ScheduleBeforeShiftMinutes { get; set; }=30; // Thời gian đặt trước ca (phút)
    }*/

    /*public async Task<List<AvailableTimeSlotDto>> FindAvailableTimeSlotsAsync(CheckAvailableTimeSlotsRequest request)
    {
        var result = new List<AvailableTimeSlotDto>();
        const decimal maxWeight = 20000m;
        const decimal maxVolume = 160m;

        var shipment = await _context.Shipments
            .Include(s => s.Parcels)
            .Include(s => s.ShipmentItineraries)
            .ThenInclude(i => i.Route)
            .FirstOrDefaultAsync(s => s.Id == request.ShipmentId);

        if (shipment == null || !shipment.Parcels.Any())
            return result;

        var bookedAt = shipment.BookedAt ?? DateTimeOffset.UtcNow;
        var offset = bookedAt.Offset;

        // ✅ Nếu người dùng đã chọn thời gian thì ưu tiên
        var startDate = shipment.ScheduledDateTime?.Date ?? bookedAt.Date;

        var routeId = shipment.ShipmentItineraries.FirstOrDefault()?.RouteId;
        if (routeId == null)
            return result;

        var totalWeight = shipment.Parcels.Sum(p => p.WeightKg);
        var totalVolume = shipment.Parcels.Sum(p => (p.LengthCm * p.WidthCm * p.HeightCm) / 1_000_000m);

        shipment.TotalWeightKg = totalWeight;
        shipment.TotalVolumeM3 = totalVolume;

        if (totalWeight > maxWeight || totalVolume > maxVolume)
        {
            shipment.ShipmentStatus = ShipmentStatusEnum.Rejected;
            await _context.SaveChangesAsync();
            return result;
        }

        var timeSlots = await _context.MetroTimeSlots.ToListAsync();

        for (int dayOffset = 0; dayOffset <= request.MaxAttempts; dayOffset++)
        {
            var currentDate = startDate.AddDays(dayOffset);

            var itineraries = await _context.ShipmentItineraries
                .Include(i => i.Shipment)
                .Where(i =>
                    i.Date.HasValue &&
                    i.Date.Value.Equals(currentDate) &&
                    i.Shipment.ShipmentStatus == ShipmentStatusEnum.AwaitingDropOff)
                .ToListAsync();

            foreach (var slot in timeSlots)
            {
                var usage = itineraries.Where(i => i.TimeSlotId == slot.Id).ToList();

                var usedVolume = usage.Sum(x => x.Shipment.TotalVolumeM3 ?? 0);
                var usedWeight = usage.Sum(x => x.Shipment.TotalWeightKg ?? 0);

                var remainingVol = maxVolume - usedVolume;
                var remainingWgt = maxWeight - usedWeight;

                if (remainingVol >= totalVolume && remainingWgt >= totalWeight)
                {
                    var shiftStartTime = slot.Shift switch
                    {
                        ShiftEnum.Morning => new TimeSpan(8, 0, 0),
                        ShiftEnum.Afternoon => new TimeSpan(13, 0, 0),
                        ShiftEnum.Evening => new TimeSpan(18, 0, 0),
                        ShiftEnum.Night => new TimeSpan(22, 0, 0),
                        _ => new TimeSpan(0, 0, 0)
                    };

                    var scheduledDateTime = new DateTimeOffset(
                        currentDate.Year, currentDate.Month, currentDate.Day,
                        shiftStartTime.Hours, shiftStartTime.Minutes, shiftStartTime.Seconds,
                        offset
                    );

                    var finalStartDate = new DateTimeOffset(startDate, offset);

                    // ✅ Logging để xác minh
                    Console.WriteLine($"Slot Found - Shipment: {shipment.Id}, StartDate: {finalStartDate}, Date: {scheduledDateTime}");

                    result.Add(new AvailableTimeSlotDto
                    {
                        StartDate = finalStartDate,
                        Date = scheduledDateTime,
                        TimeSlotId = slot.Id,
                        TimeSlotName = slot.Shift.ToString(),
                        RemainingWeightKg = remainingWgt,
                        RemainingVolumeM3 = remainingVol,
                        ParcelIds = shipment.Parcels.Select(p => p.Id).ToList(),

                        DayOfWeek = slot.DayOfWeek,
                        SpecialDate = slot.SpecialDate,
                        OpenTime = slot.OpenTime,
                        CloseTime = slot.CloseTime,
                        Shift = slot.Shift,
                        IsAbnormal = slot.IsAbnormal,
                        ScheduleBeforeShiftMinutes = 30
                    });
                }
            }

            // ✅ Dừng sau khi có 3 slots hợp lệ
            if (result.Count >= 3)
                break;
        }

        return result;
    }*/

    /*public async Task<ShipmentItinerary?> GetItineraryByShipmentIdAsync(string shipmentId)
    {
        return await _context.ShipmentItineraries
            .Include(si => si.Route)
                .ThenInclude(r => r.ToStation)
            .Include(si => si.Train)
            .FirstOrDefaultAsync(si => si.ShipmentId == shipmentId);
    }*/

    public async Task UpdateShipmentStatusAsync(string shipmentId, ShipmentStatusEnum status)
    {
        var shipment = await _context.Shipments.FirstOrDefaultAsync(s => s.Id == shipmentId);

        if (shipment == null)
        {
            throw new InvalidOperationException($"Không tìm thấy shipment với ID {shipmentId}");
        }

        shipment.ShipmentStatus = status;
        _context.Shipments.Update(shipment);
        await _context.SaveChangesAsync();
    }

    public async Task AddParcelTrackingAsync(string parcelId, string status, string stationId, string updatedBy)
    {
        var tracking = new ParcelTracking
        {
            ParcelId = parcelId,
            Status = status,
            StationId = stationId,
            UpdatedBy = updatedBy,
            EventTime = DateTimeOffset.UtcNow
        };
        _context.ParcelTrackings.Add(tracking);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> CheckIfThisStationCanWaitNextTrain (string stationId, string shipmentId)
    {
        var shipment = await _context.Shipments
            .Include(s => s.ShipmentItineraries)
            .ThenInclude(i => i.Route)
            .FirstOrDefaultAsync(s => s.Id == shipmentId);

        // Kiểm tra xem shipment có tồn tại không
        if (shipment == null)
        {
            Log.Error($"Shipment with ID {shipmentId} not found.");
            return false;
        }

        shipment.ShipmentItineraries.OrderBy(i => i.LegOrder);

        // Get itinerary của shipment mà route.toStationId chứa stationId
        var itinerary = shipment.ShipmentItineraries
            .FirstOrDefault(i => i.Route.ToStationId == stationId);

        // Nếu có station, kiểm tra xem itinerary có phải the last of shipment.ShipmentItineraries
        // có return false
        // không thì kiểm tra xem itinerary kế tiếp route.lineId có giống với itinerary.Route.LineId không
        // có return false
        // nếu không thì return true
        if (itinerary == null)
        {
            Log.Error($"No itinerary found for shipment {shipmentId} at station {stationId}.");
            return false;
        }

        var lastItineraryId = shipment.ShipmentItineraries
            .OrderBy(i => i.LegOrder)
            .Select(i => i.Id)
            .LastOrDefault();

        if (itinerary.Id == lastItineraryId)
        {
            Log.Information($"Shipment {shipmentId} has reached the last itinerary at station {stationId}.");
            return false; // Đã đến ga cuối, không thể chờ chuyến tiếp theo
        }

        // Lấy itinerary kế tiếp
        var nextItinerary = shipment.ShipmentItineraries
            .OrderBy(i => i.LegOrder)
            .SkipWhile(i => i != itinerary)
            .Skip(1)
            .FirstOrDefault();

        // Kiểm tra xem lineId của itinerary kế tiếp có giống với lineId của itinerary hiện tại không
        if (nextItinerary.Route.LineId == itinerary.Route.LineId)
        {
            Log.Information($"Shipment {shipmentId} can't wait for the next train at station {stationId}: Same line!");
            return false; // Cùng tuyến, không thể chờ chuyến tiếp theo
        }

        Log.Information($"Shipment {shipmentId} can wait for the next train at station {stationId}.");
        return true; // Có thể chờ chuyến tiếp theo
    }

    public async Task<PaginatedList<Shipment>> GetShipmentsCanWaitNextTrainAtStation(
        int pageNumber, int pageSize, string stationId,
        Expression<Func<Shipment, bool>> predicate = null,
        Expression<Func<Shipment, object>> orderBy = null,
        bool? isDesc = false)
    {
        // First, get shipment IDs that can wait for next train at this station
        var statusList = new List<ShipmentStatusEnum>
        {
            ShipmentStatusEnum.AwaitingDropOff,
            ShipmentStatusEnum.PickedUp,
            ShipmentStatusEnum.WaitingForNextTrain,
            ShipmentStatusEnum.InTransit
        };

        var shipmentIdsCanWait = _context.Shipments
            .Include(s => s.ShipmentItineraries)
            .ThenInclude(i => i.Route)
            .Where(s => statusList.Contains(s.ShipmentStatus) &&
            s.ShipmentItineraries.Any(i => i.Route.ToStationId == stationId))
            .AsEnumerable() // Switch to client-side evaluation for complex logic
            .Where(s =>
            {
                var orderedItineraries = s.ShipmentItineraries.OrderBy(i => i.LegOrder).ToList();
                var currentItinerary = orderedItineraries.FirstOrDefault(i => i.Route.ToStationId == stationId);

                if (currentItinerary == null) return false;

                var currentIndex = orderedItineraries.FindIndex(i => i.Id == currentItinerary.Id);
                if (currentIndex == -1 || currentIndex == orderedItineraries.Count - 1) return false; // Last itinerary

                var nextItinerary = orderedItineraries[currentIndex + 1];
                return nextItinerary.Route.LineId != currentItinerary.Route.LineId; // Different lines
            })
            .Select(s => s.Id)
            .ToList();

        predicate = predicate.And(s => shipmentIdsCanWait.Contains(s.Id));

        return await GetPaginatedListForListResponseAsync(
            pageNumber, pageSize,predicate,orderBy,isDesc);
    }
}