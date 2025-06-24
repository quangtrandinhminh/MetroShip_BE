using MetroShip.Repository.Base;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Enums;
using MetroShip.Utility.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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
        var configValue = _context.SystemConfigs.FirstOrDefault(
            x => x.ConfigKey == key && x.IsActive && x.DeletedAt == null
            )?.ConfigValue;
        if (string.IsNullOrEmpty(configValue))
        {
            throw new AppException(
                ErrorCode.SystemConfigNotFound, 
                $"System config with key {key} not found.",
                StatusCodes.Status500InternalServerError);
        }
        return configValue;
    }

    // filter by IsActive and Type
    public async Task<List<SystemConfig>> GetAllSystemConfigs(
        ConfigTypeEnum? type = null,
        string[]? ConfigKeys = null
        , bool? isActive = null)
    {
        var query = _context.SystemConfigs.AsNoTracking()
            .Where(x => x.DeletedAt == null);

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }
        else
        {
            query = query.Where(x => x.IsActive);
        }

        if (ConfigKeys != null && ConfigKeys.Any())
        {
            query = query.Where(x => ConfigKeys.Contains(x.ConfigKey));
            // check if all keys exist
            var missingKeys = ConfigKeys.Except(query.Select(x => x.ConfigKey)).ToList();
            if (missingKeys.Any())
            {
                throw new AppException(
                ErrorCode.SystemConfigNotFound,
                $"System config keys not found: {string.Join(", ", missingKeys)}",
                StatusCodes.Status500InternalServerError);
            }
        }

        if (type.HasValue)
        {
            query = query.Where(x => x.ConfigType == type.Value);
        }

        return await query.ToListAsync();
    }
}