using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.Interfaces;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Enums;
using MetroShip.Utility.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MetroShip.WebAPI.Controllers
{
    [ApiController]
    public class PricingController(IServiceProvider serviceProvider) : ControllerBase
    {
        private readonly IPricingService _pricingService = serviceProvider.GetRequiredService<IPricingService>();

        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        [HttpGet(WebApiEndpoint.PricingEndpoint.GetAllPricing)]
        public async Task<IActionResult> GetAllPricingAsync([FromQuery] PaginatedListRequest request)
        {
            var response = await _pricingService.GetPricingPaginatedList(request);
            return Ok(response);
        }

        [HttpGet(WebApiEndpoint.PricingEndpoint.CalculatePrice)]
        public async Task<IActionResult> CalculatePriceAsync([FromQuery] decimal weightKg, [FromQuery] decimal distanceKm)
        {
            var price = await _pricingService.CalculatePriceAsync(weightKg, distanceKm);
            return Ok(price);
        }

        [HttpGet(WebApiEndpoint.PricingEndpoint.GetPricingTable)]
        public async Task<IActionResult> GetPricingTableAsync([FromQuery] string? pricingConfigId)
        {
            var pricingTable = await _pricingService.GetPricingTableAsync(pricingConfigId);
            return Ok(BaseResponse.OkResponseDto(pricingTable));
        }
    }
}
