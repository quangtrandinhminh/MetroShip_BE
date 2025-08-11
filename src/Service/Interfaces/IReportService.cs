using MetroShip.Service.ApiModels.Shipment;
using MetroShip.Service.ApiModels.Transaction;
using MetroShip.Service.ApiModels.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.Interfaces
{
    public interface IReportService
    {
        Task<ShipmentListWithStatsResponse> GetShipmentStatsAsync();
        Task<UserListWithStatsResponse> GetUserStatsAsync();
        Task<TransactionListWithStatsResponse> GetTransactionStatsAsync();
    }
}
