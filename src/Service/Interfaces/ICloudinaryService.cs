using Microsoft.AspNetCore.Http;

namespace MetroShip.Service.Interfaces;

public interface ICloudinaryService
{
    Task<string> UploadImageAsync(IFormFile file);
    Task DeleteImageAsync(string imageUrl);
}