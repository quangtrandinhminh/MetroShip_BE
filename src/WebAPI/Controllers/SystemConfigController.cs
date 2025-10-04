using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.SystemConfig;
using MetroShip.Service.Interfaces;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MetroShip.WebAPI.Controllers
{
    [ApiController]
    public class SystemConfigController(IServiceProvider serviceProvider) : ControllerBase
    {
        private readonly ISystemConfigService _systemConfigService = serviceProvider.GetRequiredService<ISystemConfigService>();

        [HttpGet(WebApiEndpoint.SystemConfigEndpoint.GetSystemConfigs)]
        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        public async Task<IActionResult> GetAllSystemConfigPaginated([FromQuery] PaginatedListRequest request, [FromQuery] bool isIncludeDeactivated = false)
        {
            var result = await _systemConfigService.GetAllSystemConfigPaginated(request, isIncludeDeactivated);
            return Ok(BaseResponse.OkResponseDto(result));
        }

        [Authorize]
        [HttpGet(WebApiEndpoint.SystemConfigEndpoint.GetConfigValueByKey)]
        public async Task<IActionResult> GetSystemConfigValueByKey([FromRoute] string configKey)
        {
            var result = await _systemConfigService.GetSystemConfigValueByKey(configKey);
            return Ok(BaseResponse.OkResponseDto(result));
        }

        [HttpPost(WebApiEndpoint.SystemConfigEndpoint.ChangeConfigValue)]
        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        public async Task<IActionResult> ChangeConfigValue([FromBody] ChangeConfigValueRequest request)
        {
            var result = await _systemConfigService.ChangeConfigValue(request.ConfigKey, request.ConfigValue);
            return Ok(BaseResponse.OkResponseDto(result));
        }

        [HttpPut(WebApiEndpoint.SystemConfigEndpoint.UpdateSystemConfig)]
        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        public async Task<IActionResult> UpdateSystemConfig([FromBody] SystemConfigRequest request)
        {
            var result = await _systemConfigService.UpdateSystemConfig(request);
            return Ok(BaseResponse.OkResponseDto(result));
        }
    }
}
