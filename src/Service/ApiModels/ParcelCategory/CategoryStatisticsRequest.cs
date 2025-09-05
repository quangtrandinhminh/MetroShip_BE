using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.ApiModels.ParcelCategory
{
    public enum RangeType
    {
        Today,
        ThisWeek,
        ThisMonth,
        Year
    }

    public class CategoryStatisticsRequest
    {
        public RangeType RangeType { get; set; } = RangeType.Year;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
