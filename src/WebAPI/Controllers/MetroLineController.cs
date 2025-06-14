using MetroShip.Service.ApiModels;
using MetroShip.Service.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MetroShip.WebAPI.Controllers
{
    [Route("api/metro-lines")]
    [ApiController]
    public class MetroLineController : ControllerBase
    {
        private readonly IMetroLineService _metroLineService;

        public MetroLineController(IMetroLineService metroLineService)
        {
            _metroLineService = metroLineService;
        }

        [HttpGet("dropdown-list")]
        public async Task<IActionResult> GetAllMetroLinesAsync()
        {
            var metroLines = await _metroLineService.GetAllMetroLine();
            return Ok(BaseResponse.OkResponseDto(metroLines));
        }
    }
}
