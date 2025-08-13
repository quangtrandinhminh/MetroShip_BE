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
        Year,
        Quarter,
        MonthRange
    }
    public class RevenueChartRequest
    {
        public RevenueFilterType FilterType { get; set; } = RevenueFilterType.Default;
        public int? Year { get; set; }           
        public int? Quarter { get; set; }        
        public int? StartYear { get; set; }      
        public int? StartMonth { get; set; }     
        public int? EndYear { get; set; }        
        public int? EndMonth { get; set; }      
    }
}
