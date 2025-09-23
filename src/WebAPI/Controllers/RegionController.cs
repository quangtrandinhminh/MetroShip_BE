using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Region;
using MetroShip.Service.Interfaces;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MetroShip.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    public class RegionController(IServiceProvider serviceProvider) : ControllerBase
    {
        private readonly IRegionService _regionService = serviceProvider.GetRequiredService<IRegionService>();

        [HttpGet(WebApiEndpoint.RegionEndpoint.GetRegions)]
        public async Task<IActionResult> GetRegionsAsync([FromQuery] PaginatedListRequest request)
        {
            var regions = await _regionService.GetAllRegionsAsync(request);
            return Ok(BaseResponse.OkResponseDto(regions));
        }

        /*[HttpGet(WebApiEndpoint.RegionEndpoint.GetRegionById)]
        public async Task<IActionResult> GetRegionByIdAsync([FromRoute] string id)
        {
            var region = await _regionService.GetRegionByIdAsync(id);
            return Ok(BaseResponse.OkResponseDto(region));
        }*/

        [HttpPost(WebApiEndpoint.RegionEndpoint.CreateRegion)]
        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        public async Task<IActionResult> CreateRegionAsync([FromBody] CreateRegionRequest request)
        {
            var result = await _regionService.CreateRegionAsync(request);
            return Created(nameof(CreateRegionAsync), BaseResponse.OkResponseDto(result, null));
        }

        [HttpPut(WebApiEndpoint.RegionEndpoint.UpdateRegion)]
        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        public async Task<IActionResult> UpdateRegionAsync([FromBody] UpdateRegionRequest request)
        {
            var result = await _regionService.UpdateRegionAsync(request);
            return Ok(BaseResponse.OkResponseDto(result, null));
        }
    }
}
