using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.MetroLine;
using MetroShip.Service.Interfaces;
using MetroShip.Utility.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MetroShip.WebAPI.Controllers
{
    [ApiController]
    public class MetroRouteController : ControllerBase
    {
        private readonly IMetroRouteService _metroRouteService;

        public MetroRouteController(IMetroRouteService metroRouteService)
        {
            _metroRouteService = metroRouteService;
        }

        [HttpGet(WebApiEndpoint.MetroLineEndpoint.GetMetroLinesDropdownList)]
        public async Task<IActionResult> GetAllMetroRoutesDropdownAsync()
        {
            var response = await _metroRouteService.GetAllMetroRouteDropdown();
            return Ok(BaseResponse.OkResponseDto(response));
        }

        [HttpPost(WebApiEndpoint.MetroLineEndpoint.CreateMetroLine)]
        public async Task<IActionResult> CreateMetroRouteAsync([FromBody] MetroRouteRequest request)
        {
            var response = await _metroRouteService.CreateMetroRoute(request);
            return Created(nameof(CreateMetroRouteAsync), BaseResponse.OkResponseDto(response));
        }

        /*[HttpGet(WebApiEndpoint.MetroRouteEndpoint.GetMetroLines)]
        public async Task<IActionResult> GetAllMetroRoutesAsync()
        {
            var response = await _metroLineService.GetAllMetroRoutes();
            return Ok(BaseResponse.OkResponseDto(response));
        }*/

        /*[HttpGet(WebApiEndpoint.MetroLine.GetMetroLinesByRegion)]
        public async Task<IActionResult> GetAllMetroLinesByRegionAsync([FromQuery] string? regionId)
        {
            var metroLines = await _metroLineService.GetAllMetroLineByRegion(regionId);
            return Ok(BaseResponse.OkResponseDto(metroLines));
        }*/
    }
}
