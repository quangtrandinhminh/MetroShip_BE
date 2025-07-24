using MetroShip.Repository.Base;
using MetroShip.Repository.Models;

namespace MetroShip.Repository.Interfaces;

public interface IStationRepository : IBaseRepository<Station>
{
    Task<List<string>> GetAllStationIdNearUser(double userLatitude,
        double userLongitude,
        int maxDistanceInMeters = 1000,
        int maxCount = 10);
        
    IEnumerable<Station> GetStationsByRegion(string? regionId);
    bool AreStationsInSameMetroLine(string departureStationId, string destinationStationId);
}