using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.RefreshToken;
using MetroShip.Service.ApiModels.User;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Utils;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace MetroShip.WebAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthController(IAuthService authSevices, IUserService userService)
        {
            _userService = userService;
            _authService = authSevices;
            _httpContextAccessor = new HttpContextAccessor();
        }

        [HttpGet(WebApiEndpoint.Authentication.Hello)]
        public IActionResult Hello()
        {
            return Ok("Hello");
        }

        [Authorize(Roles = nameof(UserRoleEnum.Staff), 
            Policy = $"RequireAssignmentRole.{nameof(AssignmentRoleEnum.Checker)}")]
        [HttpGet(WebApiEndpoint.Authentication.Hello + "/{shipmentId}")]
        public IActionResult Hello1([FromRoute] string shipmentId)
        {
            var stationId = JwtClaimUltils.GetUserStation(_httpContextAccessor);
            var assignmentRole = JwtClaimUltils.GetAssignmentRole(_httpContextAccessor);
            return Ok($"Hello from shipment {shipmentId} with assignmentRole {assignmentRole.ToString()} at station {stationId}");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("admin/roles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _authService.GetAllRoles();
            return Ok(BaseResponse.OkResponseDto(roles));
        }

        [HttpPost]
        [AllowAnonymous]
        [Route(WebApiEndpoint.Authentication.Register)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            await _authService.Register(request);
            return Ok(BaseResponse.OkResponseDto(ResponseMessageIdentitySuccess.REGIST_USER_SUCCESS, null));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route("admin/register")]
        public async Task<IActionResult> RegisterByAdmin([FromBody] RegisterRequest request, int role)
        {
            await _authService.RegisterByAdmin(request, role);
            return Ok(BaseResponse.OkResponseDto(ResponseMessageIdentitySuccess.REGIST_USER_SUCCESS, null));
        }

        [HttpPost]
        [AllowAnonymous]
        [Route(WebApiEndpoint.Authentication.Login)]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var response = await _authService.Authenticate(request);
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<ActionResult> RefreshToken(RefreshToken request)
        {
            var refreshToken = request.Token ?? Request.Cookies["refreshToken"];
            var response = await _authService.RefreshToken(refreshToken);
            SetTokenCookie(response.RefreshToken);
            return Created(nameof(RefreshToken), response);
        }

        private void SetTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }

        [HttpPost("email/verification")]
        public async Task<IActionResult> VerifyEmail(VerifyEmailRequest request)
        {
            await _authService.VerifyEmail(request);
            return Ok(BaseResponse.OkResponseDto(ResponseMessageIdentitySuccess.VERIFY_EMAIL_SUCCESS, null));
        }

        [HttpPost("password/forgot")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
        {
            await _authService.ForgotPassword(request);
            return Ok(BaseResponse.OkResponseDto(ResponseMessageIdentitySuccess.FORGOT_PASSWORD_SUCCESS, null));
        }

        [HttpPost("password/change")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            await _authService.ChangePassword(request);
            return Ok(BaseResponse.OkResponseDto(ResponseMessageIdentitySuccess.CHANGE_PASSWORD_SUCCESS, null));
        }

        [HttpPost("password/reset")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            await _authService.ResetPassword(request);
            return Ok(BaseResponse.OkResponseDto(ResponseMessageIdentitySuccess.RESET_PASSWORD_SUCCESS, null));
        }

        [HttpPost("email/resend")]
        public async Task<IActionResult> ResendEmail(ResendEmailRequest request)
        {
            await _authService.ReSendEmail(request);
            return Ok(BaseResponse.OkResponseDto(ResponseMessageIdentitySuccess.RESEND_EMAIL_SUCCESS, null));
        }

        [HttpPost("authentication/google")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginModel request)
        {
            return Ok(await _authService.GoogleAuthenticate(request));
        }
    }
}