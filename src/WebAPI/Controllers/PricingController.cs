using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Pricing;
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
    [Produces("application/json")]
    public class PricingController(IServiceProvider serviceProvider) : ControllerBase
    {
        private readonly IPricingService _pricingService = serviceProvider.GetRequiredService<IPricingService>();

        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        [HttpGet(WebApiEndpoint.PricingEndpoint.GetAllPricing)]
        [ProducesResponseType(typeof(BaseResponse<PaginatedListResponse<PricingTableResponse>>), 200)]
        public async Task<IActionResult> GetAllPricingAsync([FromQuery] PaginatedListRequest request, [FromQuery] bool? isActive = null)
        {
            var response = await _pricingService.GetPricingPaginatedList(request, isActive);
            return Ok(response);
        }

        [HttpGet(WebApiEndpoint.PricingEndpoint.CalculatePrice)]
        public async Task<IActionResult> CalculatePriceAsync([FromQuery] decimal weightKg, [FromQuery] decimal distanceKm)
        {
            var price = await _pricingService.CalculatePriceAsync(weightKg, distanceKm);
            return Ok(price);
        }

        [HttpGet(WebApiEndpoint.PricingEndpoint.GetPricingTable)]
        [ProducesResponseType(typeof(BaseResponse<PricingTableResponse>), 200)]
        public async Task<IActionResult> GetPricingTableAsync([FromQuery] string? pricingConfigId)
        {
            var pricingTable = await _pricingService.GetPricingTableAsync(pricingConfigId);
            return Ok(BaseResponse.OkResponseDto(pricingTable));
        }

        [HttpPost(WebApiEndpoint.PricingEndpoint.CreatePricing)]
        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        public async Task<IActionResult> CreatePricingAsync([FromBody] PricingConfigRequest request)
        {
            var newPricingConfig = await _pricingService.ChangePricingConfigAsync(request);
            return Ok(BaseResponse.OkResponseDto(newPricingConfig, null));
        }

        [HttpPut(WebApiEndpoint.PricingEndpoint.UpdatePricing)]
        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        public async Task<IActionResult> ActivatePricingAsync([FromRoute] string pricingConfigId)
        {
            var activatedPricingConfigId = await _pricingService.ActivatePricingConfigAsync(pricingConfigId);
            return Ok(BaseResponse.OkResponseDto(activatedPricingConfigId, null));
        }

        [HttpDelete(WebApiEndpoint.PricingEndpoint.DeletePricing)]
        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        public async Task<IActionResult> DeletePricingAsync([FromRoute] string pricingConfigId)
        {
            var deletedPricingConfigId = await _pricingService.DeletePricingConfigAsync(pricingConfigId);
            return Ok(BaseResponse.OkResponseDto(deletedPricingConfigId, null));
        }
    }
}
