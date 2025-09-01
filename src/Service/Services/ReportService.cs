using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
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
        var totalCompletedWithCompensationShipments = await query.CountAsync(s => s.ShipmentStatus == ShipmentStatusEnum.CompletedWithCompensation);

        var newCompleteShipmentsCount = await query.CountAsync(s => s.ShipmentStatus == ShipmentStatusEnum.Completed && s.CreatedAt >= todayUtc);
        var newCompensatedShipmentsCount = await query.CountAsync(s => s.ShipmentStatus == ShipmentStatusEnum.Compensated && s.CreatedAt >= todayUtc);
        var newCompletedWithCompensationShipmentsCount = await query.CountAsync(s => s.ShipmentStatus == ShipmentStatusEnum.CompletedWithCompensation && s.CreatedAt >= todayUtc);

        var percentageNewCompleteShipments = totalCompleteShipments + totalCompensatedShipments + totalCompletedWithCompensationShipments > 0
            ? Math.Round((double)(newCompleteShipmentsCount + newCompensatedShipmentsCount + newCompletedWithCompensationShipmentsCount) / (totalCompleteShipments + totalCompensatedShipments + totalCompletedWithCompensationShipments) * 100, 2)
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
        var filterType = request.FilterType ?? RevenueFilterType.Default;

        var query = ApplyDateFilter(
            _transactionRepository.GetAllWithCondition().Where(t => t.DeletedAt == null),
            filterType,
            request,
            t => t.CreatedAt
        );

        // ⚡ GroupBy theo Year/Month
        var rawData = await query
            .GroupBy(t => new { Year = t.CreatedAt.Year, Month = t.CreatedAt.Month })
            .Select(g => new TransactionDataItem
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalTransactions = g.Count(),

                ShipmentCost = g.Where(t => t.TransactionType == TransactionTypeEnum.ShipmentCost
                                         && t.PaymentStatus == PaymentStatusEnum.Paid)
                                .Sum(t => t.PaymentAmount),

                Surcharge = g.Where(t => t.TransactionType == TransactionTypeEnum.Surcharge
                                       && t.PaymentStatus == PaymentStatusEnum.Paid)
                             .Sum(t => t.PaymentAmount),

                Refund = g.Where(t => t.TransactionType == TransactionTypeEnum.Refund
                                    && t.PaymentStatus == PaymentStatusEnum.Paid)
                          .Sum(t => t.PaymentAmount),

                Compensation = g.Where(t => t.TransactionType == TransactionTypeEnum.Compensation
                                          && t.PaymentStatus == PaymentStatusEnum.Paid)
                                .Sum(t => t.PaymentAmount),
            })
            .ToListAsync();

        List<TransactionDataItem> fullData;

        switch (filterType)
        {
            case RevenueFilterType.day:
                var targetYear = request.Year ?? DateTime.UtcNow.Year;
                var targetMonth = request.StartMonth ?? DateTime.UtcNow.Month;
                // Day -> chỉ lấy đúng tháng đó
                fullData = rawData
                    .Where(d => d.Year == targetYear && d.Month == targetMonth)
                    .ToList();
                break;

            case RevenueFilterType.Default: // ✅ Default = Year
            case RevenueFilterType.Year:
                var year = request.Year ?? DateTime.UtcNow.Year;
                fullData = Enumerable.Range(1, 12)
                    .Select(m => BuildTransactionItem(rawData, year, m))
                    .ToList();
                break;

            case RevenueFilterType.Quarter:
                var qYear = request.Year ?? DateTime.UtcNow.Year;
                var q = request.Quarter ?? 1;
                var monthsInQuarter = Enumerable.Range((q - 1) * 3 + 1, 3);
                fullData = monthsInQuarter
                    .Select(m => BuildTransactionItem(rawData, qYear, m))
                    .ToList();
                break;

            case RevenueFilterType.MonthRange:
                var startYear = request.StartYear ?? DateTime.UtcNow.Year;
                var startMonth = request.StartMonth ?? 1;
                var endYear = request.EndYear ?? startYear;
                var endMonth = request.EndMonth ?? 12;

                var months = Enumerable.Range(startYear * 12 + startMonth,
                    (endYear * 12 + endMonth) - (startYear * 12 + startMonth) + 1)
                    .Select(x => new
                    {
                        Year = (x - 1) / 12,
                        Month = (x - 1) % 12 + 1
                    });

                fullData = months
                    .Select(m => BuildTransactionItem(rawData, m.Year, m.Month))
                    .ToList();
                break;

            default:
                fullData = rawData;
                break;
        }

        // === Tính Income, Outcome, NetAmount ===
        foreach (var item in fullData)
        {
            item.TotalIncome = item.ShipmentCost + item.Surcharge;
            item.TotalOutcome = item.Refund + item.Compensation;
            item.NetAmount = item.TotalIncome - item.TotalOutcome;
        }

        // === Tính Growth % theo NetAmount ===
        for (int i = 0; i < fullData.Count; i++)
        {
            var current = fullData[i];
            var prevYear = current.Month == 1 ? current.Year - 1 : current.Year;
            var prevMonth = current.Month == 1 ? 12 : current.Month - 1;

            var prev = fullData.FirstOrDefault(d => d.Year == prevYear && d.Month == prevMonth);
            if (prev != null && prev.NetAmount != 0)
            {
                current.NetGrowthPercent = Math.Round(
                    ((current.NetAmount - prev.NetAmount) / prev.NetAmount) * 100m, 2);
            }
            else
            {
                current.NetGrowthPercent = 0;
            }
        }

        return new RevenueChartResponse<TransactionDataItem>
        {
            FilterType = filterType,
            Year = request.Year,
            Quarter = request.Quarter,
            StartYear = request.StartYear,
            StartMonth = request.StartMonth,
            EndYear = request.EndYear,
            EndMonth = request.EndMonth,
            Data = fullData
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
        var filterType = request.FilterType ?? RevenueFilterType.Default;

        var query = ApplyDateFilter(
            _shipmentRepository.GetAllWithCondition(),
            filterType,
            request,
            s => s.FeedbackAt
        );

        // ⚡ GroupBy theo Year/Month
        var rawData = await query
            .Where(s => s.FeedbackAt.HasValue)
            .GroupBy(s => new
            {
                Year = s.FeedbackAt.Value.Year,
                Month = s.FeedbackAt.Value.Month
            })
            .Select(g => new ShipmentFeedbackDataItem
            {
                Year = (int)g.Key.Year,
                Month = (int)g.Key.Month,

                TotalShipments = g.Count(),
                CompleteAndCompensatedCount = g.Count(s =>
                    s.ShipmentStatus == ShipmentStatusEnum.Completed ||
                    s.ShipmentStatus == ShipmentStatusEnum.Compensated),
                CompletedWithCompensationCount = g.Count(s =>
                    s.ShipmentStatus == ShipmentStatusEnum.CompletedWithCompensation),

                TotalFeedbacks = g.Count(s => s.Rating != null),
                FiveStarFeedbacks = g.Count(s => s.Rating == 5)
            })
            .ToListAsync();

        List<ShipmentFeedbackDataItem> fullData;

        switch (filterType)
        {
            case RevenueFilterType.day:
                // Day -> chỉ lấy đúng ngày, không fill đủ 12 tháng
                fullData = rawData;
                break;

            case RevenueFilterType.Default: // ✅ Default = Year
            case RevenueFilterType.Year:
                var year = request.Year ?? DateTime.UtcNow.Year;
                fullData = Enumerable.Range(1, 12)
                    .Select(m => BuildItem(rawData, year, m))
                    .ToList();
                break;

            case RevenueFilterType.Quarter:
                var qYear = request.Year ?? DateTime.UtcNow.Year;
                var q = request.Quarter ?? 1;
                var monthsInQuarter = Enumerable.Range((q - 1) * 3 + 1, 3);
                fullData = monthsInQuarter
                    .Select(m => BuildItem(rawData, qYear, m))
                    .ToList();
                break;

            case RevenueFilterType.MonthRange:
                var startYear = request.StartYear ?? DateTime.UtcNow.Year;
                var startMonth = request.StartMonth ?? 1;
                var endYear = request.EndYear ?? startYear;
                var endMonth = request.EndMonth ?? 12;

                var months = Enumerable.Range(startYear * 12 + startMonth,
                    (endYear * 12 + endMonth) - (startYear * 12 + startMonth) + 1)
                 .Select(x => new
                 {
                     Year = (x - 1) / 12,
                     Month = (x - 1) % 12 + 1
                 });

                fullData = months
                    .Select(m => BuildItem(rawData, m.Year, m.Month))
                    .ToList();
                break;

            default:
                fullData = rawData;
                break;
        }

        return new RevenueChartResponse<ShipmentFeedbackDataItem>
        {
            FilterType = filterType,
            Year = request.Year,
            Quarter = request.Quarter,
            StartYear = request.StartYear,
            StartMonth = request.StartMonth,
            EndYear = request.EndYear,
            EndMonth = request.EndMonth,
            Data = fullData
        };
    }

    public async Task<ActivityMetricsDto> GetActivityMetricsAsync(RevenueChartRequest request)
    {
        IQueryable<Shipment> q;

        // Lấy filterType từ request, nếu null thì mặc định = Day (Default)
        var finalFilterType = request?.FilterType ?? RevenueFilterType.Default;

        if (finalFilterType == RevenueFilterType.Default || finalFilterType == RevenueFilterType.day)
        {
            // ✅ Default = Today
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            q = _shipmentRepository.GetAllWithCondition()
                 .Where(s => s.CreatedAt >= today && s.CreatedAt < tomorrow);
        }
        else
        {
            // ✅ ApplyDateFilter
            q = _shipmentRepository.GetAllWithCondition();
            q = ApplyDateFilter(q, finalFilterType, request, s => s.CreatedAt);
        }

        // nhóm status
        var successfulStatuses = new[]
        {
        ShipmentStatusEnum.Completed,
        ShipmentStatusEnum.CompletedWithCompensation,
        ShipmentStatusEnum.Compensated
    };

        var unsuccessfulStatuses = new[]
        {
        ShipmentStatusEnum.Cancelled,
        ShipmentStatusEnum.Rejected,
        ShipmentStatusEnum.Unpaid,
        ShipmentStatusEnum.Refunded,
        ShipmentStatusEnum.NoDropOff,
        ShipmentStatusEnum.Returned,
        ShipmentStatusEnum.Expired
    };

        // aggregate query (theo filter đã áp)
        var agg = await q
            .GroupBy(_ => 1)
            .Select(g => new
            {
                PeriodStart = g.Min(s => s.CreatedAt),
                PeriodEnd = g.Max(s => s.CreatedAt),
                TotalOrders = g.Count(),
                SuccessfulOrders = g.Count(s => successfulStatuses.Contains(s.ShipmentStatus)),
                UnsuccessfulOrders = g.Count(s => unsuccessfulStatuses.Contains(s.ShipmentStatus)),
                TotalFeedbacks = g.Count(s => s.Rating != null),
                GoodFeedbacks = g.Count(s => s.Rating != null && s.Rating >= 4)
            })
            .FirstOrDefaultAsync();

        // ✅ Xác định khoảng thời gian theo FilterType (không phụ thuộc dữ liệu trong DB)
        DateTime periodStart;
        DateTime periodEnd;

        switch (finalFilterType)
        {
            case RevenueFilterType.day:
            case RevenueFilterType.Default:
                var today = DateTime.UtcNow.Date;
                periodStart = today;
                periodEnd = today.AddDays(1).AddTicks(-1);
                break;

            case RevenueFilterType.MonthRange:
                var startYear = request.StartYear ?? DateTime.UtcNow.Year;
                var startMonth = request.StartMonth ?? DateTime.UtcNow.Month;
                periodStart = new DateTime(startYear, startMonth, 1);

                var endYear = request.EndYear ?? startYear;
                var endMonth = request.EndMonth ?? startMonth;
                // Lấy ngày cuối cùng của endMonth
                var endMonthLastDay = DateTime.DaysInMonth(endYear, endMonth);
                periodEnd = new DateTime(endYear, endMonth, endMonthLastDay, 23, 59, 59);
                break;

            case RevenueFilterType.Quarter:
                var qYear = request.Year ?? DateTime.UtcNow.Year;
                var quarter = request.Quarter ?? ((DateTime.UtcNow.Month - 1) / 3 + 1);
                var qStartMonth = (quarter - 1) * 3 + 1;
                periodStart = new DateTime(qYear, qStartMonth, 1);
                periodEnd = periodStart.AddMonths(3).AddTicks(-1);
                break;

            case RevenueFilterType.Year:
                var year = request.Year ?? DateTime.UtcNow.Year;
                periodStart = new DateTime(year, 1, 1);
                periodEnd = new DateTime(year, 12, 31, 23, 59, 59);
                break;

            default:
                // fallback theo dữ liệu
                periodStart = agg?.PeriodStart.DateTime ?? DateTime.UtcNow.Date;
                periodEnd = agg?.PeriodEnd.DateTime ?? DateTime.UtcNow.Date;
                break;
        }

        // map DTO
        return new ActivityMetricsDto
        {
            FilterType = finalFilterType,
            FilterTypeName = finalFilterType.ToString(),
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            TotalOrders = agg?.TotalOrders ?? 0,
            SuccessfulOrders = agg?.SuccessfulOrders ?? 0,
            UnsuccessfulOrders = agg?.UnsuccessfulOrders ?? 0,
            TotalFeedbacks = agg?.TotalFeedbacks ?? 0,
            GoodFeedbacks = agg?.GoodFeedbacks ?? 0,
            SatisfactionPercent = (agg != null && agg.TotalFeedbacks > 0)
                ? Math.Round(100.0 * agg.GoodFeedbacks / agg.TotalFeedbacks, 2)
                : 0,
        };
    }

    #region Helper Methods
    private IQueryable<T> ApplyDateFilter<T>(
     IQueryable<T> query,
     RevenueFilterType filterType,
     RevenueChartRequest request,
     Expression<Func<T, DateTimeOffset?>> dateSelector)
    {
        var propertyName = GetPropertyName(dateSelector);

        switch (filterType)
        {
            case RevenueFilterType.Year:
            case RevenueFilterType.Default: // ✅ Default = Year
                var year = request.Year ?? DateTime.UtcNow.Year;
                var yearStart = new DateTimeOffset(year, 1, 1, 0, 0, 0, TimeSpan.Zero);
                var yearEnd = yearStart.AddYears(1).AddTicks(-1);

                query = query.Where(x =>
                    EF.Property<DateTimeOffset>(x, propertyName) >= yearStart &&
                    EF.Property<DateTimeOffset>(x, propertyName) <= yearEnd);
                break;

            case RevenueFilterType.Quarter:
                if (request.Year.HasValue && request.Quarter.HasValue)
                {
                    var qYear = request.Year.Value;
                    var startMonth = (request.Quarter.Value - 1) * 3 + 1;
                    var start = new DateTimeOffset(qYear, startMonth, 1, 0, 0, 0, TimeSpan.Zero);
                    var end = start.AddMonths(3).AddTicks(-1);

                    query = query.Where(x =>
                        EF.Property<DateTimeOffset>(x, propertyName) >= start &&
                        EF.Property<DateTimeOffset>(x, propertyName) <= end);
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

            case RevenueFilterType.day:
                var targetDate = request.Day?.Date ?? DateTime.UtcNow.Date;
                var dayStart = new DateTimeOffset(targetDate, TimeSpan.Zero);
                var dayEnd = dayStart.AddDays(1).AddTicks(-1);

                query = query.Where(x =>
                    EF.Property<DateTimeOffset>(x, propertyName) >= dayStart &&
                    EF.Property<DateTimeOffset>(x, propertyName) <= dayEnd);
                break;
        }

        return query;
    }

    private TransactionDataItem BuildTransactionItem(List<TransactionDataItem> rawData, int year, int month)
    {
        var found = rawData.FirstOrDefault(d => d.Year == year && d.Month == month);
        if (found != null) return found;

        return new TransactionDataItem
        {
            Year = year,
            Month = month,
            TotalTransactions = 0,
            ShipmentCost = 0,
            Surcharge = 0,
            Refund = 0,
            Compensation = 0,
            TotalIncome = 0,
            TotalOutcome = 0,
            NetAmount = 0,
            NetGrowthPercent = 0
        };
    }

    private ShipmentFeedbackDataItem BuildItem(List<ShipmentFeedbackDataItem> rawData, int year, int month)
    {
        var item = rawData.FirstOrDefault(x => x.Month == month && x.Year == year);
        int totalShipments = item?.TotalShipments ?? 0;
        int totalFeedbacks = item?.TotalFeedbacks ?? 0;

        return new ShipmentFeedbackDataItem
        {
            Year = year,
            Month = month,
            TotalShipments = totalShipments,
            CompleteAndCompensatedCount = item?.CompleteAndCompensatedCount ?? 0,
            CompletedWithCompensationCount = item?.CompletedWithCompensationCount ?? 0,
            CompleteAndCompensatedPercent = totalShipments > 0
                ? Math.Round((item?.CompleteAndCompensatedCount ?? 0) * 100.0 / totalShipments, 2) : 0,
            CompletedWithCompensationPercent = totalShipments > 0
                ? Math.Round((item?.CompletedWithCompensationCount ?? 0) * 100.0 / totalShipments, 2) : 0,

            TotalFeedbacks = totalFeedbacks,
            FiveStarFeedbacks = item?.FiveStarFeedbacks ?? 0,
            FiveStarPercent = totalFeedbacks > 0
                ? Math.Round((item?.FiveStarFeedbacks ?? 0) * 100.0 / totalFeedbacks, 2) : 0
        };
    }

    private string GetPropertyName<T, TProp>(Expression<Func<T, TProp>> expression)
    {
        if (expression.Body is MemberExpression member)
            return member.Member.Name;
        if (expression.Body is UnaryExpression unary && unary.Operand is MemberExpression memberExpr)
            return memberExpr.Member.Name;
        throw new InvalidOperationException("Invalid expression");
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