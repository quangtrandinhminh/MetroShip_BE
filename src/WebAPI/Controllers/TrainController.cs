using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Train;
using MetroShip.Service.Interfaces;
using MetroShip.Utility.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MetroShip.WebAPI.Controllers
{
    [ApiController]
    public class TrainController(IServiceProvider serviceProvider) : ControllerBase
    {
        private readonly ITrainService _trainService = serviceProvider.GetRequiredService<ITrainService>();

        [HttpGet]
        [Route(WebApiEndpoint.MetroTrainEndpoint.GetAllTrains)]
        public async Task<IActionResult> GetTrainsAsync([FromQuery] TrainListFilterRequest request)
        {
            var response = await _trainService.PaginatedListResponse(request);
            var additionalData = await _trainService.GetTrainSystemConfigAsync();
            return Ok(BaseResponse.OkResponseDto(response, additionalData));
        }

        [Authorize]
        [HttpGet(WebApiEndpoint.MetroTrainEndpoint.GetTrainsByLineSlotAndDate)]
        public async Task<IActionResult> GetAllTrainsByLineSlotDateAsync(LineSlotDateFilterRequest request)
        {
            var response = await _trainService.GetAllTrainsByLineSlotDateAsync(request);
            var additionalData = await _trainService.GetTrainSystemConfigAsync();
            return Ok(BaseResponse.OkResponseDto(response, additionalData));
        }

        [Authorize]
        [HttpPost(WebApiEndpoint.MetroTrainEndpoint.AddShipmentItinerariesForTrain)]
        public async Task<IActionResult> AddShipmentItinerariesForTrainAsync(
                       [FromBody] AddTrainToItinerariesRequest request)
        {
            var response = await _trainService.AddShipmentItinerariesForTrain(request);
            return Ok(BaseResponse.OkResponseDto(response, null));
        }
    }
}
