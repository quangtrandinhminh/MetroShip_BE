using System.ComponentModel.DataAnnotations;
using MetroShip.Utility.Helpers;

namespace MetroShip.Service.ApiModels.Statistic;

public class StatisticRequest
{
    public DateTimeOffset FromDateTimeOffset { get; set; } = CoreHelper.SystemTimeNow;

    [Range(1, 366)]
    public int DayNumber { get; set; } = 7;
}