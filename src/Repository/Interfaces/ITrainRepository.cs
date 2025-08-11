using MetroShip.Repository.Base;
using MetroShip.Repository.Extensions;
using MetroShip.Repository.Models;
using System.Linq.Expressions;
using MetroShip.Utility.Enums;

namespace MetroShip.Repository.Interfaces;

public interface ITrainRepository : IBaseRepository<MetroTrain>
{
    Task<IList<Shipment>> GetShipmentsByTrainAsync(string trainId);
    Task SaveTrainLocationAsync(string trainId, double lat, double lng, string stationId);
    Task<MetroTrain?> GetTrainWithItineraryAndStationsAsync(string trainId);
    Task<Shipment?> GetShipmentWithTrainAsync(string trackingCode);
    Task<MetroTrain?> GetTrainWithItineraryAndStationsAsync(string trainId, DateOnly date,
        string timeSlotId, DirectionEnum direction);
    Task<MetroTrain?> GetTrainWithRoutesAsync(string trainId, DirectionEnum direction);
    Task<MetroTrain?> GetTrainWithAllRoutesAsync(string trainId);
    Task<IList<Shipment>> GetLoadedShipmentsByTrainAsync(string trainId);
}