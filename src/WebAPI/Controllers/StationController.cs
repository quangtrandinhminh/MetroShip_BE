﻿using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Station;
using MetroShip.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace MetroShip.WebAPI.Controllers
{
    [Route("api/stations")]
    [ApiController]
    public class StationController : ControllerBase
    {
        private readonly IStationService _stationService;

        public StationController(IStationService stationService)
        {
            _stationService = stationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStationsAsync([FromQuery] string? regionId)
        {
            var stations = await _stationService.GetAllStationsAsync(regionId);
            return Ok(BaseResponse.OkResponseDto(stations));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStationById(Guid id)
        {
            var station = await _stationService.GetStationByIdAsync(id);
            if (station == null)
                return NotFound();

            return Ok(station);
        }

        [HttpPost]
        public async Task<IActionResult> CreateStation([FromBody] CreateStationRequest request)
        {
            var createdStation = await _stationService.CreateStationAsync(request);
            return CreatedAtAction(nameof(GetStationById), new { id = createdStation.Id }, createdStation);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStation(Guid id, [FromBody] UpdateStationRequest request)
        {
            if (id.ToString() != request.StationId)
                return BadRequest("Mismatched station ID.");

            var updatedStation = await _stationService.UpdateStationAsync(id,request);
            if (updatedStation == null)
                return NotFound();

            return Ok(updatedStation);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStation(Guid id)
        {
            await _stationService.DeleteStationAsync(id);
            return NoContent();
        }
    }
}
