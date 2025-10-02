using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.MetroTimeSlot;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.Interfaces;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MetroShip.WebAPI.Controllers
{
    [Route("api/time-slots")]
    [ApiController]
    public class MetroTimeSlotController : ControllerBase
    {
        private readonly IMetroTimeSlotService _metroTimeSlotService;

        public MetroTimeSlotController(IMetroTimeSlotService metroTimeSlotService)
        {
            _metroTimeSlotService = metroTimeSlotService;
        }

        [HttpGet(WebApiEndpoint.MetroTimeSlotEndpoint.GetMetroTimeSlots)]
        public async Task<IActionResult> GetAllMetroTimeSlotsAsync()
        {
            var timeSlots = await _metroTimeSlotService.GetAllForMetroTimeSlot();
            return Ok(BaseResponse.OkResponseDto(timeSlots));
        }

        [HttpGet(WebApiEndpoint.MetroTimeSlotEndpoint.GetAllMetroTimeSlots)]
        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        public async Task<IActionResult> GetAllMetroTimeSlots([FromQuery] PaginatedListRequest request)
        {
            var paginatedTimeSlots = await _metroTimeSlotService.GetAllMetroTimeSlot(request);
            return Ok(BaseResponse.OkResponseDto(paginatedTimeSlots));
        }

        [HttpPut(WebApiEndpoint.MetroTimeSlotEndpoint.UpdateMetroTimeSlot)]
        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        public async Task<IActionResult> UpdateMetroTimeSlot([FromBody] MetroTimeSlotUpdateRequest request)
        {
            var result = await _metroTimeSlotService.UpdateMetroTimeSlot(request);
            return Ok(BaseResponse.OkResponseDto(result, null));
        }
    }
}
