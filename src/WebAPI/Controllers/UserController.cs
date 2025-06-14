﻿using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.PaginatedList;
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

        [HttpGet]
        [Route(WebApiEndpoint.User.GetUserRoles)]
        public IActionResult GetUserRoles()
        {
            return Ok(BaseResponse.OkResponseDto(_enumResponses));
        }

        [HttpGet]
        [Route(WebApiEndpoint.User.GetUsers)]
        public async Task<IActionResult> GetUser([FromQuery] PaginatedListRequest request, UserRoleEnum? role)
        {
            return Ok(BaseResponse.OkResponseDto(
                await _userService.GetAllUsersAsync(request.PageNumber, request.PageSize, role),
                _enumResponses
                ));
        }

        [HttpGet]
        [Route(WebApiEndpoint.User.GetUser)]
        public async Task<IActionResult> GetUser([FromRoute] string id)
        {
            return Ok(BaseResponse.OkResponseDto(await _userService.GetByIdAsync(id)));
        }

        [HttpPost]
        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        [Route(WebApiEndpoint.User.CreateUser)]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateRequest request)
        {
            await _userService.CreateUserAsync(request);
            return Ok(BaseResponse.OkResponseDto(ResponseMessageConstantsUser.CREATE_USER_SUCCESS, null));
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
        public async Task<IActionResult> DeleteUser([FromRoute] int id)
        {
            await _userService.DeleteUserAsync(id);
            return Ok(BaseResponse.OkResponseDto(ResponseMessageConstantsUser.DELETE_USER_SUCCESS, null));
        }
    }
}
