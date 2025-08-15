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
        public int? StartYear { get; set; }
        public int? StartMonth { get; set; }
        public int? EndYear { get; set; }
        public int? EndMonth { get; set; }
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
        public int TotalTransactions { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public decimal PaidAmountGrowthPercent { get; set; }
    }
}
