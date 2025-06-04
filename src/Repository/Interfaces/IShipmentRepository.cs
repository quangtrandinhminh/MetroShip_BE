using MetroShip.Repository.Base;
using MetroShip.Repository.Extensions;
using MetroShip.Repository.Models;
using static MetroShip.Repository.Repositories.ShipmentRepository;
using System.Linq.Expressions;

namespace MetroShip.Repository.Interfaces;

public interface IShipmentRepository : IBaseRepository<Shipment>
{
    int GetQuantityByBookedAtAndRegion(DateTimeOffset bookAtDate, string regionCode);
    Task<PaginatedList<ShipmentDto>>
        GetPaginatedListForListResponseAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<Shipment, bool>> predicate = null,
            Expression<Func<Shipment, object>> orderBy = null);

    Task<ShipmentDto?> GetShipmentByTrackingCodeAsync(string trackingCode);
}