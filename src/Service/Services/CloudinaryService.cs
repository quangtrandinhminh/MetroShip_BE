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
                Transformation = new Transformation().Quality(80).Crop("fit"),
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.SecureUrl.ToString();
        }
    }

    public async Task<List<string>> UploadMultipleImagesAsync(IFormFileCollection files)
    {
        _logger.Information($"Uploading {files.Count} images to Cloudinary");

        if (files == null || files.Count == 0)
        {
            throw new AppException(HttpResponseCodeConstants.BAD_REQUEST,
                "No files provided", StatusCodes.Status400BadRequest);
        }

        var uploadTasks = new List<Task<string>>();

        foreach (var file in files)
        {
            uploadTasks.Add(UploadImageAsync(file));
        }

        try
        {
            var results = await Task.WhenAll(uploadTasks);
            _logger.Information($"Successfully uploaded {results.Length} images");
            return results.ToList();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error uploading multiple images");
            throw;
        }
    }

    public async Task<string> UploadVideoAsync(IFormFile file)
    {
        _logger.Information("Uploading video to Cloudinary");
        VideoValidate(file);

        using (var stream = file.OpenReadStream())
        {
            var uploadParams = new VideoUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Transformation = new Transformation().Quality(80)
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.SecureUrl.ToString();
        }
    }

    public async Task<List<string>> UploadMultipleVideosAsync(IFormFileCollection files)
    {
        _logger.Information($"Uploading {files.Count} videos to Cloudinary");

        if (files == null || files.Count == 0)
        {
            throw new AppException(HttpResponseCodeConstants.BAD_REQUEST,
                "No files provided", StatusCodes.Status400BadRequest);
        }

        var uploadTasks = new List<Task<string>>();

        foreach (var file in files)
        {
            uploadTasks.Add(UploadVideoAsync(file));
        }

        try
        {
            var results = await Task.WhenAll(uploadTasks);
            _logger.Information($"Successfully uploaded {results.Length} videos");
            return results.ToList();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error uploading multiple videos");
            throw;
        }
    }

    public async Task<string> UploadRawFileAsync(IFormFile file)
    {
        _logger.Information("Uploading raw file to Cloudinary");
        RawFileValidate(file);

        using (var stream = file.OpenReadStream())
        {
            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                //PublicId = $"documents/{Path.GetFileNameWithoutExtension(file.FileName)}_{DateTime.UtcNow:yyyyMMddHHmmss}"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.SecureUrl.ToString();
        }
    }

    public async Task<List<string>> UploadMultipleRawFilesAsync(IFormFileCollection files)
    {
        _logger.Information($"Uploading {files.Count} raw files to Cloudinary");

        if (files == null || files.Count == 0)
        {
            throw new AppException(HttpResponseCodeConstants.BAD_REQUEST,
                "No files provided", StatusCodes.Status400BadRequest);
        }

        var uploadTasks = new List<Task<string>>();

        foreach (var file in files)
        {
            uploadTasks.Add(UploadRawFileAsync(file));
        }

        try
        {
            var results = await Task.WhenAll(uploadTasks);
            _logger.Information($"Successfully uploaded {results.Length} raw files");
            return results.ToList();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error uploading multiple raw files");
            throw;
        }
    }

    /*public Task DeleteImageAsync(string imageUrl)
    {
        _logger.Information($"Deleting image from Cloudinary. Url: {imageUrl}");
        var publicId = UrlValidate(imageUrl);
        var deleteParams = new DeletionParams(publicId);
        return _cloudinary.DestroyAsync(deleteParams);
    }*/

    public async Task DeleteAsync(string url)
    {
        _logger.Information($"Deleting file from Cloudinary. Url: {url}");

        var (publicId, resourceType) = UrlValidateAndDetectType(url);

        var deleteParams = new DeletionParams(publicId);

        if (resourceType == "video")
        {
            deleteParams.ResourceType = ResourceType.Video;
        }
        else if (resourceType == "raw")
        {
            deleteParams.ResourceType = ResourceType.Raw;
            deleteParams.Invalidate = true; // Invalidate cache for raw files
        }
        else
        {
            deleteParams.ResourceType = ResourceType.Image;
        }

        var result = await _cloudinary.DestroyAsync(deleteParams);
        if (result.Result != "ok")
        {
            _logger.Error($"Failed to delete {resourceType} file. Result: {result.Result}");
            throw new AppException(HttpResponseCodeConstants.INTERNAL_SERVER_ERROR,
                               $"Failed to delete {resourceType} file: {result.Result}",
                                              StatusCodes.Status500InternalServerError);
        }
        _logger.Information($"Successfully deleted {resourceType} file");
    }

    public async Task DeleteMultipleAsync(IEnumerable<string> urls)
    {
        _logger.Information($"Deleting multiple files from Cloudinary");

        var deleteTasks = urls.Select(url => DeleteAsync(url));

        try
        {
            await Task.WhenAll(deleteTasks);
            _logger.Information($"Successfully deleted {urls.Count()} files");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error deleting multiple files");
            throw;
        }
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

    /*private string UrlValidate(string imageUrl)
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
    }*/

    private void VideoValidate(IFormFile file)
    {
        // Danh sách định dạng video hợp lệ
        var validExtensions = new[] { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".webm", ".mkv", ".m4v" };

        // Kiểm tra null hoặc rỗng
        if (file == null || file.Length == 0)
        {
            throw new AppException(HttpResponseCodeConstants.NOT_FOUND,
                "Invalid video format (null)", StatusCodes.Status404NotFound);
        }

        // Lấy phần mở rộng file và kiểm tra định dạng
        var fileExtension = Path.GetExtension(file.FileName).ToLower();
        if (!validExtensions.Contains(fileExtension))
        {
            throw new AppException(HttpResponseCodeConstants.BAD_REQUEST,
                "Invalid video format (Accept: .mp4, .avi, .mov, .wmv, .flv, .webm, .mkv, .m4v)",
                StatusCodes.Status400BadRequest);
        }

        // Kiểm tra kích thước file (tối đa 50MB cho video)
        if (file.Length > 50 * 1024 * 1024)
        {
            throw new AppException(HttpResponseCodeConstants.BAD_REQUEST,
                "Video file too large (max 50MB)", StatusCodes.Status400BadRequest);
        }
    }

    private void RawFileValidate(IFormFile file)
    {
        var validExtensions = new[]
        {
            ".txt", ".doc", ".docx", ".pdf", ".xlsx", ".xls", ".ppt", ".pptx",
            ".rtf", ".odt", ".ods", ".odp", ".csv", ".xml", ".json", ".zip",
            ".rar", ".7z", ".tar", ".gz", ".sql", ".log", ".md", ".yaml", ".yml"
        };

        if (file == null || file.Length == 0)
        {
            throw new AppException(HttpResponseCodeConstants.NOT_FOUND,
                "Invalid file format (null)", StatusCodes.Status404NotFound);
        }

        var fileExtension = Path.GetExtension(file.FileName).ToLower();
        if (!validExtensions.Contains(fileExtension))
        {
            throw new AppException(HttpResponseCodeConstants.BAD_REQUEST,
                $"Invalid file format (Accept: {string.Join(", ", validExtensions)})",
                StatusCodes.Status400BadRequest);
        }

        // 50MB limit for documents
        if (file.Length > 50 * 1024 * 1024)
        {
            throw new AppException(HttpResponseCodeConstants.BAD_REQUEST,
                "File too large (max 50MB)", StatusCodes.Status400BadRequest);
        }
    }

    /*private string UrlValidate(string url, bool isVideo = false)
    {
        if (string.IsNullOrEmpty(url))
        {
            throw new AppException(HttpResponseCodeConstants.BAD_REQUEST,
                ResponseMessageConstrantsImage.INVALID_URL + "(null)",
                StatusCodes.Status400BadRequest);
        }

        // Parse the URL
        var uri = new Uri(url);

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

        // Validate path structure for images and videos
        var expectedResourceType = isVideo ? "video" : "image";
        if (segments.Length < 4 || segments[1] != expectedResourceType || segments[2] != "upload")
        {
            throw new AppException(HttpResponseCodeConstants.BAD_REQUEST,
                ResponseMessageConstrantsImage.INVALID_URL + $" (path structure for {expectedResourceType})",
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
    }*/

    /*private string UrlValidate(string url, string resourceType = "image")
    {
        if (string.IsNullOrEmpty(url))
        {
            throw new AppException(HttpResponseCodeConstants.BAD_REQUEST,
                ResponseMessageConstrantsImage.INVALID_URL + "(null)",
                StatusCodes.Status400BadRequest);
        }

        var uri = new Uri(url);

        var host = "res.cloudinary.com";
        if (!uri.Host.Equals(host, StringComparison.OrdinalIgnoreCase))
        {
            throw new AppException(HttpResponseCodeConstants.BAD_REQUEST,
                ResponseMessageConstrantsImage.INVALID_URL + $" Host required: {host}",
                StatusCodes.Status400BadRequest);
        }

        var cloudName = CloudinarySetting.Instance.CloudinaryUrl.Split("@").Last();
        var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2 || !segments[0].Equals(cloudName, StringComparison.OrdinalIgnoreCase))
        {
            throw new AppException(HttpResponseCodeConstants.BAD_REQUEST,
                ResponseMessageConstrantsImage.INVALID_URL + $" CloudName required: {cloudName}",
                StatusCodes.Status400BadRequest);
        }

        // Validate path structure for different resource types
        if (segments.Length < 4 || segments[1] != resourceType || segments[2] != "upload")
        {
            throw new AppException(HttpResponseCodeConstants.BAD_REQUEST,
                ResponseMessageConstrantsImage.INVALID_URL + $" (path structure for {resourceType})",
                StatusCodes.Status400BadRequest);
        }

        // For raw files, the public ID might include folders and extensions
        if (resourceType == "raw")
        {
            // Raw files keep their full path as public ID (including folders and extension)
            var publicIdSegments = segments.Skip(3).ToArray();
            return string.Join("/", publicIdSegments);
        }

        // For images and videos, extract public ID without extension
        var publicIdWithExtension = segments[^1];
        var publicId = Path.GetFileNameWithoutExtension(publicIdWithExtension);

        var isValidPublicId = !string.IsNullOrEmpty(publicId) &&
                              Regex.IsMatch(publicId, @"^[a-zA-Z0-9_\-]+$");

        if (!isValidPublicId)
        {
            throw new AppException(HttpResponseCodeConstants.BAD_REQUEST,
                ResponseMessageConstrantsImage.INVALID_URL + " (public ID)",
                StatusCodes.Status400BadRequest);
        }

        return publicId;
    }*/

    private (string publicId, string resourceType) UrlValidateAndDetectType(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            throw new AppException(HttpResponseCodeConstants.BAD_REQUEST,
            ResponseMessageConstrantsImage.INVALID_URL + "(null)",
            StatusCodes.Status400BadRequest);
        }

        // check if this is a valid URL
        if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            throw new AppException(HttpResponseCodeConstants.BAD_REQUEST,
            ResponseMessageConstrantsImage.INVALID_URL + " (invalid URL format)",
            StatusCodes.Status400BadRequest);
        }

        var uri = new Uri(url);

        var host = "res.cloudinary.com";
        if (!uri.Host.Equals(host, StringComparison.OrdinalIgnoreCase))
        {
            throw new AppException(HttpResponseCodeConstants.BAD_REQUEST,
                ResponseMessageConstrantsImage.INVALID_URL + $" Host required: {host}",
                StatusCodes.Status400BadRequest);
        }

        var cloudName = CloudinarySetting.Instance.CloudinaryUrl.Split("@").Last();
        var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2 || !segments[0].Equals(cloudName, StringComparison.OrdinalIgnoreCase))
        {
            throw new AppException(HttpResponseCodeConstants.BAD_REQUEST,
                ResponseMessageConstrantsImage.INVALID_URL + $" CloudName required: {cloudName}",
                StatusCodes.Status400BadRequest);
        }

        // Detect resource type from URL path
        if (segments.Length < 4 || segments[2] != "upload")
        {
            throw new AppException(HttpResponseCodeConstants.BAD_REQUEST,
                ResponseMessageConstrantsImage.INVALID_URL + " (invalid path structure)",
                StatusCodes.Status400BadRequest);
        }

        var resourceType = segments[1].ToLower();
        if (resourceType != "image" && resourceType != "video" && resourceType != "raw")
        {
            throw new AppException(HttpResponseCodeConstants.BAD_REQUEST,
                ResponseMessageConstrantsImage.INVALID_URL + " (unsupported resource type)",
                StatusCodes.Status400BadRequest);
        }

        string publicId;

        if (resourceType == "raw")
        {
            // For raw files, keep publicId with extension
            var publicIdWithExtension = segments[^1];
           publicId = Path.GetFileName(publicIdWithExtension);

            var isValidPublicId = !string.IsNullOrEmpty(publicId) &&
                                  Regex.IsMatch(publicId, @"^[a-zA-Z0-9_\-\.]+$");

            if (!isValidPublicId)
            {
                throw new AppException(HttpResponseCodeConstants.BAD_REQUEST,
                ResponseMessageConstrantsImage.INVALID_URL + " (invalid public ID - raw)",
                StatusCodes.Status400BadRequest);
            }
        }
        else
        {
            // For images and videos, extract public ID without extension
            var publicIdWithExtension = segments[^1];
           publicId = Path.GetFileNameWithoutExtension(publicIdWithExtension);
           
           var isValidPublicId = !string.IsNullOrEmpty(publicId) &&
                                 Regex.IsMatch(publicId, @"^[a-zA-Z0-9_\-]+$");
           
           if (!isValidPublicId)
           {
               throw new AppException(HttpResponseCodeConstants.BAD_REQUEST,
                   ResponseMessageConstrantsImage.INVALID_URL + " (invalid public ID - image/video)",
                   StatusCodes.Status400BadRequest);
           }
        }

        return (publicId, resourceType);
    }
}