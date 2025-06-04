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
            Expression<Func<Shipment, object>> orderBy = null)
    {
        // Base query
        IQueryable<Shipment> q = _context.Shipments
            .AsNoTracking();

        // Apply filtering
        if (predicate is not null)
            q = q.Where(predicate);

        // Apply ordering
        q = orderBy is not null
            ? q.OrderByDescending(orderBy)
            : q.OrderByDescending(s => s.CreatedAt);

        // Project into your DTO, grabbing only the first & last itinerary
        var projected = q.Select(s => new ShipmentDto
        {
            SenderName = s.SenderName,
            SenderPhone = s.SenderPhone,
            RecipientName = s.RecipientName,
            RecipientPhone = s.RecipientPhone,
            BookedAt = s.BookedAt.Value,
            TrackingCode = s.TrackingCode,
            TotalCostVnd = s.TotalCostVnd,
            ScheduledDateTime = s.ScheduledDateTime,
            DepartureStationName = s.ShipmentItineraries
                .OrderBy(i => i.LegOrder)
                .Select(i => i.Route.FromStation.StationNameVi)
                .FirstOrDefault(),
            DestinationStationName = s.ShipmentItineraries
                .OrderBy(i => i.LegOrder)
                .Select(i => i.Route.ToStation.StationNameVi)
                .LastOrDefault()
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
            PickupAt = shipment.PickupAt,
            DeliveredAt = shipment.DeliveredAt,
            SurchargeAppliedAt = shipment.SurchargeAppliedAt,
            CancelledAt = shipment.CancelledAt,
            RefundedAt = shipment.RefundedAt,
            ShipmentStatus = shipment.ShipmentStatus,
            TotalKm = shipment.TotalKm,
            ShipmentItineraries = shipment.ShipmentItineraries.Select(itinerary => new ShipmentItinerary
            {
                RouteId = itinerary.RouteId,
                LegOrder = itinerary.LegOrder,
                BasePriceVndPerKm = itinerary.BasePriceVndPerKm,
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
            
            ShippingFeeVnd = shipment.ShippingFeeVnd,
            InsuranceFeeVnd = shipment.InsuranceFeeVnd,
            SurchargeFeeVnd = shipment.SurchargeFeeVnd,
            Parcels = shipment.Parcels
            .Select(parcel => new Parcel
            {
                Id = parcel.Id,
                ParcelCode = parcel.ParcelCode,
                WeightKg = parcel.WeightKg,
                LengthCm = parcel.LengthCm,
                WidthCm = parcel.WidthCm,
                HeightCm = parcel.HeightCm,
                Description = parcel.Description,
                ParcelStatus = parcel.ParcelStatus,
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
}