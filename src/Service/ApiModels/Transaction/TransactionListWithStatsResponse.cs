using MetroShip.Service.ApiModels.PaginatedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MetroShip.Service.Services.ReportService;

namespace MetroShip.Service.ApiModels.Transaction
{
    public class TransactionListWithStatsResponse
    {
        public int TotalTransactions { get; set; }
        public double PercentageNewTransactions { get; set; }

        public int TotalPaidTransactions { get; set; }
        public double PercentageNewPaidTransactions { get; set; }
        public decimal TotalPaidAmount { get; set; }

        public int TotalUnpaidTransactions { get; set; }
        public double PercentageUnpaidTransactions { get; set; }

        public int TotalPendingTransactions { get; set; }
        public double PercentagePendingTransactions { get; set; }

        public int TotalCancelledTransactions { get; set; }
        public double PercentageCancelledTransactions { get; set; }
    }
}
