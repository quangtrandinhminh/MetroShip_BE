using MetroShip.Repository.Base;
using MetroShip.Repository.Extensions;
using MetroShip.Repository.Models;
using System.Linq.Expressions;

namespace MetroShip.Repository.Interfaces;

public interface ITrainRepository : IBaseRepository<MetroTrain>
{
    Task<IList<Shipment>> GetShipmentsByTrainAsync(string trainId);
    Task SaveTrainLocationAsync(string trainId, double lat, double lng, string stationId);
}