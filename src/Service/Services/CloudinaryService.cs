using System.Text.RegularExpressions;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using MetroShip.Service.Interfaces;
using MetroShip.Utility.Config;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace MetroShip.Service.Services;

public class CloudinaryService(IServiceProvider serviceProvider) : ICloudinaryService
{
    private readonly Cloudinary _cloudinary = new Cloudinary(CloudinarySetting.Instance.CloudinaryUrl);
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();

    public async Task<string> UploadImageAsync(IFormFile file)
    {
        _logger.Information("Uploading image to Cloudinary");
        ImageValidate(file);
        using (var stream = file.OpenReadStream())
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Transformation = new Transformation().Quality(80).Crop("fit")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.SecureUrl.ToString();
        }
    }

    public Task DeleteImageAsync(string imageUrl)
    {
        _logger.Information($"Deleting image from Cloudinary. Url: {imageUrl}");
        var publicId = UrlValidate(imageUrl);
        var deleteParams = new DeletionParams(publicId);
        return _cloudinary.DestroyAsync(deleteParams);
    }

    private void ImageValidate(IFormFile file)
    {
        // Danh sách định dạng ảnh hợp lệ
        var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".jfif" };

        // Kiểm tra null hoặc rỗng
        if (file == null || file.Length == 0)
        {
            throw new AppException(HttpResponseCodeConstants.NOT_FOUND, ResponseMessageConstrantsImage.INVALID_FORMAT + "(null)",
                StatusCodes.Status404NotFound);
        }

        // Lấy phần mở rộng file và kiểm tra định dạng
        var fileExtension = Path.GetExtension(file.FileName).ToLower();
        if (!validExtensions.Contains(fileExtension))
        {
            throw new AppException(HttpResponseCodeConstants.BAD_REQUEST, ResponseMessageConstrantsImage.INVALID_FORMAT + "(Accept: .jpg, .jpeg, .png, .gif)",
                StatusCodes.Status400BadRequest);
        }

        // Kiểm tra kích thước file (tối đa 5MB)
        if (file.Length > 5 * 1024 * 1024)
        {
            throw new AppException(HttpResponseCodeConstants.BAD_REQUEST, ResponseMessageConstrantsImage.INVALID_SIZE,
                StatusCodes.Status400BadRequest);
        }
    }

    private string UrlValidate(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
        {
            throw new AppException(HttpResponseCodeConstants.BAD_REQUEST, ResponseMessageConstrantsImage.INVALID_URL + "(null)",
                               StatusCodes.Status400BadRequest);
        }

        // Parse the URL
        var uri = new Uri(imageUrl);

        // Validate hostname
        var host = "res.cloudinary.com";
        if (!uri.Host.Equals(host, StringComparison.OrdinalIgnoreCase))
        {
            throw new AppException(HttpResponseCodeConstants.BAD_REQUEST,
                ResponseMessageConstrantsImage.INVALID_URL + $" Host required: {host}",
                                              StatusCodes.Status400BadRequest);
        }

        // Validate that the cloud name matches (first segment in the path)
        var cloudName = CloudinarySetting.Instance.CloudinaryUrl.Split("@").Last();
        var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2 || !segments[0].Equals(cloudName, StringComparison.OrdinalIgnoreCase))
        {
            throw new AppException(HttpResponseCodeConstants.BAD_REQUEST,
                ResponseMessageConstrantsImage.INVALID_URL + $" CloudName required: {cloudName}",
                                                             StatusCodes.Status400BadRequest);
        }

        // Validate path structure (e.g., /image/upload/v1234567890/<public-id>)
        if (segments.Length < 4 || segments[1] != "image" || segments[2] != "upload")
        {
            throw new AppException(HttpResponseCodeConstants.BAD_REQUEST,
                ResponseMessageConstrantsImage.INVALID_URL + " (path structure)",
                StatusCodes.Status400BadRequest);
        }

        // Extract and validate public ID (last segment without extension)
        var publicIdWithExtension = segments[^1];
        var publicId = Path.GetFileNameWithoutExtension(publicIdWithExtension);

        // Ensure public ID is not empty and matches a safe pattern
        var isValidPublicId = !string.IsNullOrEmpty(publicId) &&
                              Regex.IsMatch(publicId, @"^[a-zA-Z0-9_\-]+$");

        if (!isValidPublicId)
        {
            throw new AppException(HttpResponseCodeConstants.BAD_REQUEST,
                ResponseMessageConstrantsImage.INVALID_URL + " (public ID)",
          StatusCodes.Status400BadRequest);
        }

        return publicId;
    }

}