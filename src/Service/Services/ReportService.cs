using MetroShip.Repository.Interfaces;
using MetroShip.Service.ApiModels.ParcelCategory;
using MetroShip.Service.ApiModels.Report;
using MetroShip.Service.ApiModels.Shipment;
using MetroShip.Service.ApiModels.Transaction;
using MetroShip.Service.ApiModels.User;
using MetroShip.Service.Interfaces;
using MetroShip.Utility.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;
using ILogger = Serilog.ILogger;

namespace MetroShip.Service.Services;

public class ReportService(IServiceProvider serviceProvider): IReportService
{
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
    private readonly IShipmentRepository _shipmentRepository = serviceProvider.GetRequiredService<IShipmentRepository>();
    private readonly IUserRepository _userRepository = serviceProvider.GetRequiredService<IUserRepository>();
    private readonly ITransactionRepository _transactionRepository = serviceProvider.GetRequiredService<ITransactionRepository>();

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
        var totalCompensatedShipments = await query.CountAsync(s => s.ShipmentStatus == ShipmentStatusEnum.Compensated);
        var newCompleteShipmentsCount = await query.CountAsync(s => s.ShipmentStatus == ShipmentStatusEnum.Completed && s.CreatedAt >= todayUtc);
        var newCompensatedShipmentsCount = await query.CountAsync(s => s.ShipmentStatus == ShipmentStatusEnum.Compensated && s.CreatedAt >= todayUtc);
        var percentageNewCompleteShipments = totalCompleteShipments + totalCompensatedShipments > 0
            ? Math.Round((double)(newCompleteShipmentsCount + newCompensatedShipmentsCount) / (totalCompleteShipments + totalCompensatedShipments) * 100, 2)
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

        // ===== Thêm phần tính Growth so với tháng trước =====
        var prevMonthStart = firstDayOfMonth.AddMonths(-1);
        var prevMonthEnd = firstDayOfMonth.AddTicks(-1);

        decimal CalcGrowth(decimal current, decimal prev) =>
            prev > 0 ? Math.Round(((current - prev) / prev) * 100m, 2) : 0;

        var prevTotalTransactions = await query.CountAsync(t => t.CreatedAt >= prevMonthStart && t.CreatedAt <= prevMonthEnd);
        var prevTotalPaidAmount = await query
            .Where(t => t.PaymentStatus == PaymentStatusEnum.Paid &&
                        t.CreatedAt >= prevMonthStart && t.CreatedAt <= prevMonthEnd)
            .SumAsync(t => t.PaymentAmount);

        var growthTotalTransactions = CalcGrowth(newTransactionsCount, prevTotalTransactions);
        var growthPaidAmount = CalcGrowth(totalPaidAmount, prevTotalPaidAmount);

