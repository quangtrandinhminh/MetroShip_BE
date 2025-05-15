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
    }
}