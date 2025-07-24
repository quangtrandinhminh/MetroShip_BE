using MetroShip.Repository.Base;
using MetroShip.Repository.Models;

namespace MetroShip.Repository.Interfaces;

public interface IMetroLineRepository : IBaseRepository<MetroLine>
{
    Task<List<MetroLine>> GetAllWithBasePriceByRegionAsync(string? regionId);
}