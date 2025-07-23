using System.Linq.Expressions;
using MetroShip.Utility.Enums;

namespace MetroShip.Utility.Helpers
{
    public static class DataHelper
    {
        public static int GenerateTrackingCode(Expression expression)
        {
            var random = new Random();
            return random.Next(100000, 999999);
        }

        public static string GenerateTrackingCodeString(Expression expression)
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public static MediaTypeEnum IsImage(string mediaUrl)
        {
            if (mediaUrl.Contains("image"))
            {
                return MediaTypeEnum.Image;
            }

            if (mediaUrl.Contains("video"))
            {
                return MediaTypeEnum.Video;
            }

            if (mediaUrl.Contains("raw"))
            {
                return MediaTypeEnum.Raw;
            }

            return MediaTypeEnum.Other;
        }
    }
}
