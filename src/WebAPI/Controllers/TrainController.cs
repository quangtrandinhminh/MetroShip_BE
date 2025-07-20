using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Train;
using MetroShip.Service.Interfaces;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Enums;
using MetroShip.WebAPI.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace MetroShip.WebAPI.Controllers
{
    [ApiController]
    public class TrainController(IServiceProvider serviceProvider) : ControllerBase
    {
        private readonly ITrainService _trainService = serviceProvider.GetRequiredService<ITrainService>();
        private readonly IHubContext<TrackingHub> _hub = serviceProvider.GetRequiredService<IHubContext<TrackingHub>>();

        [Authorize]
        [HttpGet]
        [Route(WebApiEndpoint.MetroTrainEndpoint.GetAllTrains)]
        public async Task<IActionResult> GetTrainsAsync([FromQuery] TrainListFilterRequest request)
        {
            var response = await _trainService.PaginatedListResponse(request);
            var additionalData = await _trainService.GetTrainSystemConfigAsync();
            return Ok(BaseResponse.OkResponseDto(response, additionalData));
        }

        [Authorize(Roles = nameof(UserRoleEnum.Staff))]
        [HttpPost(WebApiEndpoint.MetroTrainEndpoint.SendLocation)]
        public async Task<IActionResult> SendLocation([FromBody] TrackingLocationUpdateDto location)
        {
            await _trainService.UpdateTrainLocationAsync(location.TrainId, location.Latitude, location.Longitude, location.StationId);
            var trackingCodes = await _trainService.GetTrackingCodesByTrainAsync(location.TrainId);

            foreach (var code in trackingCodes)
            {
                await _hub.Clients.Group(code).SendAsync("ReceiveLocationUpdate", location);
            }

            return Ok();
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
