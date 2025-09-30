using System.Linq.Expressions;
using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.SystemConfig;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Mapper;
using MetroShip.Utility.Config;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Exceptions;
using MetroShip.Utility.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace MetroShip.Service.Services;

public class SystemConfigService(IServiceProvider serviceProvider) : ISystemConfigService
{
    private readonly ISystemConfigRepository _systemConfigRepository = serviceProvider.GetRequiredService<ISystemConfigRepository>();
    private readonly IUnitOfWork _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
    private readonly IMapperlyMapper _mapper = serviceProvider.GetRequiredService<IMapperlyMapper>();
    private readonly IHttpContextAccessor _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();

    public async Task<PaginatedListResponse<SystemConfigResponse>> GetAllSystemConfigPaginated(PaginatedListRequest request, bool isIncludeDeactivated = false)
    {
        _logger.Information("Get all system config paginated: {@Request}", request);

        Expression<Func<SystemConfig, bool>> predicate = x => x.DeletedAt == null;
        if (!isIncludeDeactivated)
        {
            predicate = predicate.And(x => x.IsActive);
        }

        var configs = await _systemConfigRepository.GetAllPaginatedQueryable(
            request.PageNumber, request.PageSize, predicate);

        var result = _mapper.MapToSystemConfigPaginatedList(configs);
        return result;
    }

    public async Task<string> ChangeConfigValue(string configKey, string configValue)
    {
        _logger.Information("Change config value for {ConfigKey} to {ConfigValue}", configKey, configValue);
        ValidateConfigValue(configKey, configValue);
        var config = await _systemConfigRepository.GetSingleAsync(x => x.ConfigKey == configKey && x.IsActive && x.DeletedAt == null);
        if (config == null)
        {
            throw new AppException(
                ErrorCode.NotFound,
                ResponseMessageSystemConfig.CONFIG_NOT_FOUND,
                StatusCodes.Status400BadRequest);
        }

        if (config.ConfigValue == configValue)
        {
            return ResponseMessageSystemConfig.CONFIG_VALUE_SAME;
        }

        // deactivate old config
        config.IsActive = false;
        _systemConfigRepository.Update(config);

        // create new config with same key but new value
        var newConfig = new SystemConfig
        {
            ConfigKey = config.ConfigKey,
            ConfigValue = configValue,
            Description = config.Description,
            ConfigType = config.ConfigType,
            IsActive = true,
        };
        await _systemConfigRepository.AddAsync(newConfig);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);

        return ResponseMessageSystemConfig.CONFIG_UPDATE_SUCCESS;
    }

    private void ValidateConfigValue(string configKey, string configValue)
    {
        switch (configKey)
        {
            case nameof(SystemConfigSetting.MAX_DISTANCE_IN_METERS):
                if (!int.TryParse(configValue, out var maxDistance) || maxDistance <= 0)
                {
                    throw new AppException(
                    ErrorCode.BadRequest,
                    ResponseMessageSystemConfig.MAX_DISTANCE_INVALID,
                    StatusCodes.Status400BadRequest);
                }
                break;

            case nameof(SystemConfigSetting.MAX_CAPACITY_PER_LINE_KG):
                if (!int.TryParse(configValue, out var maxCapacityKg) || maxCapacityKg <= 0)
                {
                    throw new AppException(
                    ErrorCode.BadRequest,
                    ResponseMessageSystemConfig.MAX_CAPACITY_INVALID + "(kg)",
                    StatusCodes.Status400BadRequest);
                }
                break;

            case nameof(SystemConfigSetting.MAX_CAPACITY_PER_LINE_M3):
                if (!int.TryParse(configValue, out var maxCapacityM3) || maxCapacityM3 <= 0)
                {
                    throw new AppException(
                        ErrorCode.BadRequest,
                        ResponseMessageSystemConfig.MAX_CAPACITY_INVALID + "(m3)",
                        StatusCodes.Status400BadRequest);
                }
                break;

            case nameof(SystemConfigSetting.MAX_COUNT_STATION_NEAR_USER):
                if (!int.TryParse(configValue, out var maxCountStation) || maxCountStation <= 0)
                {
                    throw new AppException(
                    ErrorCode.BadRequest,
                    ResponseMessageSystemConfig.MAX_COUNT_STATION_INVALID,
                    StatusCodes.Status400BadRequest);
                }
                break;

            case nameof(SystemConfigSetting.MAX_SCHEDULE_SHIPMENT_DAY):
                if (!int.TryParse(configValue, out var maxScheduleDay) || maxScheduleDay <= 0)
                {
                    throw new AppException(
                    ErrorCode.BadRequest,
                    ResponseMessageSystemConfig.MAX_SCHEDULE_DAY_INVALID,
                    StatusCodes.Status400BadRequest);
                }
                break;

            case nameof(SystemConfigSetting.MAX_NUMBER_OF_SHIFT_ATTEMPTS):
                if (!int.TryParse(configValue, out var maxShiftAttempts) || maxShiftAttempts <= 0)
                {
                    throw new AppException(
                    ErrorCode.BadRequest,
                    ResponseMessageSystemConfig.MAX_SHIFT_ATTEMPTS_INVALID,
                    StatusCodes.Status400BadRequest);
                }
                break;

            default:
                throw new AppException(
                ErrorCode.BadRequest,
                ResponseMessageSystemConfig.CONFIG_KEY_INVALID,
                StatusCodes.Status400BadRequest);
        }
    }

    public async Task<string> UpdateSystemConfig(SystemConfigRequest request)
    {
        _logger.Information("Update system config: {@Request}", request);
        var existingConfig = await _systemConfigRepository.GetSingleAsync
            (x => x.Id == request.Id && x.IsActive && x.DeletedAt == null);
        if (existingConfig == null)
        {
            throw new AppException(
                ErrorCode.NotFound,
                ResponseMessageSystemConfig.CONFIG_NOT_FOUND,
                StatusCodes.Status400BadRequest);
        }

        _mapper.MapToSystemConfigEntity(request, existingConfig);
        _systemConfigRepository.Update(existingConfig);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);

        return ResponseMessageSystemConfig.CONFIG_UPDATE_SUCCESS;
    }

    // get config value by key
    public async Task<SystemConfigResponse> GetSystemConfigValueByKey(string configKey)
    {
        _logger.Information("Get system config value by key: {ConfigKey}", configKey);
        var config = await _systemConfigRepository.GetSingleAsync(x => x.ConfigKey == configKey && x.IsActive && x.DeletedAt == null);
        if (config == null)
        {
            throw new AppException(
            ErrorCode.NotFound,
            ResponseMessageSystemConfig.CONFIG_NOT_FOUND,
            StatusCodes.Status400BadRequest);
        }

        return _mapper.MapToSystemConfigResponse(config);
    }
}