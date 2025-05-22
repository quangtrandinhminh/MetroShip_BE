using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.User;

namespace MetroShip.Service.Interfaces
{
    public interface IAuthService
    {
        Task<IList<RoleResponse>> GetAllRoles();
        Task<LoginResponse> Authenticate(LoginRequest request);
        Task Register(RegisterRequest request, CancellationToken cancellationToken = default);
        Task RegisterByAdmin(RegisterRequest request, int role);
        Task<LoginResponse> RefreshToken(string token);
        Task VerifyEmail(VerifyEmailRequest request, CancellationToken cancellationToken = default);
        Task ForgotPassword(ForgotPasswordRequest model, CancellationToken cancellationToken = default);
        Task ResetPassword(ResetPasswordRequest dto, CancellationToken cancellationToken = default);
        Task ChangePassword(ChangePasswordRequest dto, CancellationToken cancellationToken = default);
        Task ReSendEmail(ResendEmailRequest model, CancellationToken cancellationToken = default);
        Task<LoginResponse> GoogleAuthenticate(GoogleLoginModel model);
    }
}