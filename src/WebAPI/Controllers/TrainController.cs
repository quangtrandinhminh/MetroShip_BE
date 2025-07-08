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

        [Authorize(Roles = $"{nameof(UserRoleEnum.Staff)},{nameof(UserRoleEnum.Admin)}")]
        [HttpGet(WebApiEndpoint.MetroTrainEndpoint.GetAllTrains)]
        public async Task<IActionResult> GetAllTrainsAsync(
                       [FromQuery] PaginatedListRequest request,
                       [FromQuery] string? lineId = null)
        {
            var response = await _trainService.GetAllTrainsAsync(request, lineId);
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
    }
}
