using MetroShip.Repository.Base;
using MetroShip.Repository.Extensions;
using MetroShip.Repository.Models;
using static MetroShip.Repository.Repositories.ShipmentRepository;
using System.Linq.Expressions;
using MetroShip.Utility.Enums;

namespace MetroShip.Repository.Interfaces;

public interface IShipmentRepository : IBaseRepository<Shipment>
{
    int GetQuantityByBookedAtAndRegion(DateTimeOffset bookAtDate, string regionCode);
    Task<PaginatedList<ShipmentDto>> GetPaginatedListForListResponseAsync(int pageNumber,
        int pageSize,
        Expression<Func<Shipment, bool>> predicate = null,
        Expression<Func<Shipment, object>> orderBy = null, bool? isDesc = false);

    Task<ShipmentDto?> GetShipmentByTrackingCodeAsync(string trackingCode);

    /*Task<PaginatedList<ShipmentDto>> GetShipmentsByLineIdAndDateAsync(
        int pageNumber, int pageSize, string lineId, DateTimeOffset date, string? regionId, ShiftEnum? shift);*/

    Task<List<AvailableTimeSlotDto>> FindAvailableTimeSlotsAsync(CheckAvailableTimeSlotsRequest request);
}