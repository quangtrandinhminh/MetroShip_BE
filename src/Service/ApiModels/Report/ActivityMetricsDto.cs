using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.ApiModels.Report
{
    public class ActivityMetricsDto
    {
        public RevenueFilterType FilterType { get; set; }
        public string FilterTypeName { get; set; }

        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }

        public int TotalOrders { get; set; }           // tổng đơn trong khoảng
        public int SuccessfulOrders { get; set; }      // đơn thành công (Completed / Compensated / ...)
        public int UnsuccessfulOrders { get; set; }    // đơn huỷ / không thành công

        public int TotalFeedbacks { get; set; }        // tổng feedback (rating != null)
        public int GoodFeedbacks { get; set; }         // feedback 4★ or 5★ (rating >= 4)
        public double SatisfactionPercent { get; set; } // = GoodFeedbacks / TotalFeedbacks * 100

    }

}
