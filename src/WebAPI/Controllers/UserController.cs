using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Pricing;
using MetroShip.Service.ApiModels.StaffAssignment;
using MetroShip.Service.ApiModels.User;
using MetroShip.Service.Helpers;
using MetroShip.Service.Interfaces;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MetroShip.WebAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/users")]
    public class UserController(IServiceProvider serviceProvider) : Controller
    {
        private readonly IUserService _userService = serviceProvider.GetRequiredService<IUserService>();
        private readonly IList<EnumResponse> _enumResponses = EnumHelper.GetEnumList<UserRoleEnum>();
        private readonly IStaffAssignmentService _staffAssignmentService = serviceProvider.GetRequiredService<IStaffAssignmentService>();

        [HttpGet]
        [Route(WebApiEndpoint.User.GetUserRoles)]
        public IActionResult GetUserRoles()
        {
            return Ok(BaseResponse.OkResponseDto(_enumResponses));
        }

        [HttpGet]
        [Route(WebApiEndpoint.User.GetUsers)]
        [ProducesResponseType(typeof(BaseResponse<PaginatedListResponse<UserResponse>>), 200)]
        public async Task<IActionResult> GetUser([FromQuery] PaginatedListRequest request, [FromQuery] UserRoleEnum? role, 
            [FromQuery] string? searchKeyword, [FromQuery] DateTimeOffset? createdFrom, [FromQuery] DateTimeOffset? createdTo, 
            [FromQuery] OrderByRequest? orderBy)
        {
            var result = await _userService.GetAllUsersAsync(
                request, role, searchKeyword, createdFrom, createdTo, orderBy);

            return Ok(BaseResponse.OkResponseDto(result, _enumResponses));
        }

        [HttpGet]
        [Route(WebApiEndpoint.User.GetUser)]
        [ProducesResponseType(typeof(BaseResponse<UserResponse>), 200)]
        public async Task<IActionResult> GetUser([FromRoute] string id)
        {
            return Ok(BaseResponse.OkResponseDto(await _userService.GetByIdAsync(id)));
        }

        [HttpPost]
        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        [Route(WebApiEndpoint.User.CreateUser)]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateRequest request)
        {
            var userId = await _userService.CreateUserAsync(request);
            return Ok(BaseResponse.OkResponseDto(ResponseMessageConstantsUser.CREATE_USER_SUCCESS, 
                new { userId = userId }));
        }

        [HttpPut]
        [Route(WebApiEndpoint.User.UpdateUser)]
        public async Task<IActionResult> UpdateUser([FromBody] UserUpdateRequest request)
        {
            await _userService.UpdateUserAsync(request);
            return Ok(BaseResponse.OkResponseDto(ResponseMessageConstantsUser.UPDATE_USER_SUCCESS , null));
        }

        [HttpDelete]
        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        [Route(WebApiEndpoint.User.DeleteUser)]
        public async Task<IActionResult> BanUser([FromRoute] string id)
        {
            await _userService.BanUserAsync(id);
            return Ok(BaseResponse.OkResponseDto(ResponseMessageConstantsUser.BAN_USER_SUCCESS, null));
        }

        [HttpPost]
        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        [Route(WebApiEndpoint.User.AssignRoleToStaff)]
        public async Task<IActionResult> AssignRole([FromBody] StaffAssignmentRequest request)
        {
            var result = await _staffAssignmentService.AssignAsync(request);
            return Ok(BaseResponse.OkResponseDto(result, null));
        }

        [HttpDelete]
        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        [Route(WebApiEndpoint.User.RevokeStaffAssignment)]
        public async Task<IActionResult> RevokeAssignment([FromRoute] string staffId)
        {
            var result = await _staffAssignmentService.DeactivateAssignmentByStaffId(staffId);
            return Ok(BaseResponse.OkResponseDto(result, null, result));
        }

        // Get all assignmentRole enums
        [HttpGet]
        [Route(WebApiEndpoint.User.GetAssignmentRoles)]
        public IActionResult GetAssignmentRoles()
        {
            var assignmentRoles = EnumHelper.GetEnumList<AssignmentRoleEnum>();
            return Ok(BaseResponse.OkResponseDto(assignmentRoles));
        }

        [HttpPut]
        [Route(WebApiEndpoint.User.UpdateBankInfo)]
        public async Task<IActionResult> UpdateBankInfo([FromBody] BankInfoRequest request)
        {
            await _userService.UpdateUserAsync(request);
            return Ok(BaseResponse.OkResponseDto(ResponseMessageConstantsUser.UPDATE_BANK_INFO_SUCCESS, null));
        }
    }
}
