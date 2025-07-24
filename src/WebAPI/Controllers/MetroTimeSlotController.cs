using MetroShip.Service.ApiModels;
using MetroShip.Service.Interfaces;
using MetroShip.Utility.Constants;
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
    }
}
