using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.SystemConfig;

namespace MetroShip.Service.Interfaces;

public interface ISystemConfigService
{
    Task<PaginatedListResponse<SystemConfigResponse>> GetAllSystemConfigPaginated(PaginatedListRequest request, bool isIncludeDeactivated = false);
    Task<SystemConfigResponse> GetSystemConfigValueByKey(string configKey);
    Task<string> ChangeConfigValue(string configKey, string configValue);
    Task<string> UpdateSystemConfig(SystemConfigRequest request);
}