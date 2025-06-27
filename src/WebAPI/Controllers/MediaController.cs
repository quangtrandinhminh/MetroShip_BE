using MetroShip.Service.ApiModels;
using MetroShip.Service.Interfaces;
using MetroShip.Utility.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MetroShip.WebAPI.Controllers
{
    [ApiController]
    [Authorize]
    public class MediaController(IServiceProvider serviceProvider) : ControllerBase
    {
        private readonly ICloudinaryService _cloudinaryService = 
            serviceProvider.GetRequiredService<ICloudinaryService>();

        [HttpPost(WebApiEndpoint.MediaEndpoint.UploadImage)]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
        {
            var imageUrl = await _cloudinaryService.UploadImageAsync(file);
            return Ok(BaseResponse.OkResponseDto(imageUrl));
        }

        [HttpDelete(WebApiEndpoint.MediaEndpoint.DeleteImage)]
        public async Task<IActionResult> DeleteImage([FromQuery] string imageUrl)
        {
            await _cloudinaryService.DeleteImageAsync(imageUrl);
            return Ok(BaseResponse.OkResponseDto(ResponseMessageConstantsCommon.SUCCESS));
        }
    }
}
