using MetroShip.Repository.Base;
using MetroShip.Repository.Models;

namespace MetroShip.Repository.Interfaces;

public interface ISystemConfigRepository : IBaseRepository<SystemConfig>
{
    string GetSystemConfigValueByKey(string key);
}