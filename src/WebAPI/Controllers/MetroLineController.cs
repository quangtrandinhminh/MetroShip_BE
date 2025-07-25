﻿using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.MetroLine;
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

        [HttpGet(WebApiEndpoint.MetroLineEndpoint.GetMetroLinesDropdownList)]
        public async Task<IActionResult> GetAllMetroLinesAsync()
        {
            var metroLines = await _metroLineService.GetAllMetroLine();
            return Ok(BaseResponse.OkResponseDto(metroLines));
        }

        /*[HttpGet(WebApiEndpoint.MetroLine.GetMetroLinesByRegion)]
        public async Task<IActionResult> GetAllMetroLinesByRegionAsync([FromQuery] string? regionId)
        {
            var metroLines = await _metroLineService.GetAllMetroLineByRegion(regionId);
            return Ok(BaseResponse.OkResponseDto(metroLines));
        }*/

        [HttpPost(WebApiEndpoint.MetroLineEndpoint.CreateMetroLine)]
        public async Task<IActionResult> CreateMetroLineAsync([FromBody] MetroLineCreateRequest request)
        {
            var response = await _metroLineService.CreateMetroLine(request);
            return Created(nameof(CreateMetroLineAsync), BaseResponse.OkResponseDto(response));
        }
    }
}
