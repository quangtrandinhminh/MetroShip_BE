namespace MetroShip.Service.ApiModels.Statistic;

public class RevenueStatisticResponse
{
    public decimal TotalRevenue { get; set; }
    public int? ShopId { get; set; } = null;
    public int TotalOrderShop { get; set; }
    public DateTimeOffset From { get; set; }
    public DateTimeOffset To { get; set; }
}