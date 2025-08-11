using MetroShip.Repository.Interfaces;
using MetroShip.Service.ApiModels.Shipment;
using MetroShip.Service.ApiModels.Transaction;
using MetroShip.Service.ApiModels.User;
using MetroShip.Service.Interfaces;
using MetroShip.Utility.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ILogger = Serilog.ILogger;

namespace MetroShip.Service.Services;

public class ReportService(IServiceProvider serviceProvider): IReportService
{
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
    private readonly IShipmentRepository _shipmentRepository = serviceProvider.GetRequiredService<IShipmentRepository>();
    private readonly IUserRepository _userRepository = serviceProvider.GetRequiredService<IUserRepository>();
    private readonly ITransactionRepository _transactionRepository = serviceProvider.GetRequiredService<ITransactionRepository>();

    public record RevenueChartRequest
    {
        public int? Year { get; set; }
        public int? FromMonth { get; set; }
        public int? FromYear { get; set; }
        public int? ToMonth { get; set; }
        public int? ToYear { get; set; }
    }

    public record RevenueChartResponse
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Revenue { get; set; }
    }

    /*public async Task<List<RevenueChartResponse>> GetRevenueChart(RevenueChartRequest request)
    {
        _logger.Information("Getting revenue chart for year {Year}, from month {FromMonth}, to month {ToMonth}",
                       request.Year, request.FromMonth, request.ToMonth);

        // get all shipments status is delivered or completed, which DeliveredAt in range of request.Year or request.FromYear to request.ToYear
        var revenueData = await _shipmentRepository.GetAllAsync(
        s => (s.Status == ShipmentStatusEnum.Delivered || s.Status == ShipmentStatusEnum.Completed) &&
        (request.Year.HasValue ? s.DeliveredAt.Year == request.Year.Value : true) &&
        (request.FromYear.HasValue ? s.DeliveredAt.Year >= request.FromYear.Value : true) &&
        (request.ToYear.HasValue ? s.DeliveredAt.Year <= request.ToYear.Value : true) &&
        (request.FromMonth.HasValue ? s.DeliveredAt.Month >= request.FromMonth.Value : true) &&
        (request.ToMonth.HasValue ? s.DeliveredAt.Month <= request.ToMonth.Value : true));

        return revenueData.Select(data => new RevenueChartResponse
        {
            Year = data.Year,
            Month = data.Month,
            Revenue = data.Revenue
        }).ToList();
    }*/

    public async Task<ShipmentListWithStatsResponse> GetShipmentStatsAsync()
    {
        var query = _shipmentRepository.GetAllWithCondition();
        var totalShipments = await query.CountAsync();

        var todayVietnamTime = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)).Date;
        var todayUtc = todayVietnamTime.ToUniversalTime();

        var newShipmentsCount = await query.CountAsync(s => s.CreatedAt >= todayUtc);
        var percentageNewShipments = totalShipments > 0
            ? Math.Round((double)newShipmentsCount / totalShipments * 100, 2)
            : 0;

        var totalCompleteShipments = await query.CountAsync(s => s.ShipmentStatus == ShipmentStatusEnum.Completed);
        var newCompleteShipmentsCount = await query.CountAsync(
            s => s.ShipmentStatus == ShipmentStatusEnum.Completed && s.CreatedAt >= todayUtc);
        var percentageNewCompleteShipments = totalCompleteShipments > 0
            ? Math.Round((double)newCompleteShipmentsCount / totalCompleteShipments * 100, 2)
            : 0;

        return new ShipmentListWithStatsResponse
        {
            TotalShipments = totalShipments,
            PercentageNewShipments = percentageNewShipments,
            TotalCompleteShipments = totalCompleteShipments,
            PercentageNewCompleteShipments = percentageNewCompleteShipments
        };
    }
    public async Task<UserListWithStatsResponse> GetUserStatsAsync()
    {
        var query = _userRepository.GetAllWithCondition()
            .Where(v => v.DeletedTime == null &&
                        v.UserRoles.Any(r => r.Role.Name == UserRoleEnum.Customer.ToString()));

        var totalUsersWithRoleUser = await query.CountAsync();

        var firstDayOfMonth = new DateTimeOffset(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var newUsersCount = await query.CountAsync(u => u.CreatedTime >= firstDayOfMonth);
        var percentageNewUsers = totalUsersWithRoleUser > 0
            ? Math.Round((double)newUsersCount / totalUsersWithRoleUser * 100, 2)
            : 0;

        return new UserListWithStatsResponse
        {
            TotalUsersWithRoleUser = totalUsersWithRoleUser,
            PercentageNewUsers = percentageNewUsers
        };
    }

    public async Task<TransactionListWithStatsResponse> GetTransactionStatsAsync()
    {
        var query = _transactionRepository.GetAllWithCondition()
            .Where(t => t.DeletedAt == null);

        var totalTransactions = await query.CountAsync();

        var firstDayOfMonth = new DateTimeOffset(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var newTransactionsCount = await query.CountAsync(t => t.CreatedAt >= firstDayOfMonth);
        var percentageNewTransactions = totalTransactions > 0
            ? Math.Round((double)newTransactionsCount / totalTransactions * 100, 2)
            : 0;

        var totalPaidTransactions = await query.CountAsync(t => t.PaymentStatus == PaymentStatusEnum.Paid);
        var totalPaidAmount = await query
            .Where(t => t.PaymentStatus == PaymentStatusEnum.Paid)
            .SumAsync(t => t.PaymentAmount);

        var newPaidTransactionsCount = await query.CountAsync(
            t => t.PaymentStatus == PaymentStatusEnum.Paid && t.CreatedAt >= firstDayOfMonth);
        var percentageNewPaidTransactions = totalPaidTransactions > 0
            ? Math.Round((double)newPaidTransactionsCount / totalPaidTransactions * 100, 2)
            : 0;

        var totalUnpaidTransactions = await query.CountAsync(t => t.PaymentStatus == PaymentStatusEnum.Failed);
        var percentageUnpaidTransactions = totalTransactions > 0
            ? Math.Round((double)totalUnpaidTransactions / totalTransactions * 100, 2)
            : 0;

        var totalPendingTransactions = await query.CountAsync(t => t.PaymentStatus == PaymentStatusEnum.Pending);
        var percentagePendingTransactions = totalTransactions > 0
            ? Math.Round((double)totalPendingTransactions / totalTransactions * 100, 2)
            : 0;

        var totalCancelledTransactions = await query.CountAsync(t => t.PaymentStatus == PaymentStatusEnum.Cancelled);
        var percentageCancelledTransactions = totalTransactions > 0
            ? Math.Round((double)totalCancelledTransactions / totalTransactions * 100, 2)
            : 0;

        return new TransactionListWithStatsResponse
        {
            TotalTransactions = totalTransactions,
            PercentageNewTransactions = percentageNewTransactions,

            TotalPaidTransactions = totalPaidTransactions,
            PercentageNewPaidTransactions = percentageNewPaidTransactions,
            TotalPaidAmount = totalPaidAmount,

            TotalUnpaidTransactions = totalUnpaidTransactions,
            PercentageUnpaidTransactions = percentageUnpaidTransactions,

            TotalPendingTransactions = totalPendingTransactions,
            PercentagePendingTransactions = percentagePendingTransactions,

            TotalCancelledTransactions = totalCancelledTransactions,
            PercentageCancelledTransactions = percentageCancelledTransactions
        };
    }
}