using MetroShip.Repository.Base;
using MetroShip.Repository.Extensions;
using MetroShip.Repository.Models;
using MetroShip.Utility.Enums;

namespace MetroShip.Repository.Interfaces;

public interface IStationRepository : IBaseRepository<Station>
{
    Task<List<NearbyStationIds>> GetAllStationIdNearUser(double userLatitude,
        double userLongitude,
        int maxDistanceInMeters = 1000,
        int maxCount = 10);
        
    IEnumerable<Station> GetStationsByRegion(string? regionId);
    bool AreStationsInSameMetroLine(string departureStationId, string destinationStationId);
    Task<string?> GetStationNameByIdAsync(string stationId);
    Task<Dictionary<string, string>> GetStationNamesByIdsAsync(List<string> stationIds);
    Task<Dictionary<(string, DirectionEnum), (string, string)>> GetStartAndEndStationIdsOfRouteAsync(string metroLineId);
    Task<Dictionary<(string, DirectionEnum), (string, string)>> GetStartAndEndStationIdsOfRouteAsync(List<string> metroLineIds);

    Task<List<string>> GetAllStationsCanLoadShipmentAsync(string shipmentId);
    Task<List<string>> GetAllStationsCanUnloadShipmentAsync(string shipmentId);
}