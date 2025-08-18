using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.Interfaces;
using MetroShip.Utility.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MetroShip.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    public class InsuranceController(IServiceProvider serviceProvider) : ControllerBase
    {
        private readonly IInsuranceService _insuranceService = serviceProvider.GetRequiredService<IInsuranceService>();

        [HttpGet(WebApiEndpoint.InsurancePolicyEndpoint.GetAllPolicies)]
        public async Task<IActionResult> GetAllPoliciesPaginatedList([FromQuery] PaginatedListRequest request)
        {
            var response = await _insuranceService.GetAllPoliciesPaginatedList(request);
            return Ok(response);
        }

        [HttpGet(WebApiEndpoint.InsurancePolicyEndpoint.GetPolicyById)]
        public async Task<IActionResult> GetPolicyById([FromRoute] string id)
        {
            var response = await _insuranceService.GetPolicyById(id);
            return Ok(response);
        }
    }
}
