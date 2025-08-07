using MetroShip.Repository.Base;
using MetroShip.Repository.Models;

namespace MetroShip.Repository.Interfaces;

public interface IMetroRouteRepository : IBaseRepository<MetroLine>
{
    Task<List<MetroLine>> GetAllWithBasePriceByRegionAsync(string? regionId);
}