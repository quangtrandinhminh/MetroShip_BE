using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.PaginatedList;
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

        [Authorize]
        [HttpGet(WebApiEndpoint.MetroTrain.GetAllTrains)]
        public async Task<IActionResult> GetAllTrainsAsync(
                       [FromQuery] PaginatedListRequest request,
                       [FromQuery] string? lineId = null)
        {
            var response = await _trainService.GetAllTrainsAsync(request, lineId);
            var additionalData = await _trainService.GetTrainSystemConfigAsync();
            return Ok(BaseResponse.OkResponseDto(response, additionalData));
        }
    }
}
