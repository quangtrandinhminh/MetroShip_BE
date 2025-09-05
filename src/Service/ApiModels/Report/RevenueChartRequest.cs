using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.ApiModels.Report
{
    public enum RevenueFilterType
    {
        Default,
        Day,
        Week,
        Year,
        Quarter,
        MonthRange
    }
    public class RevenueChartRequest
    {
        public RevenueFilterType? FilterType { get; set; }
        public int? Week { get; set; }
        public int? Year { get; set; }
        public int? Quarter { get; set; }
        public int? StartYear { get; set; }
        public int? StartMonth { get; set; }
        public int? EndYear { get; set; }
        public int? EndMonth { get; set; }
        public DateTime? Day { get; set; }
    }
}
