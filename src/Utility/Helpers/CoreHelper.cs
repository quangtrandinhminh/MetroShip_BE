using Microsoft.AspNetCore.Http;
using TimeZoneConverter;
using MetroShip.Utility.Constants;

namespace MetroShip.Utility.Helpers
{
    public static class CoreHelper
    {
        private static IHttpContextAccessor? _contextAccessor;
        public static HttpContext CurrentHttpContext => Current;

        public static TimeZoneInfo SystemTimeZoneInfo => GetTimeZoneInfo(Formattings.TimeZone);

        public static DateTimeOffset SystemTimeNow => DateTimeOffset.UtcNow;

        public static DateTime UtcToSystemTime(this DateTimeOffset dateTimeOffsetUtc)
        {
            return dateTimeOffsetUtc.UtcDateTime.UtcToSystemTime();
        }

        public static DateTime UtcToSystemTime(this DateTime dateTimeUtc)
        {
            var dateTimeWithTimeZone = TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, SystemTimeZoneInfo);

            return dateTimeWithTimeZone;
        }
        public static TimeZoneInfo GetTimeZoneInfo(string timeZoneId)
        {
            return TZConvert.GetTimeZoneInfo(timeZoneId);
        }

        public static async Task<string> UploadFileReturnPathAsync(IFormFile formFile, string type, string fileName)
        {
            if (formFile.Length > 0)
            {
                var timeNow = DateTimeOffset.UtcNow;
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), timeNow.Year.ToString(), timeNow.Month.ToString(), type);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                var extentionFile = Path.GetExtension(formFile.FileName);
                var filePath = Path.Combine(folderPath, fileName + extentionFile);
                var count = 0;
                while (File.Exists(filePath))
                {
                    count = count + 1;
                    filePath = Path.Combine(folderPath, fileName + "_" + count + extentionFile);
                }
                using (var stream = File.Create(filePath))
                {
                    await formFile.CopyToAsync(stream);
                }
                return filePath;
            }
            return null;
        }

        public static HttpContext? Current => _contextAccessor?.HttpContext;
    }
}
