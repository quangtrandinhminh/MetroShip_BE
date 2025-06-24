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

            var dLat = (lat2 - lat1) * Math.PI / 180;
            var dLon = (lon2 - lon1) * Math.PI / 180;

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return (decimal)(R * c); // Trả về khoảng cách tính bằng km
        }

        public static decimal CalculateChargeableWeight(
            decimal length, decimal width, decimal height, decimal weight)
        {
            decimal volume = length * width * height; // cm^3
            // Đơn vị tính là cm^3/kg, 5000 cho vận chuyển mặt đất
            decimal divisor = 5000; 
            decimal volumetricWeight = volume / divisor;
            decimal chargeableWeight = Math.Max(weight, volumetricWeight);

            return chargeableWeight;
        }
    }
}