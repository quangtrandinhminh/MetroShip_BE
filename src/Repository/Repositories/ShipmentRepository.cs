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


namespace MetroShip.Repository.Repositories;

public class ShipmentRepository : BaseRepository<Shipment>, IShipmentRepository
{
    private readonly AppDbContext _context;

    public ShipmentRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    // get quantity of shipments by BookAt date and trackingCode contain regionCode
    public int GetQuantityByBookedAtAndRegion(DateTimeOffset bookAtDate, string regionCode)
    {
        var quantity = _context.Shipments
            .Count(x => x.BookedAt != null && x.BookedAt.Value.Date == bookAtDate.Date && x.TrackingCode.Contains(regionCode));

        return quantity;
    }

    public class ShipmentDto : Shipment
    {
        public string DepartureStationName { get; set; }
        public string DestinationStationName { get; set; }
    }

    public class RouteDto : Route
    {
        public string LineName { get; set; }
    }

    public async Task<PaginatedList<ShipmentDto>>
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
        var projected = q.Select(s => new ShipmentDto
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
            ScheduledDateTime = s.ScheduledDateTime,
        });

        // Use your existing paging helper on the projection
        return await PaginatedList<ShipmentDto>
            .CreateAsync(projected, pageNumber, pageSize);
    }

    // get shipment by trackingCode with all information in ShipmentDto
    public async Task<ShipmentDto?> GetShipmentByTrackingCodeAsync(string trackingCode)
    {
        var shipment = _context.Shipments.AsNoTracking();

        if (shipment == null) return null;

        var shipmentDto = await shipment.Select(shipment => new ShipmentDto
        {
            // info 
            Id = shipment.Id,
            SenderId = shipment.SenderId,
            SenderName = shipment.SenderName,
            SenderPhone = shipment.SenderPhone,
            RecipientName = shipment.RecipientName,
            RecipientPhone = shipment.RecipientPhone,
            RecipientEmail = shipment.RecipientEmail,
            RecipientNationalId = shipment.RecipientNationalId,
            TrackingCode = shipment.TrackingCode,

            // status tracking
            ApprovedAt = shipment.ApprovedAt,
            BookedAt = shipment.BookedAt.Value,
            TotalCostVnd = shipment.TotalCostVnd,
            ScheduledDateTime = shipment.ScheduledDateTime,
            PaidAt = shipment.PaidAt,
            PickedUpAt = shipment.PickedUpAt,
            DeliveredAt = shipment.DeliveredAt,
            SurchargeAppliedAt = shipment.SurchargeAppliedAt,
            CancelledAt = shipment.CancelledAt,
            RefundedAt = shipment.RefundedAt,
            ShipmentStatus = shipment.ShipmentStatus,
            TotalKm = shipment.TotalKm,
            ShipmentItineraries = shipment.ShipmentItineraries.Select(itinerary => new ShipmentItinerary
            {
                Id = itinerary.Id,
                RouteId = itinerary.RouteId,
                LegOrder = itinerary.LegOrder,
                Route = new RouteDto
                {
                    LineId = itinerary.Route.LineId,
                    LineName = itinerary.Route.MetroLine.LineNameVi,
                    FromStationId = itinerary.Route.FromStationId,
                    ToStationId = itinerary.Route.ToStationId,
                    RouteNameVi = itinerary.Route.RouteNameVi,
                    Direction = itinerary.Route.Direction,
                    TravelTimeMin = itinerary.Route.TravelTimeMin,
                    LengthKm = itinerary.Route.LengthKm,
                }
            }).OrderBy(itinerary => itinerary.LegOrder).ToList(),
            DepartureStationName = shipment.ShipmentItineraries
                .OrderBy(i => i.LegOrder)
                .Select(i => i.Route.FromStation.StationNameVi)
                .FirstOrDefault(),
            DestinationStationName = shipment.ShipmentItineraries
                .OrderBy(i => i.LegOrder)
                .Select(i => i.Route.ToStation.StationNameVi)
                .LastOrDefault(),
            
            TotalShippingFeeVnd = shipment.TotalShippingFeeVnd,
            TotalInsuranceFeeVnd = shipment.TotalInsuranceFeeVnd,
            TotalSurchargeFeeVnd = shipment.TotalSurchargeFeeVnd,
            Parcels = shipment.Parcels
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
                ParcelCategory = new ParcelCategory
                {
                    Id = parcel.ParcelCategory.Id,
                    CategoryName = parcel.ParcelCategory.CategoryName,
                    Description = parcel.ParcelCategory.Description,
                },
            })
            .ToList(),
            //Transactions = shipment.Transactions.ToList(),
        }).AsSplitQuery().FirstOrDefaultAsync(x => x.TrackingCode == trackingCode);

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

    public class CheckAvailableTimeSlotsRequest
    {
        public string RouteId { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public int MaxAttempts { get; set; } = 3; // số ca thử dời tối đa
        public List<string>? ParcelIds { get; set; }
    }

    public class AvailableTimeSlotDto
    {
        public DateTimeOffset Date { get; set; }
        public string TimeSlotId { get; set; }
        public string TimeSlotName { get; set; } // ví dụ: "Morning", "Afternoon", "Evening"
        public decimal RemainingWeightKg { get; set; }
        public decimal RemainingVolumeM3 { get; set; }
    }


    public async Task<List<AvailableTimeSlotDto>> FindAvailableTimeSlotsAsync(CheckAvailableTimeSlotsRequest request)
    {
        var result = new List<AvailableTimeSlotDto>();
        var maxWeight = 20000m;
        var maxVolume = 160m;

        // 1. Lấy danh sách parcel
        var parcels = await _context.Parcels
            .Where(p => request.ParcelIds.Contains(p.Id))
            .ToListAsync();

        if (!parcels.Any())
            return result;

        // 2. Tính tổng Volume & Weight
        var requiredWeight = parcels.Sum(p => p.WeightKg);
        var requiredVolume = parcels.Sum(p => (p.LengthCm * p.WidthCm * p.HeightCm) / 1000000m); // Convert cm³ to m³

        // 3. Lấy danh sách ca (slot)
        var timeSlots = await _context.MetroTimeSlots.ToListAsync();

        // 4. Check từng ca trong khoảng cho phép
        for (int dayOffset = 0; dayOffset <= request.MaxAttempts; dayOffset++)
        {
            var currentDate = request.StartDate.Date.AddDays(dayOffset);

            foreach (var slot in timeSlots)
            {
                var usage = await _context.ShipmentItineraries
                    .Include(i => i.Shipment)
                    .ThenInclude(s => s.Parcels)
                    .Where(i =>
                        i.RouteId == request.RouteId &&
                        i.Date == currentDate &&
                        i.TimeSlotId == slot.Id &&
                        i.Shipment.ShipmentStatus == ShipmentStatusEnum.AwaitingPayment)
                    .ToListAsync();

                var usedVolume = usage.Sum(x => x.Shipment.TotalVolumeM3 ?? 0);
                var usedWeight = usage.Sum(x => x.Shipment.TotalWeightKg ?? 0);

                var remainingVol = maxVolume - usedVolume;
                var remainingWgt = maxWeight - usedWeight;

                if (remainingVol >= requiredVolume && remainingWgt >= requiredWeight)
                {
                    result.Add(new AvailableTimeSlotDto
                    {
                        Date = currentDate,
                        TimeSlotId = slot.Id,
                        TimeSlotName = slot.Shift.ToString(),
                        RemainingWeightKg = remainingWgt,
                        RemainingVolumeM3 = remainingVol
                    });
                }
            }

            if (result.Any()) break; // Dừng sớm nếu đã có slot phù hợp
        }

        return result;
    }
}