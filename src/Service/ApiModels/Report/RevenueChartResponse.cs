using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.ApiModels.Report
{
    public class RevenueChartResponse<T>
    {
        public RevenueFilterType FilterType { get; set; }
        public string FilterTypeName => FilterType.ToString();
        public int? Year { get; set; }
        public int? Quarter { get; set; }
        public int? Week { get; set; } // 🆕 thêm tuần
        public int? StartYear { get; set; }
        public int? StartMonth { get; set; }
        public int? EndYear { get; set; }
        public int? EndMonth { get; set; }
        public DateTime? WeekStartDate { get; set; } // 🆕 ngày bắt đầu tuần
        public DateTime? WeekEndDate { get; set; }   // 🆕 ngày kết thúc tuần
        public List<T> Data { get; set; } = new();
    }

    public record ShipmentDataItem
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int TotalShipments { get; set; }
        public int CompletedShipments { get; set; }
        public int ReturnedShipments { get; set; }
        public double OnTimeDeliveryRate { get; set; }
        public double SatisfactionRate { get; set; }
    }

    public record TransactionDataItem
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int? Day { get; set; }
        public int TotalTransactions { get; set; }

        // Chi tiết theo loại transaction
        public decimal ShipmentCost { get; set; }
        public decimal Surcharge { get; set; }
        public decimal Refund { get; set; }
        public decimal Compensation { get; set; }

        // Tổng hợp
        public decimal TotalIncome { get; set; }   // ShipmentCost + Surcharge
        public decimal TotalOutcome { get; set; }  // Refund + Compensation
        public decimal NetAmount { get; set; }     // Income - Outcome

        // Growth %
        public decimal NetGrowthPercent { get; set; }
    }

    public class ShipmentFeedbackDataItem
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int? Day { get; set; }

        // Shipment
        public int TotalShipments { get; set; }
        public int CompleteAndCompensatedCount { get; set; }
        public int CompletedWithCompensationCount { get; set; }
        public double CompleteAndCompensatedPercent { get; set; }
        public double CompletedWithCompensationPercent { get; set; }

        // Feedback
        public int TotalFeedbacks { get; set; }
        public int FiveStarFeedbacks { get; set; }
        public double FiveStarPercent { get; set; }
    }
}
