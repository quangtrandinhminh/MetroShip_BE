using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Station;
using MetroShip.Service.Interfaces;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using MetroShip.Utility.Enums;

namespace MetroShip.WebAPI.Controllers
{
    [ApiController]
    [Authorize]
    public class StationController : ControllerBase
    {
        private readonly IStationService _stationService;

        public StationController(IStationService stationService)
        {
            _stationService = stationService;
        }

        [HttpGet(WebApiEndpoint.StationEndpoint.GetAllStations)]
        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        public async Task<IActionResult> GetAllStationsAsync([FromQuery] PaginatedListRequest request, [FromQuery] StationFilter filter)
        {
            var stations = await _stationService.GetAllStationAsync(request, filter);
            return Ok(BaseResponse.OkResponseDto(stations));
        }

        [HttpGet(WebApiEndpoint.StationEndpoint.GetDropdownList)]
        public async Task<IActionResult> GetAllStationsAsync([FromQuery] string? regionId)
        {
            var stations = await _stationService.GetAllStationsAsync(regionId);
            return Ok(BaseResponse.OkResponseDto(stations));
        }

        [HttpGet(WebApiEndpoint.StationEndpoint.GetStationById)]
        public async Task<IActionResult> GetStationById(Guid id)
        {
            var station = await _stationService.GetStationByIdAsync(id);
            if (station == null)
                return NotFound();

            return Ok(station);
        }

        [HttpPost(WebApiEndpoint.StationEndpoint.GetStationsNearUsers)]
        public async Task<IActionResult> GetStationsNearUsers([FromBody] NearbyStationsRequest request)
        {
            var stations = await _stationService.GetStationsNearUsers(request);
            return Ok(BaseResponse.OkResponseDto(stations));
        }

        [HttpPost(WebApiEndpoint.StationEndpoint.CalculateDistance)]
        [AllowAnonymous]
        public async Task<IActionResult> CalculateDistance([FromBody] DistanceRequest request)
        {
            var distance = CalculateHelper.CalculateDistanceBetweenTwoCoordinatesByHaversine(
                               request.FromLatitude, request.FromLongitude,
                                              request.ToLatitude, request.ToLongitude);
            return Ok(BaseResponse.OkResponseDto(distance));
        }

        [HttpPost(WebApiEndpoint.StationEndpoint.CreateStation)]
        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        public async Task<IActionResult> CreateStation([FromBody] CreateStationRequest request)
        {
            var createdStation = await _stationService.CreateStationAsync(request);
            return CreatedAtAction(nameof(GetStationById), new { id = createdStation.Id }, createdStation);
        }

        [HttpPut(WebApiEndpoint.StationEndpoint.UpdateStation)]
        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        public async Task<IActionResult> UpdateStation(Guid id, [FromBody] UpdateStationRequest request)
        {
            var updatedStation = await _stationService.UpdateStationAsync(id,request);
            if (updatedStation == null)
                return NotFound();

            return Ok(updatedStation);
        }

        [HttpDelete(WebApiEndpoint.StationEndpoint.DeleteStation)]
        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        public async Task<IActionResult> DeleteStation(Guid id)
        {
            await _stationService.DeleteStationAsync(id);
            return NoContent();
        }
    }
}
