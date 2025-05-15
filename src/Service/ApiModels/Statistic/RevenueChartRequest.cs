using System.ComponentModel.DataAnnotations;

namespace MetroShip.Service.ApiModels.Statistic;

public class RevenueChartRequest
{
    [Range(2024, 3000)]
    public int Year { get; set; }

    [Range(1, 12)]
    public int? Month { get; set; }
}