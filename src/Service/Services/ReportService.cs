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
using System.Linq;
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
        var now = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7));
        var startOfThisMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);
        var startOfNextMonth = startOfThisMonth.AddMonths(1);

        var startOfLastMonth = startOfThisMonth.AddMonths(-1);
        var endOfLastMonth = startOfThisMonth.AddTicks(-1);

        var query = _shipmentRepository.GetAllWithCondition();

        // === Tháng hiện tại ===
        var thisMonthQuery = query.Where(s => s.CreatedAt >= startOfThisMonth.UtcDateTime && s.CreatedAt < startOfNextMonth.UtcDateTime);
        var totalShipmentsThisMonth = await thisMonthQuery.CountAsync();

        var totalCompleteThisMonth = await thisMonthQuery.CountAsync(s =>
            s.ShipmentStatus == ShipmentStatusEnum.Completed ||
            s.ShipmentStatus == ShipmentStatusEnum.Compensated ||
            s.ShipmentStatus == ShipmentStatusEnum.CompletedWithCompensation);

        // === Tháng trước ===
        var lastMonthQuery = query.Where(s => s.CreatedAt >= startOfLastMonth.UtcDateTime && s.CreatedAt <= endOfLastMonth.UtcDateTime);
        var totalShipmentsLastMonth = await lastMonthQuery.CountAsync();

        var totalCompleteLastMonth = await lastMonthQuery.CountAsync(s =>
            s.ShipmentStatus == ShipmentStatusEnum.Completed ||
            s.ShipmentStatus == ShipmentStatusEnum.Compensated ||
            s.ShipmentStatus == ShipmentStatusEnum.CompletedWithCompensation);

        // === % tăng trưởng ===
        var percentageNewShipments = totalShipmentsLastMonth > 0
            ? Math.Round((double)(totalShipmentsThisMonth - totalShipmentsLastMonth) / totalShipmentsLastMonth * 100, 2)
            : 0;

        var percentageNewCompleteShipments = totalCompleteLastMonth > 0
            ? Math.Round((double)(totalCompleteThisMonth - totalCompleteLastMonth) / totalCompleteLastMonth * 100, 2)
            : 0;

        return new ShipmentListWithStatsResponse
        {
            TotalShipments = totalShipmentsThisMonth,
            PercentageNewShipments = percentageNewShipments,
            TotalCompleteShipments = totalCompleteThisMonth,
            PercentageNewCompleteShipments = percentageNewCompleteShipments
        };
    }

    public async Task<UserListWithStatsResponse> GetUserStatsAsync()
    {
        var query = _userRepository.GetAllWithCondition()
            .Where(v => v.DeletedTime == null &&
                        v.UserRoles.Any(r => r.Role.Name == UserRoleEnum.Customer.ToString()));

        var now = DateTimeOffset.UtcNow;
        var startOfThisMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var startOfNextMonth = startOfThisMonth.AddMonths(1);

        var startOfLastMonth = startOfThisMonth.AddMonths(-1);
        var endOfLastMonth = startOfThisMonth.AddTicks(-1);

        // === Tháng này ===
        var thisMonthUsers = await query.CountAsync(u =>
            u.CreatedTime >= startOfThisMonth && u.CreatedTime < startOfNextMonth);

        // === Tháng trước ===
        var lastMonthUsers = await query.CountAsync(u =>
            u.CreatedTime >= startOfLastMonth && u.CreatedTime <= endOfLastMonth);

        // === % tăng trưởng ===
        var percentageNewUsers = lastMonthUsers > 0
            ? Math.Round((double)(thisMonthUsers - lastMonthUsers) / lastMonthUsers * 100, 2)
            : 0;

        return new UserListWithStatsResponse
        {
            TotalUsersWithRoleUser = thisMonthUsers,
            PercentageNewUsers = percentageNewUsers
        };
    }

    public async Task<TransactionListWithStatsResponse> GetTransactionStatsAsync()
    {
        var query = _transactionRepository.GetAllWithCondition()
            .Where(t => t.DeletedAt == null);

        var now = DateTimeOffset.UtcNow;
        var startOfThisMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var startOfNextMonth = startOfThisMonth.AddMonths(1);

        var startOfLastMonth = startOfThisMonth.AddMonths(-1);
        var endOfLastMonth = startOfThisMonth.AddTicks(-1);

        decimal CalcGrowth(decimal current, decimal prev) =>
            prev > 0 ? Math.Round(((current - prev) / prev) * 100m, 2) : 0;

        // ===== Tháng này =====
        var thisMonthTransactions = await query.CountAsync(t =>
            t.CreatedAt >= startOfThisMonth && t.CreatedAt < startOfNextMonth);

        var thisMonthPaidTransactions = await query.CountAsync(t =>
            t.PaymentStatus == PaymentStatusEnum.Paid &&
            t.CreatedAt >= startOfThisMonth && t.CreatedAt < startOfNextMonth);

        var thisMonthPaidAmount = await query
        .Where(t => t.PaymentStatus == PaymentStatusEnum.Paid &&
                    (t.TransactionType == TransactionTypeEnum.ShipmentCost ||
                     t.TransactionType == TransactionTypeEnum.Surcharge) &&
                    t.CreatedAt >= startOfThisMonth && t.CreatedAt < startOfNextMonth)
        .SumAsync(t => t.PaymentAmount);

        var thisMonthUnpaidTransactions = await query.CountAsync(t =>
            t.PaymentStatus == PaymentStatusEnum.Failed &&
            t.CreatedAt >= startOfThisMonth && t.CreatedAt < startOfNextMonth);

        var thisMonthPendingTransactions = await query.CountAsync(t =>
            t.PaymentStatus == PaymentStatusEnum.Pending &&
            t.CreatedAt >= startOfThisMonth && t.CreatedAt < startOfNextMonth);

        var thisMonthCancelledTransactions = await query.CountAsync(t =>
            t.PaymentStatus == PaymentStatusEnum.Cancelled &&
            t.CreatedAt >= startOfThisMonth && t.CreatedAt < startOfNextMonth);

        // ===== Tháng trước =====
        var prevMonthTransactions = await query.CountAsync(t =>
            t.CreatedAt >= startOfLastMonth && t.CreatedAt <= endOfLastMonth);

        var prevMonthPaidTransactions = await query.CountAsync(t =>
            t.PaymentStatus == PaymentStatusEnum.Paid &&
            t.CreatedAt >= startOfLastMonth && t.CreatedAt <= endOfLastMonth);

        var prevMonthPaidAmount = await query
            .Where(t => t.PaymentStatus == PaymentStatusEnum.Paid &&
                        t.CreatedAt >= startOfLastMonth && t.CreatedAt <= endOfLastMonth)
            .SumAsync(t => t.PaymentAmount);

        var prevMonthUnpaidTransactions = await query.CountAsync(t =>
            t.PaymentStatus == PaymentStatusEnum.Failed &&
            t.CreatedAt >= startOfLastMonth && t.CreatedAt <= endOfLastMonth);

        var prevMonthPendingTransactions = await query.CountAsync(t =>
            t.PaymentStatus == PaymentStatusEnum.Pending &&
            t.CreatedAt >= startOfLastMonth && t.CreatedAt <= endOfLastMonth);

        var prevMonthCancelledTransactions = await query.CountAsync(t =>
            t.PaymentStatus == PaymentStatusEnum.Cancelled &&
            t.CreatedAt >= startOfLastMonth && t.CreatedAt <= endOfLastMonth);

        // ===== Tính % tăng trưởng =====
        var growthTotalTransactions = CalcGrowth(thisMonthTransactions, prevMonthTransactions);
        var growthPaidTransactions = CalcGrowth(thisMonthPaidTransactions, prevMonthPaidTransactions);
        var growthPaidAmount = CalcGrowth(thisMonthPaidAmount, prevMonthPaidAmount);
        var growthUnpaidTransactions = CalcGrowth(thisMonthUnpaidTransactions, prevMonthUnpaidTransactions);
        var growthPendingTransactions = CalcGrowth(thisMonthPendingTransactions, prevMonthPendingTransactions);
        var growthCancelledTransactions = CalcGrowth(thisMonthCancelledTransactions, prevMonthCancelledTransactions);

        return new TransactionListWithStatsResponse
        {
            TotalTransactions = thisMonthTransactions,
            PercentageNewTransactions = (double)growthTotalTransactions,

            TotalPaidTransactions = thisMonthPaidTransactions,
            PercentageNewPaidTransactions = (double)growthPaidTransactions,
            TotalPaidAmount = thisMonthPaidAmount,
            GrowthPaidAmount = growthPaidAmount,

            TotalUnpaidTransactions = thisMonthUnpaidTransactions,
            PercentageUnpaidTransactions = (double)growthUnpaidTransactions,

            TotalPendingTransactions = thisMonthPendingTransactions,
            PercentagePendingTransactions = (double)growthPendingTransactions,

            TotalCancelledTransactions = thisMonthCancelledTransactions,
            PercentageCancelledTransactions = (double)growthCancelledTransactions
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

        var baseQuery = _transactionRepository.GetAllWithCondition()
            .Where(t => t.DeletedAt == null);

        IQueryable<Transaction> query;

        // 👉 Với Week thì không dùng ApplyDateFilter, để tránh mất ngày giao thoa giữa 2 tháng
        if (filterType == RevenueFilterType.Week)
        {
            query = baseQuery;
        }
        else
        {
            query = ApplyDateFilter(baseQuery, filterType, request, t => t.CreatedAt);
        }

        var rawData = await query
            .GroupBy(t => new { Year = t.CreatedAt.Year, Month = t.CreatedAt.Month, Day = t.CreatedAt.Day })
            .Select(g => new TransactionDataItem
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Day = g.Key.Day,
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
        DateTime? respWeekStart = null;
        DateTime? respWeekEnd = null;

        switch (filterType)
        {
            case RevenueFilterType.Day:
                {
                    var targetYear = request.Year ?? DateTime.UtcNow.Year;
                    var targetMonth = request.StartMonth ?? DateTime.UtcNow.Month;
                    fullData = rawData
                        .Where(d => d.Year == targetYear && d.Month == targetMonth)
                        .ToList();
                    break;
                }

            case RevenueFilterType.Week:
                {
                    DateTime ws, we;

                    if (request.Day.HasValue)
                    {
                        // Tuần chứa ngày cụ thể
                        var target = request.Day.Value.Date;
                        var diff = ((int)target.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
                        ws = target.AddDays(-diff); // Monday
                        we = ws.AddDays(6);        // Sunday
                    }
                    else if (request.StartMonth.HasValue)
                    {
                        // Tuần thứ N trong tháng, đảm bảo full tuần 7 ngày
                        var year = request.Year ?? DateTime.UtcNow.Year;
                        var month = request.StartMonth.Value;
                        var weekInMonth = Math.Max(1, request.Week ?? 1);

                        var firstDayOfMonth = new DateTime(year, month, 1);
                        // Ngày đầu tiên trong tuần (Monday) của tuần đầu tiên trong tháng
                        var firstMonday = firstDayOfMonth.AddDays((8 - (int)firstDayOfMonth.DayOfWeek) % 7);

                        ws = firstMonday.AddDays((weekInMonth - 1) * 7);
                        we = ws.AddDays(6);

                        // Nếu ws < đầu tháng thì set bằng ngày đầu tháng
                        if (ws < firstDayOfMonth)
                            ws = firstDayOfMonth;
                    }
                    else
                    {
                        // Tuần thứ N trong năm
                        var year = request.Year ?? DateTime.UtcNow.Year;
                        var weekInYear = Math.Max(1, request.Week ?? 1);
                        (ws, we) = GetWeekRangeInYear(year, weekInYear); // phương thức đã có
                    }

                    // Chuyển về UTC để lưu/so sánh với DB
                    ws = DateTime.SpecifyKind(ws.Date, DateTimeKind.Utc);
                    we = DateTime.SpecifyKind(we.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

                    respWeekStart = ws;
                    respWeekEnd = we;

                    // 🔹 Lọc trực tiếp từ DB theo CreatedAt UTC
                    var weekTransactions = await query
                        .Where(t => t.CreatedAt >= ws && t.CreatedAt <= we)
                        .ToListAsync();

                    // 🔹 Group theo ngày
                    var rawWeekData = weekTransactions
                        .GroupBy(t => new { Year = t.CreatedAt.Year, Month = t.CreatedAt.Month, Day = t.CreatedAt.Day })
                        .Select(g => new TransactionDataItem
                        {
                            Year = g.Key.Year,
                            Month = g.Key.Month,
                            Day = g.Key.Day,
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
                        .ToList();

                    // 🔹 Build full 7 ngày tuần, ngày không có giao dịch = 0
                    var daysInWeek = Enumerable.Range(0, 7).Select(i => ws.AddDays(i)).ToList();
                    fullData = BuildFullDataForDays(daysInWeek, rawWeekData);

                    break;
                }

            case RevenueFilterType.Default:
            case RevenueFilterType.Year:
                {
                    var yearOnly = request.Year ?? DateTime.UtcNow.Year;
                    fullData = Enumerable.Range(1, 12)
                        .Select(m => BuildTransactionItem(rawData, yearOnly, m))
                        .ToList();
                    break;
                }

            case RevenueFilterType.Quarter:
                {
                    var qYear = request.Year ?? DateTime.UtcNow.Year;
                    var q = request.Quarter ?? 1;
                    var monthsInQuarter = Enumerable.Range((q - 1) * 3 + 1, 3);
                    fullData = monthsInQuarter
                        .Select(m => BuildTransactionItem(rawData, qYear, m))
                        .ToList();
                    break;
                }

            case RevenueFilterType.MonthRange:
                {
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
                }

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

        // === Growth theo NetAmount ===
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
            Week = request.Week,
            StartYear = request.StartYear,
            StartMonth = request.StartMonth,
            EndYear = request.EndYear,
            EndMonth = request.EndMonth,
            WeekStartDate = respWeekStart,
            WeekEndDate = respWeekEnd,
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
            .SelectMany(s => s.Parcels)
            .Where(p => p.CategoryInsurance.ParcelCategory != null)
            .GroupBy(p => p.CategoryInsurance.ParcelCategory.CategoryName)
            .Select(g => new
            {
                Name = g.Key,
                Orders = g.Count()
            })
            .ToListAsync();

        // ==== Kỳ trước (so sánh growth) ====
        var durationDays = (endDate - startDate).TotalDays + 1;
        var previousStart = startDate.AddDays(-durationDays);
        var previousEnd = startDate.AddDays(-1);

        var previousData = await _shipmentRepository.GetAllWithCondition()
            .Where(s => s.CreatedAt >= previousStart && s.CreatedAt <= previousEnd)
            .SelectMany(s => s.Parcels)
            .Where(p => p.CategoryInsurance.ParcelCategory != null)
            .GroupBy(p => p.CategoryInsurance.ParcelCategory.CategoryName)
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

        // ✅ Base query, chỉ lấy bản ghi có FeedbackAt
        var baseQuery = _shipmentRepository.GetAllWithCondition()
            .Where(s => s.FeedbackAt.HasValue);

        IQueryable<Shipment> query;

        // 👉 Với Week thì không dùng ApplyDateFilter
        if (filterType == RevenueFilterType.Week)
        {
            query = baseQuery;
        }
        else
        {
            // ✅ Chỉ truyền s.FeedbackAt (nullable) -> EF sẽ dịch được
            query = ApplyDateFilter(baseQuery, filterType, request, s => s.FeedbackAt);
        }

        // ===== Lấy dữ liệu từ DB =====
        var shipments = await query
            .Select(s => new
            {
                FeedbackAt = s.FeedbackAt,
                s.ShipmentStatus,
                s.Rating
            })
            .ToListAsync();

        // ===== Group trong RAM (an toàn, không còn lỗi dịch EF) =====
        var rawData = shipments
            .Where(s => s.FeedbackAt.HasValue)
            .GroupBy(s => s.FeedbackAt.Value.UtcDateTime.Date)
            .Select(g => new ShipmentFeedbackDataItem
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Day = g.Key.Day,

                TotalShipments = g.Count(),
                CompleteAndCompensatedCount = g.Count(s =>
                    s.ShipmentStatus == ShipmentStatusEnum.Completed ||
                    s.ShipmentStatus == ShipmentStatusEnum.Compensated),
                CompletedWithCompensationCount = g.Count(s =>
                    s.ShipmentStatus == ShipmentStatusEnum.CompletedWithCompensation),

                TotalFeedbacks = g.Count(s => s.Rating != null),
                FiveStarFeedbacks = g.Count(s => s.Rating == 5)
            })
            .ToList();

        List<ShipmentFeedbackDataItem> fullData;
        DateTime? respWeekStart = null;
        DateTime? respWeekEnd = null;

        switch (filterType)
        {
            case RevenueFilterType.Day:
                {
                    var targetYear = request.Year ?? DateTime.UtcNow.Year;
                    var targetMonth = request.StartMonth ?? DateTime.UtcNow.Month;
                    fullData = rawData
                        .Where(d => d.Year == targetYear && d.Month == targetMonth)
                        .ToList();
                    break;
                }

            case RevenueFilterType.Week:
                {
                    DateTime ws, we;

                    if (request.Day.HasValue)
                    {
                        // Tuần chứa ngày cụ thể
                        var target = request.Day.Value.Date;
                        var diff = ((int)target.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
                        ws = target.AddDays(-diff); // Monday
                        we = ws.AddDays(6);        // Sunday
                    }
                    else if (request.StartMonth.HasValue)
                    {
                        // Tuần thứ N trong tháng
                        var year = request.Year ?? DateTime.UtcNow.Year;
                        var month = request.StartMonth.Value;
                        var weekInMonth = Math.Max(1, request.Week ?? 1);

                        var firstDayOfMonth = new DateTime(year, month, 1);
                        var firstMonday = firstDayOfMonth.AddDays((8 - (int)firstDayOfMonth.DayOfWeek) % 7);

                        ws = firstMonday.AddDays((weekInMonth - 1) * 7);
                        we = ws.AddDays(6);

                        if (ws < firstDayOfMonth)
                            ws = firstDayOfMonth;
                    }
                    else
                    {
                        // Tuần thứ N trong năm
                        var year = request.Year ?? DateTime.UtcNow.Year;
                        var weekInYear = Math.Max(1, request.Week ?? 1);
                        (ws, we) = GetWeekRangeInYear(year, weekInYear);
                    }

                    ws = DateTime.SpecifyKind(ws.Date, DateTimeKind.Utc);
                    we = DateTime.SpecifyKind(we.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

                    respWeekStart = ws;
                    respWeekEnd = we;

                    // ✅ lọc trong RAM
                    var rawWeekData = shipments
                        .Where(s => s.FeedbackAt >= ws && s.FeedbackAt <= we)
                        .GroupBy(s => s.FeedbackAt.Value.UtcDateTime.Date)
                        .Select(g => new ShipmentFeedbackDataItem
                        {
                            Year = g.Key.Year,
                            Month = g.Key.Month,
                            Day = g.Key.Day,

                            TotalShipments = g.Count(),
                            CompleteAndCompensatedCount = g.Count(s =>
                                s.ShipmentStatus == ShipmentStatusEnum.Completed ||
                                s.ShipmentStatus == ShipmentStatusEnum.Compensated),
                            CompletedWithCompensationCount = g.Count(s =>
                                s.ShipmentStatus == ShipmentStatusEnum.CompletedWithCompensation),

                            TotalFeedbacks = g.Count(s => s.Rating != null),
                            FiveStarFeedbacks = g.Count(s => s.Rating == 5)
                        })
                        .ToList();

                    // 🔹 Build full 7 ngày
                    var daysInWeek = Enumerable.Range(0, 7).Select(i => ws.AddDays(i)).ToList();
                    fullData = daysInWeek.Select(d =>
                    {
                        var existing = rawWeekData.FirstOrDefault(r =>
                            r.Year == d.Year && r.Month == d.Month && r.Day == d.Day);

                        return existing ?? new ShipmentFeedbackDataItem
                        {
                            Year = d.Year,
                            Month = d.Month,
                            Day = d.Day,
                            TotalShipments = 0,
                            CompleteAndCompensatedCount = 0,
                            CompletedWithCompensationCount = 0,
                            TotalFeedbacks = 0,
                            FiveStarFeedbacks = 0
                        };
                    }).ToList();

                    break;
                }

            case RevenueFilterType.Default:
            case RevenueFilterType.Year:
                {
                    var yearOnly = request.Year ?? DateTime.UtcNow.Year;
                    fullData = Enumerable.Range(1, 12)
                        .Select(m => BuildShipmentFeedbackItem(rawData, yearOnly, m))
                        .ToList();
                    break;
                }

            case RevenueFilterType.Quarter:
                {
                    var qYear = request.Year ?? DateTime.UtcNow.Year;
                    var q = request.Quarter ?? 1;
                    var monthsInQuarter = Enumerable.Range((q - 1) * 3 + 1, 3);
                    fullData = monthsInQuarter
                        .Select(m => BuildShipmentFeedbackItem(rawData, qYear, m))
                        .ToList();
                    break;
                }

            case RevenueFilterType.MonthRange:
                {
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
                        .Select(m => BuildShipmentFeedbackItem(rawData, m.Year, m.Month))
                        .ToList();
                    break;
                }

            default:
                fullData = rawData;
                break;
        }

        return new RevenueChartResponse<ShipmentFeedbackDataItem>
        {
            FilterType = filterType,
            Year = request.Year,
            Quarter = request.Quarter,
            Week = request.Week,
            StartYear = request.StartYear,
            StartMonth = request.StartMonth,
            EndYear = request.EndYear,
            EndMonth = request.EndMonth,
            WeekStartDate = respWeekStart,
            WeekEndDate = respWeekEnd,
            Data = fullData
        };
    }

    public async Task<ActivityMetricsDto> GetActivityMetricsAsync(RevenueChartRequest request)
    {
        IQueryable<Shipment> q;

        // ✅ Nếu null hoặc Default thì coi như Year
        var requestedFilter = request?.FilterType ?? RevenueFilterType.Default;
        var finalFilterType = (requestedFilter == RevenueFilterType.Default)
            ? RevenueFilterType.Year
            : requestedFilter;

        if (finalFilterType == RevenueFilterType.Day)
        {
            // ✅ Day = Today
            var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
            var tomorrow = today.AddDays(1);

            q = _shipmentRepository.GetAllWithCondition()
                 .Where(s => s.CreatedAt >= today && s.CreatedAt < tomorrow);
        }
        else
        {
            // ✅ Default => Year
            q = _shipmentRepository.GetAllWithCondition();
            q = ApplyDateFilter(q, finalFilterType, request, s => s.CreatedAt);
        }

        // ✅ Xác định khoảng thời gian
        DateTime periodStart;
        DateTime periodEnd;

        switch (finalFilterType)
        {
            case RevenueFilterType.Day:
                var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
                periodStart = today;
                periodEnd = today.AddDays(1).AddTicks(-1);
                break;

            case RevenueFilterType.Week:
                if (request.Day.HasValue)
                {
                    // tuần chứa ngày cụ thể
                    var target = DateTime.SpecifyKind(request.Day.Value.Date, DateTimeKind.Utc);
                    var diff = ((int)target.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
                    var ws = target.AddDays(-diff);
                    var we = ws.AddDays(6);

                    periodStart = ws;
                    periodEnd = we.AddDays(1).AddTicks(-1);
                }
                else
                {
                    // tuần thứ N trong tháng
                    var year = request.Year ?? DateTime.UtcNow.Year;
                    var month = request.StartMonth ?? DateTime.UtcNow.Month;
                    var weekInMonth = Math.Max(1, request.Week ?? 1);

                    var (wsUtc, weUtc) = GetWeekRangeInMonth(year, month, weekInMonth);

                    periodStart = DateTime.SpecifyKind(wsUtc, DateTimeKind.Utc); // Giả sử GetWeekRangeInMonth trả Unspecified
                    periodEnd = DateTime.SpecifyKind(weUtc, DateTimeKind.Utc);
                }
                break;

            case RevenueFilterType.MonthRange:
                var startYear = request.StartYear ?? DateTime.UtcNow.Year;
                var startMonth = request.StartMonth ?? DateTime.UtcNow.Month;
                periodStart = DateTime.SpecifyKind(new DateTime(startYear, startMonth, 1), DateTimeKind.Utc);

                var endYear = request.EndYear ?? startYear;
                var endMonth = request.EndMonth ?? startMonth;
                // Lấy ngày cuối cùng của endMonth
                var endMonthLastDay = DateTime.DaysInMonth(endYear, endMonth);
                periodEnd = DateTime.SpecifyKind(new DateTime(endYear, endMonth, endMonthLastDay, 23, 59, 59), DateTimeKind.Utc);
                // ✅ Apply filter trước khi aggregate
                q = q.Where(s => s.CreatedAt >= periodStart && s.CreatedAt <= periodEnd);
                break;

            case RevenueFilterType.Quarter:
                var qYear = request.Year ?? DateTime.UtcNow.Year;
                var quarter = request.Quarter ?? ((DateTime.UtcNow.Month - 1) / 3 + 1);
                var qStartMonth = (quarter - 1) * 3 + 1;
                periodStart = DateTime.SpecifyKind(new DateTime(qYear, qStartMonth, 1), DateTimeKind.Utc);
                periodEnd = periodStart.AddMonths(3).AddTicks(-1);
                break;  // Di chuyển lọc vào trước aggregate nếu có thể, hoặc re-agg sau

            case RevenueFilterType.Year: // ✅ Default cũng đã map về Year ở trên
                var yearOnly = request.Year ?? DateTime.UtcNow.Year;
                periodStart = DateTime.SpecifyKind(new DateTime(yearOnly, 1, 1), DateTimeKind.Utc);
                periodEnd = DateTime.SpecifyKind(new DateTime(yearOnly, 12, 31, 23, 59, 59), DateTimeKind.Utc);
                break;

            default:
                // fallback theo dữ liệu (sẽ set sau aggregate nếu cần)
                periodStart = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
                periodEnd = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
                break;
        }

        // Áp dụng lọc thêm cho Quarter nếu ApplyDateFilter chưa xử lý đầy đủ
        if (finalFilterType == RevenueFilterType.Quarter)
        {
            q = q.Where(s => s.CreatedAt >= periodStart && s.CreatedAt <= periodEnd);
        }

        // nhóm status (di chuyển lên để reuse nếu re-agg)
        var completedStatuses = new[]
        {
        ShipmentStatusEnum.Completed
    };

        var compensatedStatuses = new[]
        {
        ShipmentStatusEnum.Compensated,
        ShipmentStatusEnum.CompletedWithCompensation
    };

        var refundedStatuses = new[]
        {
        ShipmentStatusEnum.Returned
    };

        // aggregate query (bây giờ sau tất cả lọc)
        var agg = await q
            .GroupBy(_ => 1)
            .Select(g => new
            {
                PeriodStart = g.Min(s => s.CreatedAt),
                PeriodEnd = g.Max(s => s.CreatedAt),
                TotalOrders = g.Count(),
                CompletedOrders = g.Count(s => completedStatuses.Contains(s.ShipmentStatus)),
                CompensatedOrders = g.Count(s => compensatedStatuses.Contains(s.ShipmentStatus)),
                RefundedOrders = g.Count(s => refundedStatuses.Contains(s.ShipmentStatus)),
                TotalFeedbacks = g.Count(s => s.Rating != null),
                GoodFeedbacks = g.Count(s => s.Rating != null && s.Rating >= 4)
            })
            .FirstOrDefaultAsync();

        return new ActivityMetricsDto
        {
            FilterType = requestedFilter, // ✅ vẫn trả về đúng cái request gốc (Default nếu null)
            FilterTypeName = requestedFilter.ToString(),
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            TotalOrders = agg?.TotalOrders ?? 0,
            CompletedOrders = agg?.CompletedOrders ?? 0,
            CompensatedOrders = agg?.CompensatedOrders ?? 0,
            RefundedOrders = agg?.RefundedOrders ?? 0,
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

            case RevenueFilterType.Week:
                {
                    // Mode 1: nếu client gửi Day => lấy tuần chứa Day
                    if (request.Day.HasValue)
                    {
                        var target = request.Day.Value.Date; // DateTime (local) — ta chuyển sang UTC date
                        var targetUtc = DateTime.SpecifyKind(target, DateTimeKind.Utc);

                        // tìm Monday trước hoặc bằng target (tuần chứa target)
                        var diff = ((int)targetUtc.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
                        var weekStartDate = targetUtc.AddDays(-diff);
                        var weekEndDate = weekStartDate.AddDays(6);

                        var start = new DateTimeOffset(weekStartDate, TimeSpan.Zero);
                        var end = new DateTimeOffset(weekEndDate.Date.AddDays(1).AddTicks(-1), TimeSpan.Zero);

                        query = query.Where(x =>
                            EF.Property<DateTimeOffset>(x, propertyName) >= start &&
                            EF.Property<DateTimeOffset>(x, propertyName) <= end);
                    }
                    else
                    {
                        // Mode 2: dùng Year + StartMonth + Week (tuần thứ N của THÁNG)
                        var yeart = request.Year ?? DateTime.UtcNow.Year;
                        var month = request.StartMonth ?? DateTime.UtcNow.Month;
                        var weekInMonth = Math.Max(1, request.Week ?? 1);

                        var (wsUtc, weUtc) = GetWeekRangeInMonth(yeart, month, weekInMonth);

                        var start = new DateTimeOffset(wsUtc, TimeSpan.Zero);
                        var end = new DateTimeOffset(weUtc, TimeSpan.Zero);

                        query = query.Where(x =>
                            EF.Property<DateTimeOffset>(x, propertyName) >= start &&
                            EF.Property<DateTimeOffset>(x, propertyName) <= end);
                    }
                    break;
                }

            case RevenueFilterType.Day:
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

    private ShipmentFeedbackDataItem BuildShipmentFeedbackItem(List<ShipmentFeedbackDataItem> rawData, int year, int month)
    {
        var existing = rawData.FirstOrDefault(d => d.Year == year && d.Month == month);
        if (existing != null)
        {
            return existing;
        }

        return new ShipmentFeedbackDataItem
        {
            Year = year,
            Month = month,
            Day = null, // tháng thì không cần Day
            TotalShipments = 0,
            CompleteAndCompensatedCount = 0,
            CompletedWithCompensationCount = 0,
            TotalFeedbacks = 0,
            FiveStarFeedbacks = 0
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
        var today = DateTimeOffset.UtcNow; // ✅ luôn UTC

        switch (request.RangeType)
        {
            case RangeType.Today:
                var startOfToday = new DateTimeOffset(today.Year, today.Month, today.Day, 0, 0, 0, TimeSpan.Zero);
                return (startOfToday, startOfToday.AddDays(1).AddTicks(-1));

            case RangeType.ThisWeek:
                // Lùi về Monday (UTC)
                var diff = (int)today.DayOfWeek == 0 ? 6 : (int)today.DayOfWeek - 1;
                var weekStart = new DateTimeOffset(today.Year, today.Month, today.Day, 0, 0, 0, TimeSpan.Zero)
                                    .AddDays(-diff);
                var weekEnd = weekStart.AddDays(7).AddTicks(-1);
                return (weekStart, weekEnd);

            case RangeType.ThisMonth:
                var monthStart = new DateTimeOffset(today.Year, today.Month, 1, 0, 0, 0, TimeSpan.Zero);
                var monthEnd = monthStart.AddMonths(1).AddTicks(-1);
                return (monthStart, monthEnd);

            case RangeType.Year:
            default: // ✅ default = Year
                var yearStart = new DateTimeOffset(today.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);
                var yearEnd = yearStart.AddYears(1).AddTicks(-1);
                return (yearStart, yearEnd);
        }
    }

    private static (DateTime weekStartDateUtc, DateTime weekEndDateUtc) GetWeekRangeInMonth(int year, int month, int weekInMonth)
    {
        // đảm bảo weekInMonth >= 1
        weekInMonth = Math.Max(1, weekInMonth);

        var monthStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastDayOfMonth = new DateTime(year, month, DateTime.DaysInMonth(year, month), 0, 0, 0, DateTimeKind.Utc);

        // tìm Monday **on or after** first day of month
        var firstMonday = monthStart;
        while (firstMonday.DayOfWeek != DayOfWeek.Monday)
        {
            firstMonday = firstMonday.AddDays(1);
        }

        // tuần bắt đầu
        var weekStart = firstMonday.AddDays((weekInMonth - 1) * 7);

        // nếu tuầnStart đã vượt quá cuối tháng -> lấy Monday cuối cùng trong tháng
        if (weekStart > lastDayOfMonth)
        {
            // tìm Monday on or before lastDayOfMonth
            var lastMonday = lastDayOfMonth;
            while (lastMonday.DayOfWeek != DayOfWeek.Monday)
            {
                lastMonday = lastMonday.AddDays(-1);
            }

            weekStart = lastMonday;
        }

        var weekEnd = weekStart.AddDays(6);
        if (weekEnd > lastDayOfMonth) weekEnd = lastDayOfMonth;

        // trả về DateTime UTC (00:00:00 start, 23:59:59.9999999 end)
        var weekStartUtc = DateTime.SpecifyKind(weekStart.Date, DateTimeKind.Utc);
        var weekEndUtc = DateTime.SpecifyKind(weekEnd.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

        return (weekStartUtc, weekEndUtc);
    }

    private List<TransactionDataItem> BuildFullDataForDays(List<DateTime> days, List<TransactionDataItem> rawData)
    {
        return days.Select(d =>
        {
            var existing = rawData.FirstOrDefault(r =>
                r.Year == d.Year && r.Month == d.Month && r.Day == d.Day);

            return existing ?? new TransactionDataItem
            {
                Year = d.Year,
                Month = d.Month,
                Day = d.Day,
                TotalTransactions = 0,
                ShipmentCost = 0,
                Surcharge = 0,
                Refund = 0,
                Compensation = 0
            };
        }).ToList();
    }

    private (DateTime wsUtc, DateTime weUtc) GetWeekRangeInYear(int year, int weekOfYear)
    {
        var jan1 = new DateTime(year, 1, 1);
        int daysOffset = DayOfWeek.Monday - jan1.DayOfWeek;

        var firstMonday = jan1.AddDays(daysOffset);
        if (firstMonday.Year < year) firstMonday = firstMonday.AddDays(7);

        var ws = firstMonday.AddDays((weekOfYear - 1) * 7);
        var we = ws.AddDays(6);

        return (DateTime.SpecifyKind(ws, DateTimeKind.Utc),
                DateTime.SpecifyKind(we.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc));
    }

    #endregion
}