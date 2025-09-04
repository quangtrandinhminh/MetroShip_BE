using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.ParcelCategory;
using MetroShip.Service.ApiModels.Report;
using MetroShip.Service.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MetroShip.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        /// <summary>
        /// Thống kê Shipment (trong ngày)
        /// </summary>
        [HttpGet("shipments/stats")]
        public async Task<IActionResult> GetShipmentStats()
        {
            var result = await _reportService.GetShipmentStatsAsync();
            return Ok(BaseResponse.OkResponseDto(result));
        }

        /// <summary>
        /// Thống kê User (trong tháng)
        /// </summary>
        [HttpGet("users/stats")]
        public async Task<IActionResult> GetUserStats()
        {
            var result = await _reportService.GetUserStatsAsync();
            return Ok(BaseResponse.OkResponseDto(result));
        }

        /// <summary>
        /// Thống kê Transaction (trong tháng)
        /// </summary>
        [HttpGet("transactions/stats")]
        public async Task<IActionResult> GetTransactionStats()
        {
            var result = await _reportService.GetTransactionStatsAsync();
            return Ok(BaseResponse.OkResponseDto(result));
        }

        /// <summary>
        /// Lấy thống kê shipment chart theo filter (Year, Quarter, MonthRange)
        /// </summary>
        [HttpGet("shipment-chart")]
        public async Task<IActionResult> GetShipmentChart([FromQuery] RevenueChartRequest request)
        {
            var result = await _reportService.GetShipmentChartAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Lấy thống kê transaction chart theo filter (Year, Quarter, MonthRange)
        /// </summary>
        [HttpGet("transaction-chart")]
        public async Task<IActionResult> GetTransactionChart([FromQuery] RevenueChartRequest request)
        {
            var result = await _reportService.GetTransactionChartAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Lấy thống kê category chart theo filter (week, month, year)
        /// </summary>
        [HttpGet("category-statistics")]
        public async Task<IActionResult> GetCategoryStatistics([FromQuery] CategoryStatisticsRequest request)
        {
            var result = await _reportService.GetCategoryStatisticsAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Báo cáo thống kê feedback shipments (theo filter: day, year, quarter, month range)
        /// </summary>
        [HttpGet("shipments/feedback-chart")]
        public async Task<IActionResult> GetShipmentFeedbackChart([FromQuery] RevenueChartRequest request)
        {
            var result = await _reportService.GetShipmentFeedbackChartAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Báo cáo chỉ số hoạt động (theo filter: day, year, quarter, month range)
        ///</summary>
        [HttpGet("activity-metrics")]
        public async Task<IActionResult> GetActivityMetrics([FromQuery] RevenueChartRequest request)
        {
            var result = await _reportService.GetActivityMetricsAsync(request);
            return Ok(result);
        }
    }
}
