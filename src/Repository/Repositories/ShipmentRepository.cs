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
        public IList<ShipmentItineraryDto> ShipmentItinerariesDto { get; set; }
    }

    public class ShipmentItineraryDto : ShipmentItinerary
    {
        public string RouteId { get; set; }
        public string LineId { get; set; }
        public string LineName { get; set; }
        public string FromStationId { get; set; }
        public string ToStaionId { get; set; }
        public string FromStationName { get; set; }
        public string Direction { get; set; }
        public string ToStationName { get; set; }
        public int TravelTimeMin { get; set; }
        public decimal LengthKm { get; set; }
        public decimal MetroBasePriceVndPerKm { get; set; }
        public decimal AvgVelocityKmh { get; set; }
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
        var shipment = await _context.Shipments
            .AsSplitQuery()
            .Include(x => x.Transactions)
            .Include(x => x.Parcels)
            .Include(x => x.ShipmentItineraries)
            .ThenInclude(itinerary => itinerary.Route)
            .ThenInclude(route => route.MetroLine)
            .FirstOrDefaultAsync(x => x.TrackingCode == trackingCode);

        var shipmentItineraries = _context.ShipmentItineraries
            .AsNoTracking().Where(x => x.ShipmentId == shipment.Id);

        if (shipment == null) return null;

        var shipmentDto = new ShipmentDto
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
            ShipmentItinerariesDto = shipmentItineraries.Select(itinerary => new ShipmentItineraryDto
            {
                RouteId = itinerary.RouteId,
                LineId = itinerary.Route.LineId,
                LineName = itinerary.Route.MetroLine.LineNameVi,
                FromStationId = itinerary.Route.FromStationId,
                ToStaionId = itinerary.Route.ToStationId,
                FromStationName = itinerary.Route.FromStation.StationNameVi,
                Direction = itinerary.Route.Direction.ToString(),
                ToStationName = itinerary.Route.ToStation.StationNameVi,
                TravelTimeMin = itinerary.Route.TravelTimeMin,
                LengthKm = itinerary.Route.LengthKm,
                MetroBasePriceVndPerKm = itinerary.Route.MetroLine.BasePriceVndPerKm,
                AvgVelocityKmh = itinerary.Route.MetroLine.TotalKm / itinerary.Route.MetroLine.RouteTimeMin.Value,
            }).ToList(),
            DepartureStationName = shipmentItineraries
                .OrderBy(i => i.LegOrder)
                .Select(i => i.Route.FromStation.StationNameVi)
                .FirstOrDefault(),
            DestinationStationName = shipmentItineraries
                .OrderBy(i => i.LegOrder)
                .Select(i => i.Route.ToStation.StationNameVi)
                .LastOrDefault(),
            
            ShippingFeeVnd = shipment.ShippingFeeVnd,
            InsuranceFeeVnd = shipment.InsuranceFeeVnd,
            SurchargeFeeVnd = shipment.SurchargeFeeVnd,
            Parcels = shipment.Parcels.ToList(),
            Transactions = shipment.Transactions.ToList(),
        };

        Console.WriteLine(shipmentDto);
        return shipmentDto;
    }
}