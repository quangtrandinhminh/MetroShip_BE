namespace MetroShip.Utility.Helpers
{
    public class CalculateHelper
    {
        public static decimal CalculateTotalRentPrice(decimal rentPrice, int quantity)
        {
            var totalRentPrice = rentPrice * quantity;
            return totalRentPrice;
        }

        public static decimal CalculateTax(decimal totalRentPrice, decimal taxRate)
        {
            var tax = totalRentPrice * taxRate;
            return tax;
        }

        public static decimal CalculateProvisionalTotal(decimal totalRentPrice, decimal totalDeposit, decimal totalShipFee)
        {
            var total = totalRentPrice + totalDeposit + totalShipFee;
            return total;
        }

        public static decimal CalculateTotal(decimal provisionalTotal, decimal totalDeposit, decimal totalShipFee,
            decimal totalDiscount)
        {
            var total = provisionalTotal - totalDiscount + totalDeposit + totalShipFee;
            return total;
        }

        public static decimal CalculateTotal(decimal provisionalTotal, decimal totalDiscount, decimal tax)
        {
            var total = provisionalTotal - totalDiscount + tax;
            return total;
        }

        public static decimal CalculateCommission(decimal rentPrice, decimal commission)
        {
            var totalPlatformFee = rentPrice * commission;
            return totalPlatformFee;
        }

        public static decimal CalculateDistanceBetweenTwoCoordinatesByHaversine(
            double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Bán kính Trái Đất tính bằng km

            // Chuyển đổi decimal deg sang radian + Tính chêch lệch delta vĩ độ và kinh độ
            var dLat = (lat2 - lat1) * Math.PI / 180;
            var dLon = (lon2 - lon1) * Math.PI / 180;

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return (decimal)(R * c); // Trả về khoảng cách tính bằng km
        }

        public static decimal CalculateDistanceBetweenTwoCoordinatesByEuclidean(
            double lat1, double lon1, double lat2, double lon2)
        {
            // Tính độ chênh lệch (delta) vĩ độ và kinh độ (decimal deg)
            double dLat = lat2 - lat1;
            double dLon = lon2 - lon1;

            // Tính khoảng cách Euclidean trên mặt phẳng lat/lon (đơn vị: decimal deg)
            double distanceInDegrees = Math.Sqrt(dLat * dLat + dLon * dLon);

            // Hằng số độ dài vĩ độ trung bình: 1 deg ~ 111.32 km
            // Đổi sang km: trung bình 1 độ vĩ độ ~ 111.32km, 1 độ kinh độ ~ 111.32km * cos(vĩ độ trung bình)
            // Để chính xác hơn, ta nên quy đổi từng hướng:
            double meanLat = (lat1 + lat2) / 2.0;
            double kmPerDegLat = 111.32; // km cho 1 độ vĩ độ
            double kmPerDegLon = 111.32 * Math.Cos(meanLat * Math.PI / 180); // km cho 1 độ kinh độ tại vĩ độ trung bình

            // Chuyển dLat và dLon sang km
            double dLatKm = dLat * kmPerDegLat;
            double dLonKm = dLon * kmPerDegLon;

            double distanceKm = Math.Sqrt(dLatKm * dLatKm + dLonKm * dLonKm);

            return (decimal)distanceKm; // Trả về khoảng cách tính bằng km
        }

        public static decimal CalculateChargeableWeight(
            decimal length, decimal width, decimal height, decimal weight)
        {
            decimal volume = length * width * height; // cm^3
            decimal divisor = 5000;  // Đơn vị tính là cm^3/kg, 5000 cho vận chuyển mặt đất
            decimal volumetricWeight = volume / divisor; // kg
            decimal chargeableWeight = Math.Max(weight, volumetricWeight);

            return chargeableWeight;
        }
    }
}