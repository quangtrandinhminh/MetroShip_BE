using MetroShip.Repository.Base;
using MetroShip.Repository.Models;

namespace MetroShip.Repository.Interfaces;

public interface IMetroLineRepository : IBaseRepository<MetroLine>
{
    Task<IEnumerable<MetroLine>> GetAllWithBasePriceByRegionAsync(string? regionId, string? regionCode);
}