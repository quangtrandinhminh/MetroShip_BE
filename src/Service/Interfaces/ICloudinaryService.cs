using Microsoft.AspNetCore.Http;

namespace MetroShip.Service.Interfaces;

public interface ICloudinaryService
{
    // Image methods
    Task<string> UploadImageAsync(IFormFile file);
    Task<List<string>> UploadMultipleImagesAsync(IFormFileCollection files);

    // Video methods
    Task<string> UploadVideoAsync(IFormFile file);
    Task<List<string>> UploadMultipleVideosAsync(IFormFileCollection files);

    // Raw file methods (documents)
    Task<string> UploadRawFileAsync(IFormFile file);
    Task<List<string>> UploadMultipleRawFilesAsync(IFormFileCollection files);

    // Unified delete methods
    Task DeleteAsync(string url);
    Task DeleteMultipleAsync(IEnumerable<string> urls);
}