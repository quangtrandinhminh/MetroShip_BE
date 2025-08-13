using MetroShip.Service.ApiModels;
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
        [HttpPost("shipment-chart")]
        public async Task<IActionResult> GetShipmentChart([FromForm] RevenueChartRequest request)
        {
            var result = await _reportService.GetShipmentChartAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Lấy thống kê transaction chart theo filter (Year, Quarter, MonthRange)
        /// </summary>
        [HttpPost("transaction-chart")]
        public async Task<IActionResult> GetTransactionChart([FromForm] RevenueChartRequest request)
        {
            var result = await _reportService.GetTransactionChartAsync(request);
            return Ok(result);
        }
    }
}