        return new TransactionListWithStatsResponse
        {
            // Giữ nguyên trường cũ
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
            PercentageCancelledTransactions = percentageCancelledTransactions,

            GrowthTotalTransactions = growthTotalTransactions,
            GrowthPaidAmount = growthPaidAmount
        };
    }

    public async Task<RevenueChartResponse<ShipmentDataItem>> GetShipmentChartAsync(RevenueChartRequest request)
    {
        var finalFilterType = request.FilterType ?? RevenueFilterType.Default;

        var query = _shipmentRepository.GetAllWithCondition();
        query = ApplyDateFilter(query, finalFilterType, request, s => s.CreatedAt);

        var data = await query
    .GroupBy(s => new { s.CreatedAt.Year, s.CreatedAt.Month })
    .Select(g => new ShipmentDataItem
    {
        Year = g.Key.Year,
        Month = g.Key.Month,
        TotalShipments = g.Count(),
        CompletedShipments = g.Count(s => s.ShipmentStatus == ShipmentStatusEnum.Completed),
        ReturnedShipments = g.Count(s => s.ShipmentStatus == ShipmentStatusEnum.Returned),
        OnTimeDeliveryRate = g.Count(s => s.ShipmentStatus == ShipmentStatusEnum.Completed) > 0
            ? Math.Round(
                g.Count(s => s.ShipmentStatus == ShipmentStatusEnum.Completed &&
                             s.CompletedAt <= s.StartReceiveAt) * 100.0 /
                Math.Max(1, g.Count(s => s.ShipmentStatus == ShipmentStatusEnum.Completed)), 2)
            : 0,
        SatisfactionRate = g.Any(s => s.Rating != null)
            ? Math.Round(g.Average(s => (double)s.Rating.Value) * 20, 2)
            : 0
    })
    .OrderBy(x => x.Year)
    .ThenBy(x => x.Month)
    .ToListAsync();

        return new RevenueChartResponse<ShipmentDataItem>
        {
            FilterType = finalFilterType,
            Year = request.Year,
            Quarter = request.Quarter,
            StartYear = request.StartYear,
            StartMonth = request.StartMonth,
            EndYear = request.EndYear,
            EndMonth = request.EndMonth,
            Data = data
        };
    }

    public async Task<RevenueChartResponse<TransactionDataItem>> GetTransactionChartAsync(RevenueChartRequest request)
    {
        var finalFilterType = request.FilterType ?? RevenueFilterType.Default;

        var query = _transactionRepository.GetAllWithCondition()
            .Where(t => t.DeletedAt == null);
        query = ApplyDateFilter(query, finalFilterType, request, t => t.CreatedAt);

        var data = await query
            .GroupBy(t => new { t.CreatedAt.Year, t.CreatedAt.Month })
            .Select(g => new TransactionDataItem
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalTransactions = g.Count(),
                TotalPaidAmount = g
                    .Where(t => t.PaymentStatus == PaymentStatusEnum.Paid)
                    .Sum(t => t.PaymentAmount)
            })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync();

        // === Thêm tính Growth % ===
        for (int i = 0; i < data.Count; i++)
        {
            var current = data[i];
            var prevYear = current.Month == 1 ? current.Year - 1 : current.Year;
            var prevMonth = current.Month == 1 ? 12 : current.Month - 1;

            var prev = data.FirstOrDefault(d => d.Year == prevYear && d.Month == prevMonth);
            if (prev != null && prev.TotalPaidAmount != 0)
            {
                current.PaidAmountGrowthPercent = Math.Round(
                    ((current.TotalPaidAmount - prev.TotalPaidAmount) / prev.TotalPaidAmount) * 100m, 2);
            }
            else
            {
                current.PaidAmountGrowthPercent = 0;
            }
        }

        return new RevenueChartResponse<TransactionDataItem>
        {
            FilterType = finalFilterType,
            Year = request.Year,
            Quarter = request.Quarter,
            StartYear = request.StartYear,
            StartMonth = request.StartMonth,
            EndYear = request.EndYear,
            EndMonth = request.EndMonth,
            Data = data
        };
    }

    public async Task<CategoryStatisticsResponse> GetCategoryStatisticsAsync(CategoryStatisticsRequest request)
    {
        var (startDate, endDate) = CalculateDateRange(request);

        var query = _shipmentRepository.GetAllWithCondition()
            .Where(s => s.CreatedAt >= startDate && s.CreatedAt <= endDate);

        var totalOrders = await query.CountAsync();

        var categoryData = await query
            .GroupBy(s => s.Parcels.Any() && s.Parcels.FirstOrDefault() != null && s.Parcels.FirstOrDefault()
            .CategoryInsurance.ParcelCategory != null
                ? s.Parcels.FirstOrDefault().CategoryInsurance.ParcelCategory.CategoryName
                : "Unknown")
            .Select(g => new
            {
                Name = g.Key,
                Orders = g.Count()
            })
            .ToListAsync();

        // Kỳ trước
        var durationDays = (endDate - startDate).TotalDays + 1;
        var previousStart = startDate.AddDays(-durationDays);
        var previousEnd = startDate.AddDays(-1);

        var prevQuery = _shipmentRepository.GetAllWithCondition()
            .Where(s => s.CreatedAt >= previousStart && s.CreatedAt <= previousEnd);

        var previousData = await prevQuery
            .GroupBy(s => s.Parcels.Any() && s.Parcels.FirstOrDefault() != null && s.Parcels.FirstOrDefault()
            .CategoryInsurance.ParcelCategory != null
                ? s.Parcels.FirstOrDefault().CategoryInsurance.ParcelCategory.CategoryName
                : "Unknown")
            .Select(g => new
            {
                Name = g.Key,
                Orders = g.Count()
            })
            .ToListAsync();

        var categories = categoryData.Select(cd =>
        {
            var prevOrders = previousData.FirstOrDefault(p => p.Name == cd.Name)?.Orders ?? 0;
            var growth = prevOrders > 0
                ? Math.Round((decimal)(cd.Orders - prevOrders) / prevOrders * 100, 2)
                : 0;

            return new CategoryStatsItem
            {
                Name = cd.Name,
                Orders = cd.Orders,
                Percentage = totalOrders > 0
                    ? Math.Round((decimal)cd.Orders / totalOrders * 100, 2)
                    : 0,
                Growth = growth
            };
        }).ToList();

        return new CategoryStatisticsResponse
        {
            RangeType = request.RangeType.ToString(),
            StartDate = startDate.DateTime,
            EndDate = endDate.DateTime,
            TotalOrders = totalOrders,
            Categories = categories
        };
    }

    public async Task<RevenueChartResponse<ShipmentFeedbackDataItem>> GetShipmentFeedbackChartAsync(RevenueChartRequest request)
    {
        var finalFilterType = request.FilterType ?? RevenueFilterType.Default;

        var query = _shipmentRepository.GetAllWithCondition()
            .Where(s => s.FeedbackAt != null); // chỉ lấy shipment có feedback

        query = ApplyDateFilter(query, finalFilterType, request, s => s.FeedbackAt.Value);

        var data = await query
            .GroupBy(s => new { s.FeedbackAt.Value.Year, s.FeedbackAt.Value.Month })
            .Select(g => new ShipmentFeedbackDataItem
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalFeedbacks = g.Count(),
                AverageRating = g.Any(s => s.Rating != null)
                    ? Math.Round(g.Average(s => (double)s.Rating.Value), 2)
                    : 0,
                PositiveFeedbackRate = g.Any(s => s.Rating != null)
                    ? Math.Round(g.Count(s => s.Rating >= 4) * 100.0 / g.Count(s => s.Rating != null), 2)
                    : 0,
                ResponseRate = g.Count(s => s.FeedbackResponse != null) > 0
                    ? Math.Round(g.Count(s => s.FeedbackResponse != null) * 100.0 / g.Count(), 2)
                    : 0,
                AvgResponseTimeHours = g.Count(s => s.FeedbackResponse != null) > 0
                    ? Math.Round(
                        g.Where(s => s.FeedbackRespondedAt != null)
                         .Average(s => (s.FeedbackRespondedAt.Value - s.FeedbackAt.Value).TotalHours), 2)
                    : 0
            })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync();

        return new RevenueChartResponse<ShipmentFeedbackDataItem>
        {
            FilterType = finalFilterType,
            Year = request.Year,
            Quarter = request.Quarter,
            StartYear = request.StartYear,
            StartMonth = request.StartMonth,
            EndYear = request.EndYear,
            EndMonth = request.EndMonth,
            Data = data
        };
    }

    #region Helper Methods
    private IQueryable<T> ApplyDateFilter<T>(
    IQueryable<T> query,
    RevenueFilterType filterType,
    RevenueChartRequest request,
    Expression<Func<T, DateTimeOffset>> dateSelector)
    {
        var propertyName = GetPropertyName(dateSelector);

        switch (filterType)
        {
            case RevenueFilterType.Year:
                if (request.Year.HasValue)
                {
                    query = query.Where(x =>
                        EF.Property<DateTimeOffset>(x, propertyName).Year == request.Year.Value);
                }
                break;

            case RevenueFilterType.Quarter:
                if (request.Year.HasValue && request.Quarter.HasValue)
                {
                    var startMonth = (request.Quarter.Value - 1) * 3 + 1;
                    var endMonth = startMonth + 2;

                    query = query.Where(x =>
                        EF.Property<DateTimeOffset>(x, propertyName).Year == request.Year.Value &&
                        EF.Property<DateTimeOffset>(x, propertyName).Month >= startMonth &&
                        EF.Property<DateTimeOffset>(x, propertyName).Month <= endMonth);
                }
                break;

            case RevenueFilterType.MonthRange:
                if (request.StartYear.HasValue && request.StartMonth.HasValue &&
                    request.EndYear.HasValue && request.EndMonth.HasValue)
                {
                    var start = new DateTimeOffset(
                        request.StartYear.Value,
                        request.StartMonth.Value,
                        1, 0, 0, 0, TimeSpan.Zero);

                    var end = new DateTimeOffset(
                        request.EndYear.Value,
                        request.EndMonth.Value,
                        DateTime.DaysInMonth(request.EndYear.Value, request.EndMonth.Value),
                        23, 59, 59, TimeSpan.Zero);

                    query = query.Where(x =>
                        EF.Property<DateTimeOffset>(x, propertyName) >= start &&
                        EF.Property<DateTimeOffset>(x, propertyName) <= end);
                }
                break;

            case RevenueFilterType.day: // ✅ thêm lọc theo ngày cụ thể
                if (request.Day.HasValue)
                {
                    var targetDate = request.Day.Value.Date;
                    var start = new DateTimeOffset(targetDate, TimeSpan.Zero);
                    var end = start.AddDays(1).AddTicks(-1);

                    query = query.Where(x =>
                        EF.Property<DateTimeOffset>(x, propertyName) >= start &&
                        EF.Property<DateTimeOffset>(x, propertyName) <= end);
                }
                break;

            case RevenueFilterType.Default:
            default:
                // Không filter gì cho Default
                break;
        }

        return query;
    }

    private string GetPropertyName<T, TProp>(Expression<Func<T, TProp>> expression)
    {
        if (expression.Body is MemberExpression member)
            return member.Member.Name;
        if (expression.Body is UnaryExpression unary && unary.Operand is MemberExpression memberExpr)
            return memberExpr.Member.Name;
        throw new InvalidOperationException("Invalid expression");
    }

    private (DateTimeOffset start, DateTimeOffset end) GetDateRangeFromRequest(RevenueChartRequest request, RevenueFilterType filterType)
    {
        switch (filterType)
        {
            case RevenueFilterType.Year:
                return (new DateTimeOffset(request.Year.Value, 1, 1, 0, 0, 0, TimeSpan.Zero),
                        new DateTimeOffset(request.Year.Value, 12, 31, 23, 59, 59, TimeSpan.Zero));

            case RevenueFilterType.Quarter:
                var startMonth = (request.Quarter.Value - 1) * 3 + 1;
                var endMonth = startMonth + 2;
                return (new DateTimeOffset(request.Year.Value, startMonth, 1, 0, 0, 0, TimeSpan.Zero),
                        new DateTimeOffset(request.Year.Value, endMonth,
                            DateTime.DaysInMonth(request.Year.Value, endMonth), 23, 59, 59, TimeSpan.Zero));

            case RevenueFilterType.MonthRange:
                return (new DateTimeOffset(request.StartYear.Value, request.StartMonth.Value, 1, 0, 0, 0, TimeSpan.Zero),
                        new DateTimeOffset(request.EndYear.Value, request.EndMonth.Value,
                            DateTime.DaysInMonth(request.EndYear.Value, request.EndMonth.Value), 23, 59, 59, TimeSpan.Zero));

            default:
                var now = DateTimeOffset.UtcNow;
                return (new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero),
                        new DateTimeOffset(now.Year, now.Month,
                            DateTime.DaysInMonth(now.Year, now.Month), 23, 59, 59, TimeSpan.Zero));
        }
    }

    private (DateTimeOffset startDate, DateTimeOffset endDate) CalculateDateRange(CategoryStatisticsRequest request)
    {
        var today = DateTimeOffset.UtcNow; // luôn UTC

        switch (request.RangeType)
        {
            case RangeType.Today:
                var startOfToday = new DateTimeOffset(today.Year, today.Month, today.Day, 0, 0, 0, TimeSpan.Zero);
                return (startOfToday, startOfToday.AddDays(1).AddTicks(-1));

            case RangeType.ThisWeek:
                var startOfTodayWeek = new DateTimeOffset(today.Year, today.Month, today.Day, 0, 0, 0, TimeSpan.Zero);
                var diff = (int)today.DayOfWeek;
                var weekStart = startOfTodayWeek.AddDays(-diff);
                var weekEnd = weekStart.AddDays(7).AddTicks(-1);
                return (weekStart, weekEnd);

            case RangeType.ThisMonth:
                var monthStart = new DateTimeOffset(today.Year, today.Month, 1, 0, 0, 0, TimeSpan.Zero);
                var monthEnd = monthStart.AddMonths(1).AddTicks(-1);
                return (monthStart, monthEnd);

            case RangeType.Year:
                var yearStart = new DateTimeOffset(today.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);
                var yearEnd = yearStart.AddYears(1).AddTicks(-1);
                return (yearStart, yearEnd);

            default:
                if (request.StartDate == null || request.EndDate == null)
                    throw new ArgumentException("StartDate and EndDate are required for custom date range.");

                var customStart = new DateTimeOffset(
                    request.StartDate.Value.Year,
                    request.StartDate.Value.Month,
                    request.StartDate.Value.Day,
                    0, 0, 0, TimeSpan.Zero);

                var customEnd = new DateTimeOffset(
                    request.EndDate.Value.Year,
                    request.EndDate.Value.Month,
                    request.EndDate.Value.Day,
                    23, 59, 59, TimeSpan.Zero);

                return (customStart, customEnd);
        }
    }
    #endregion
}