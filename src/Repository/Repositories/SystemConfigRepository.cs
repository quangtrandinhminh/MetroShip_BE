using MetroShip.Repository.Base;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Exceptions;
using Microsoft.AspNetCore.Http;
using AppDbContext = MetroShip.Repository.Infrastructure.AppDbContext;

namespace MetroShip.Repository.Repositories;

public class SystemConfigRepository : BaseRepository<SystemConfig>, ISystemConfigRepository
{
    private readonly AppDbContext _context;
    public SystemConfigRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public string GetSystemConfigValueByKey(string key)
    {
        var configValue = _context.SystemConfigs.FirstOrDefault(x => x.ConfigKey == key).ConfigValue;
        if (string.IsNullOrEmpty(configValue))
        {
            throw new AppException(
                ErrorCode.SystemConfigNotFound, 
                $"System config with key {key} not found.",
                StatusCodes.Status500InternalServerError);
        }
        return configValue;
    }
}