using MetroShip.Service.ApiModels.PaginatedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.ApiModels.Transaction
{
    public class TransactionListWithStatsResponse
    {
        public PaginatedListResponse<TransactionResponse> Transactions { get; set; }
        public int TotalTransactions { get; set; }
        public double PercentageNewTransactions { get; set; }

        public int TotalPaidTransactions { get; set; }
        public double PercentageNewPaidTransactions { get; set; }
        public decimal TotalPaiddAmount { get; set; } // 💰 tổng tiền paid

        public int TotalUnpaidTransactions { get; set; }
        public double PercentageUnpaidTransactions { get; set; }

        public int TotalPendingTransactions { get; set; }
        public double PercentagePendingTransactions { get; set; }

        public int TotalCancelledTransactions { get; set; }
        public double PercentageCancelledTransactions { get; set; }
    }
}
