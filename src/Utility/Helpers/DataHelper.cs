using System.Linq.Expressions;

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
    }
}
