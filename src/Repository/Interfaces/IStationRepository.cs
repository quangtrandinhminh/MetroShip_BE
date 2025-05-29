using MetroShip.Repository.Base;
using MetroShip.Repository.Models;

namespace MetroShip.Repository.Interfaces;

public interface IStationRepository : IBaseRepository<Station>
{
    IEnumerable<Station> GetStationsByRegion(string? regionId);
}