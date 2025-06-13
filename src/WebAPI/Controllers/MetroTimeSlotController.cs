using MetroShip.Service.ApiModels;
using MetroShip.Service.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MetroShip.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetroTimeSlotController : ControllerBase
    {
        private readonly IMetroTimeSlotService _metroTimeSlotService;

        public MetroTimeSlotController(IMetroTimeSlotService metroTimeSlotService)
        {
            _metroTimeSlotService = metroTimeSlotService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMetroTimeSlotsAsync()
        {
            var timeSlots = await _metroTimeSlotService.GetAllForMetroTimeSlot();
            return Ok(BaseResponse.OkResponseDto(timeSlots));
        }
    }
}
