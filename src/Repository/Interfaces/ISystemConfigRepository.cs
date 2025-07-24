using MetroShip.Repository.Base;
using MetroShip.Repository.Models;
using MetroShip.Utility.Enums;

namespace MetroShip.Repository.Interfaces;

public interface ISystemConfigRepository : IBaseRepository<SystemConfig>
{
    string GetSystemConfigValueByKey(string key);

    Task<List<SystemConfig>> GetAllSystemConfigs(
        ConfigTypeEnum? type = null,
        string[]? ConfigKeys = null
        , bool? isActive = null);
}