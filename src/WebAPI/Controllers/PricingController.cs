using MetroShip.Service.Interfaces;
using MetroShip.Utility.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MetroShip.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PricingController(IServiceProvider serviceProvider) : ControllerBase
    {
        private readonly IPricingService _pricingService = serviceProvider.GetRequiredService<IPricingService>();

        [HttpGet("calculation")]
        public async Task<IActionResult> CalculatePriceAsync([FromQuery] decimal weightKg, [FromQuery] decimal distanceKm)
        {
            if (weightKg <= 0 || distanceKm <= 0)
            {
                return BadRequest("Weight and distance must be greater than zero.");
            }

            var price = await _pricingService.CalculatePriceAsync(weightKg, distanceKm);
            return Ok(new { Price = price });
        }

        [HttpGet("pricing-table")]
        public async Task<IActionResult> GetPricingTableAsync([FromQuery] string? pricingConfigId)
        {
            try
            {
                var pricingTable = await _pricingService.GetPricingTableAsync(pricingConfigId);
                return Ok(pricingTable);
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here for brevity)
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    "An error occurred while retrieving the pricing table.");
            }
        }
    }
}
