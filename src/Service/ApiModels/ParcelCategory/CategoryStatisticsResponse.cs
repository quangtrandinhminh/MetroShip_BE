using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.ApiModels.ParcelCategory
{
    public class CategoryStatisticsResponse
    {
        public string RangeType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalOrders { get; set; }
        public List<CategoryStatsItem> Categories { get; set; } = new();
    }

    public class CategoryStatsItem
    {
        public string Name { get; set; }
        public int Orders { get; set; }
        public decimal Percentage { get; set; }
        public decimal Growth { get; set; } // % tăng trưởng so với kỳ trước
    }
}
