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
        [RequestSizeLimit(10_000_000)] // 10MB limit for single file
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
        {
            var imageUrl = await _cloudinaryService.UploadImageAsync(file);
            return Ok(BaseResponse.OkResponseDto(imageUrl));
        }

        [HttpPost(WebApiEndpoint.MediaEndpoint.UploadMultipleImages)]
        [RequestSizeLimit(100_000_000)] // 100MB limit for multiple files
        public async Task<IActionResult> UploadImage([FromForm] IFormFileCollection files)
        {
            var imageUrl = await _cloudinaryService.UploadMultipleImagesAsync(files);
            return Ok(BaseResponse.OkResponseDto(imageUrl));
        }

        [HttpPost(WebApiEndpoint.MediaEndpoint.UploadVideo)]
        [RequestSizeLimit(100_000_000)] // 100MB limit
        public async Task<IActionResult> UploadVideo([FromForm] IFormFile file)
        {
            var videoUrl = await _cloudinaryService.UploadVideoAsync(file);
            return Ok(BaseResponse.OkResponseDto(videoUrl));
        }

        [HttpPost(WebApiEndpoint.MediaEndpoint.UploadMultipleVideos)]
        [RequestSizeLimit(500_000_000)] // 500MB limit for multiple files
        public async Task<IActionResult> UploadMultipleVideos([FromForm] IFormFileCollection files)
        {
            var videoUrls = await _cloudinaryService.UploadMultipleVideosAsync(files);
            return Ok(BaseResponse.OkResponseDto(videoUrls));
        }

        [HttpPost(WebApiEndpoint.MediaEndpoint.UploadRawFile)]
        [RequestSizeLimit(50_000_000)] // 50MB limit for raw files
        public async Task<IActionResult> UploadRawFile([FromForm] IFormFile file)
        {
            var rawFileUrl = await _cloudinaryService.UploadRawFileAsync(file);
            return Ok(BaseResponse.OkResponseDto(rawFileUrl));
        }

        [HttpPost(WebApiEndpoint.MediaEndpoint.UploadMultipleRawFiles)]
        [RequestSizeLimit(200_000_000)] // 200MB limit for multiple raw files
        public async Task<IActionResult> UploadMultipleRawFiles([FromForm] IFormFileCollection files)
        {
            var rawFileUrls = await _cloudinaryService.UploadMultipleRawFilesAsync(files);
            return Ok(BaseResponse.OkResponseDto(rawFileUrls));
        }

        [HttpDelete(WebApiEndpoint.MediaEndpoint.DeleteResource)]
        public async Task<IActionResult> DeleteImage([FromQuery] string resourceUrl)
        {
            await _cloudinaryService.DeleteAsync(resourceUrl);
            return Ok(BaseResponse.OkResponseDto(ResponseMessageConstantsCommon.SUCCESS, null));
        }

        [HttpDelete(WebApiEndpoint.MediaEndpoint.DeleteMultipleResources)]
        public async Task<IActionResult> DeleteMultipleImages([FromBody] List<string> resourceUrls)
        {
            await _cloudinaryService.DeleteMultipleAsync(resourceUrls);
            return Ok(BaseResponse.OkResponseDto(ResponseMessageConstantsCommon.SUCCESS, null));
        }
    }
}
