using MetroShip.Service.ApiModels;
using MetroShip.Service.Interfaces;
using MetroShip.Utility.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MetroShip.WebAPI.Controllers
{
    [ApiController]
    public class MetroLineController : ControllerBase
    {
        private readonly IMetroLineService _metroLineService;

        public MetroLineController(IMetroLineService metroLineService)
        {
            _metroLineService = metroLineService;
        }

        [HttpGet(WebApiEndpoint.MetroLine.GetMetroLines)]
        public async Task<IActionResult> GetAllMetroLinesAsync()
        {
            var metroLines = await _metroLineService.GetAllMetroLine();
            return Ok(BaseResponse.OkResponseDto(metroLines));
        }
        [HttpGet(WebApiEndpoint.MetroLine.GetMetroLinesByRegion)]
        public async Task<IActionResult> GetAllMetroLinesByRegionAsync([FromQuery] string? regionId, [FromQuery] string? regionCode)
        {
            var metroLines = await _metroLineService.GetAllMetroLineByRegion(regionId, regionCode);
            return Ok(BaseResponse.OkResponseDto(metroLines));
        }
    }
}
