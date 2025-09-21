using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.MetroLine;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.Interfaces;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MetroShip.WebAPI.Controllers
{
    [ApiController]
    [Authorize]
    public class MetroRouteController : ControllerBase
    {
        private readonly IMetroRouteService _metroRouteService;

        public MetroRouteController(IMetroRouteService metroRouteService)
        {
            _metroRouteService = metroRouteService;
        }

        [HttpGet(WebApiEndpoint.MetroRouteEndpoint.GetMetroLinesDropdownList)]
        public async Task<IActionResult> GetAllMetroRoutesDropdownAsync([FromQuery] string? stationId)
        {
            var response = await _metroRouteService.GetAllMetroRouteDropdown(stationId);
            return Ok(BaseResponse.OkResponseDto(response));
        }

        [HttpPost(WebApiEndpoint.MetroRouteEndpoint.CreateMetroLine)]
        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        public async Task<IActionResult> CreateMetroRouteAsync([FromBody] MetroRouteRequest request)
        {
            var response = await _metroRouteService.CreateMetroRoute(request);
            return Created(nameof(CreateMetroRouteAsync), BaseResponse.OkResponseDto(response));
        }

        [HttpPost(WebApiEndpoint.MetroRouteEndpoint.ActivateMetroLine)]
        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        public async Task<IActionResult> ActivateMetroLineAsync([FromRoute] string id)
        {
            var response = await _metroRouteService.ActivateMetroLine(id);
            return Ok(BaseResponse.OkResponseDto(response));
        }

        [HttpGet(WebApiEndpoint.MetroRouteEndpoint.GetMetroLines)]
        public async Task<IActionResult> GetAllMetroRoutesAsync([FromQuery] PaginatedListRequest request
            , [FromQuery] MetroRouteFilterRequest filter)
        {
            var response = await _metroRouteService.GetAllMetroRoutes(request, filter);
            return Ok(BaseResponse.OkResponseDto(response));
        }

        [HttpGet(WebApiEndpoint.MetroRouteEndpoint.GetMetroLineById)]
        public async Task<IActionResult> GetMetroRouteByIdAsync([FromRoute] string id)
        {
            var response = await _metroRouteService.GetMetroRouteById(id);
            return Ok(BaseResponse.OkResponseDto(response));
        }

        [HttpGet]
        [Route(WebApiEndpoint.MetroRouteEndpoint.GetMetroLineWithStationsById)]
        public async Task<IActionResult> GetMetroLineWithStationsByIdAsync([FromRoute] string id)
        {
            var response = await _metroRouteService.GetMetroLineByIdAsync(id);
            return Ok(BaseResponse.OkResponseDto(response));
        }

        [HttpGet]
        [Route(WebApiEndpoint.MetroRouteEndpoint.GetAllActiveMetroLines)]
        public async Task<IActionResult> GetAllMetroLinesWithStationsAsync()
        {
            var response = await _metroRouteService.GetAllMetroLinesWithStationsAsync();
            return Ok(BaseResponse.OkResponseDto(response));
        }

        [HttpPut(WebApiEndpoint.MetroRouteEndpoint.UpdateMetroLine)]
        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        public async Task<IActionResult> UpdateMetroLineAsync([FromBody] MetroRouteUpdateRequest request)
        {
            var response = await _metroRouteService.UpdateMetroLine(request);
            return Ok(BaseResponse.OkResponseDto(response, null));
        }
    }
}
