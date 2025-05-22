using MetroShip.Repository.Base;
using MetroShip.Repository.Models;

namespace MetroShip.Repository.Interfaces;

public interface IShipmentRepository : IBaseRepository<Shipment>
{
    int GetQuantityByBookedAtAndRegion(DateTimeOffset bookAtDate, string regionCode);
}