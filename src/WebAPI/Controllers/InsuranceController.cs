using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.InsurancePolicy;
using MetroShip.Service.ApiModels.MetroLine;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.Interfaces;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Enums;
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
        [ProducesResponseType(typeof(BaseResponse<PaginatedListResponse<InsurancePolicyResponse>>), 200)]
        public async Task<IActionResult> GetAllPoliciesPaginatedList([FromQuery] PaginatedListRequest request, [FromQuery] bool? isActive = null)
        {
            var response = await _insuranceService.GetAllPoliciesPaginatedList(request, isActive);
            return Ok(response);
        }

        [HttpGet(WebApiEndpoint.InsurancePolicyEndpoint.GetPolicyById)]
        [ProducesResponseType(typeof(BaseResponse<InsurancePolicyResponse>), 200)]
        public async Task<IActionResult> GetPolicyById([FromRoute] string id)
        {
            var response = await _insuranceService.GetPolicyById(id);
            return Ok(response);
        }

        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        [HttpPost(WebApiEndpoint.InsurancePolicyEndpoint.CreatePolicy)]
        public async Task<IActionResult> CreatePolicy([FromBody] InsurancePolicyRequest request)
        {
            var response = await _insuranceService.CreatePolicy(request);
            return Ok(BaseResponse.OkResponseDto(response));
        }

        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        [HttpPut(WebApiEndpoint.InsurancePolicyEndpoint.ActivatePolicy)]
        public async Task<IActionResult> ActivatePolicy([FromRoute] string id)
        {
            var response = await _insuranceService.ActivatePolicy(id);
            return Ok(BaseResponse.OkResponseDto(response));
        }

        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        [HttpPut(WebApiEndpoint.InsurancePolicyEndpoint.DeactivatePolicy)]
        public async Task<IActionResult> DeactivatePolicy([FromRoute] string id)
        {
            var response = await _insuranceService.DeactivatePolicy(id);
            return Ok(BaseResponse.OkResponseDto(response));
        }

        [HttpGet(WebApiEndpoint.InsurancePolicyEndpoint.GetAllActivePoliciesDropdown)]
        public async Task<IActionResult> GetAllActivePoliciesDropdown()
        {
            var response = await _insuranceService.GetAllActivePoliciesDropdown();
            return Ok(BaseResponse.OkResponseDto(response));
        }

        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        [HttpDelete(WebApiEndpoint.InsurancePolicyEndpoint.DeletePolicy)]
        public async Task<IActionResult> DeletePolicy([FromRoute] string id)
        {
            var response = await _insuranceService.DeletePolicy(id);
            return Ok(BaseResponse.OkResponseDto(response));
        }
    }
}
