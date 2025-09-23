using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Google.Apis.Auth;
using Invedia.Core.StringUtils;
using MetroShip.Repository.Base;
using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.User;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Mapper;
using MetroShip.Service.Utils;
using MetroShip.Service.Validations;
using MetroShip.Utility.Config;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Enums;
using MetroShip.Utility.Exceptions;
using MetroShip.Utility.Helpers;
using MetroShip.Service.ApiModels.RefreshToken;
using System;
using System.Security;

namespace MetroShip.Service.Services
{
    public class AuthService(IServiceProvider serviceProvider) : IAuthService
    {
        private readonly IUserRepository _userRepository = serviceProvider.GetRequiredService<IUserRepository>();
        private readonly IMapperlyMapper _mapper = serviceProvider.GetRequiredService<IMapperlyMapper>();
        private readonly RoleManager<RoleEntity> _roleManager = serviceProvider.GetRequiredService<RoleManager<RoleEntity>>();
        private readonly UserManager<UserEntity> _userManager = serviceProvider.GetRequiredService<UserManager<UserEntity>>();
        private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
        //private readonly IBaseRepository<RefreshToken> _refreshTokenRepository;
        private readonly IEmailService _emailService = serviceProvider.GetRequiredService<IEmailService>();
        private readonly IUnitOfWork _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
        private readonly IStaffAssignmentService _staffAssignmentService = serviceProvider.GetRequiredService<IStaffAssignmentService>();

        // get all roles
        public async Task<IList<RoleResponse>> GetAllRoles()
        {
            _logger.Information("Get all roles");
            var roles = await _roleManager.Roles.ToListAsync();
            return _mapper.MapToRoleResponseList(roles);
        }

        public async Task<LoginResponse> Authenticate(LoginRequest request)
        {
            _logger.Information("Authenticate user: {@request}", request.Username);
            var account = await GetUserByUserName(request.Username);
            AuthValidator.ValidateLogin(request, account);

            try
            {
                var roles = await _userManager.GetRolesAsync(account);
                var token = await GenerateJwtToken(account, roles, 24);

                await GenerateRefreshToken(account, 48);
                var count = await _unitOfWork.SaveChangeAsync();

                var response = _mapper.MapToLoginResponse(account);
                response.Token = token;
                response.Role = roles;
                return response;
            }
            catch (Exception e)
            {
                throw new AppException(HttpResponseCodeConstants.FAILED, e.Message, StatusCodes.Status400BadRequest);
            }
        }

        public async Task Register(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            _logger.Information("Register new user: {@request}", request.UserName);
            AuthValidator.ValidateRegisterRequest(request);
            // check if user is existed in system
            var validateUser = await _userRepository.IsExistAsync(
                u => u.UserName.Equals(request.UserName)
                     || u.NormalizedUserName.Equals(request.UserName.Normalize()));
            if (validateUser)
            {
                throw new AppException(HttpResponseCodeConstants.EXISTED,
                    ResponseMessageIdentity.EXISTED_USER, StatusCodes.Status400BadRequest);
            }

            var existingUserWithEmail = await _userRepository.IsExistAsync(
                x => x.Email == request.Email || x.NormalizedEmail == request.Email.Normalize());
            if (existingUserWithEmail)
            {
                throw new AppException(HttpResponseCodeConstants.EXISTED,
                    ResponseMessageIdentity.EXISTED_EMAIL, StatusCodes.Status400BadRequest);
            }

            var existingUserWithPhone = await _userManager.Users.FirstOrDefaultAsync(
                x => x.PhoneNumber == request.PhoneNumber, cancellationToken);
            if (existingUserWithPhone != null)
            {
                throw new AppException(HttpResponseCodeConstants.EXISTED,
                    ResponseMessageIdentity.EXISTED_PHONE, StatusCodes.Status400BadRequest);
            }

            try
            {
                var account = _mapper.MapToUserEntity(request);
                account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                account.SecurityStamp = Guid.NewGuid().ToString();
                account.OTP = GenerateOTP();
                account.NormalizedUserName = _userManager.NormalizeName(request.UserName);
                account.NormalizedEmail = _userManager.NormalizeEmail(request.Email);
                await _userRepository.CreateUserAsync(account, cancellationToken);
                var roleEntity = await _roleManager.FindByNameAsync(UserRoleEnum.Customer.ToString());
                var roleIds = new List<string> { roleEntity.Id };
                await _userRepository.AddUserToRoleAsync(account.Id, roleIds, cancellationToken);
                var count = await _userRepository.SaveChangeAsync();

                if (count > 0)
                {
                    _logger.Information("Scheduling to send email to user {@email}", account.Email);
                    var mailRequest = new SendMailModel()
                    {
                        Name = account.NormalizedUserName,
                        Email = account.Email,
                        Token = account.OTP,
                        Type = MailTypeEnum.Verify
                    };
                    //_emailService.SendMail(mailRequest);
                    _emailService.ScheduleEmailJob(mailRequest);
                }
            }
            catch (Exception e)
            {
                throw new AppException(HttpResponseCodeConstants.FAILED, e.Message, StatusCodes.Status400BadRequest);
            }
        }

        // register by admin
        public async Task RegisterByAdmin(RegisterRequest request, int role)
        {
            _logger.Information("Register new user by admin: {@request}", request);
            // check role is valid in system
            var roleEntity = await _roleManager.FindByIdAsync(role.ToString());
            if (roleEntity == null)
            {
                throw new AppException(HttpResponseCodeConstants.NOT_FOUND, ResponseMessageIdentity.ROLE_INVALID, StatusCodes.Status400BadRequest);
            }

            // get user by name
            var validateUser = await _userManager.FindByNameAsync(request.UserName);
            if (validateUser != null)
            {
                throw new AppException(HttpResponseCodeConstants.EXISTED, ResponseMessageIdentity.EXISTED_USER, StatusCodes.Status400BadRequest);
            }

            var existingUserWithEmail = await _userManager.FindByEmailAsync(request.Email);
            if (existingUserWithEmail != null)
            {
                throw new AppException(HttpResponseCodeConstants.EXISTED, ResponseMessageIdentity.EXISTED_EMAIL, StatusCodes.Status400BadRequest);
            }

            var existingUserWithPhone = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == request.PhoneNumber);
            if (existingUserWithPhone != null)
            {
                throw new AppException(HttpResponseCodeConstants.EXISTED, ResponseMessageIdentity.EXISTED_PHONE, StatusCodes.Status400BadRequest);
            }

            if (!string.IsNullOrEmpty(request.PhoneNumber) && !Regex.IsMatch(request.PhoneNumber, @"^\d{10}$"))
            {
                throw new AppException(HttpResponseCodeConstants.INVALID_INPUT, ResponseMessageIdentity.PHONENUMBER_INVALID, StatusCodes.Status400BadRequest);
            }

            if (request.Password != request.ConfirmPassword)
            {
                throw new AppException(HttpResponseCodeConstants.INVALID_INPUT, ResponseMessageIdentity.PASSWORD_NOT_MATCH, StatusCodes.Status400BadRequest);
            }

            try
            {
                var account = _mapper.MapToUserEntity(request);
                account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                account.SecurityStamp = Guid.NewGuid().ToString();
                account.Verified = CoreHelper.SystemTimeNow;
                await _userRepository.CreateUserAsync(account);
                await _userRepository.SaveChangeAsync();
                await _userManager.AddToRoleAsync(account, roleEntity.NormalizedName);
            }
            catch (Exception e)
            {
                throw new AppException(HttpResponseCodeConstants.FAILED, e.Message, StatusCodes.Status400BadRequest);
            }
        }

        public async Task<LoginResponse> GoogleAuthenticate(GoogleLoginModel model)
        {
            _logger.Information("Google authenticate: {@model}", model);
            var payload = await ValidateGoogleToken(model.IdToken);
            if (payload == null)
            {
                throw new AppException(ErrorCode.TokenInvalid, ResponseMessageIdentity.GOOGLE_TOKEN_INVALID, StatusCodes.Status401Unauthorized);
            }

            var account = await GetUserByEmail(payload.Email);
            if (account == null)
            {
                account = new UserEntity
                {
                    UserName = payload.Email,
                    Email = payload.Email,
                    FullName = payload.Name,
                    Avatar = payload.Picture,
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    Verified = CoreHelper.SystemTimeNow
                };
                await _userRepository.CreateUserAsync(account);
                await _userRepository.SaveChangeAsync();
                await _userManager.AddToRoleAsync(account, UserRoleEnum.Customer.ToString());
            }

            var roles = await _userManager.GetRolesAsync(account);
            var token = await GenerateJwtToken(account, roles, 24);
            //var refreshToken = GenerateRefreshToken(account.Id, 48);
            // RemoveOldRefreshTokens(account.RefreshTokens);
            // await _refreshTokenRepository.AddAsync(refreshToken);
            await GenerateRefreshToken(account, 48);
            await _unitOfWork.SaveChangeAsync();

            var response = _mapper.MapToLoginResponse(account);
            response.Token = token;
            // response.RefreshToken = refreshToken.Token;
            // response.RefreshTokenExpiredTime = refreshToken.Expires;
            response.Role = roles;
            return response;
        }

        public async Task<LoginResponse> RefreshToken(string token)
        {
            _logger.Information("Refresh token: {@token}", token);
            var account = await GetUserByRefreshToken(token);
            await GenerateRefreshToken(account, 48);
            var count = await _unitOfWork.SaveChangeAsync();

            try
            {
                var roles = await _userManager.GetRolesAsync(account);
                var jwtToken = await GenerateJwtToken(account, roles, 24);
                var response = _mapper.MapToLoginResponse(account);
                response.Token = jwtToken;
                response.Role = roles;
                return response;
            }
            catch (Exception e)
            {
                throw new AppException(HttpResponseCodeConstants.FAILED, e.Message, StatusCodes.Status400BadRequest);
            }
        }

        public async Task VerifyEmail(VerifyEmailRequest request, CancellationToken cancellationToken = default)
        {
            _logger.Information("Verify email: {@request}", request);
            var account = await GetUserByUserName(request.UserName);

            if (account == null || account.OTP != request.OTP)
                throw new AppException(ErrorCode.TokenInvalid
                    , ResponseMessageIdentity.OTP_INVALID, StatusCodes.Status401Unauthorized);

            account.Verified = CoreHelper.SystemTimeNow;
            account.OTP = null;
            await _userRepository.UpdateAsync(account, cancellationToken);
            await _userRepository.SaveChangeAsync();
        }

        /// <summary>
        /// Send mail to user to reset password
        /// </summary>
        /// <param name="model"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task ForgotPassword(ForgotPasswordRequest model, CancellationToken cancellationToken = default)
        {
            _logger.Information("Forgot password: {@model}", model);
            //var account = await GetUserByUserName(model.UserName);
            var account = await GetUserByEmail(model.Email);
            if (account == null) throw new AppException(ErrorCode.UserInvalid, ResponseMessageIdentity.INVALID_USER, StatusCodes.Status401Unauthorized);

            // create reset token that expires after 1 day
            account.OTP = GenerateOTP();
            await _userRepository.UpdateAsync(account, cancellationToken);

            var mailRequest = new SendMailModel
            {
                Name = account.NormalizedUserName,
                Email = account.Email,
                Token = account.OTP,
                Type = MailTypeEnum.ResetPassword
            };
            //_emailService.SendMail(mailRequest);
            _emailService.ScheduleEmailJob(mailRequest);
        }

        /// <summary>
        /// Reset password for user after verify OTP
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task ResetPassword(ResetPasswordRequest dto, CancellationToken cancellationToken = default)
        {
            _logger.Information("Reset password: {@dto}", dto);
            var account = await GetUserByEmail(dto.Email);

            if (account == null) 
                throw new AppException(ErrorCode.UserInvalid, ResponseMessageIdentity.INVALID_USER, StatusCodes.Status401Unauthorized);

            if (account.OTP != dto.OTP)
            {
                throw new AppException(ErrorCode.TokenInvalid, ResponseMessageIdentity.OTP_INVALID, StatusCodes.Status401Unauthorized);
            }

            account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            account.OTP = null;
            account.AccessFailedCount = 0;
            account.LastUpdatedTime = CoreHelper.SystemTimeNow;
            account.LastUpdatedBy = account.Id;

            await _userRepository.UpdateAsync(account, cancellationToken);
        }

        public async Task ChangePassword(ChangePasswordRequest dto, CancellationToken cancellationToken)
        {
            _logger.Information("Change password: {@dto}", dto);
            var account = await GetUserByUserName(dto.UserName);

            if (account == null || !account.IsActive) throw new AppException(ErrorCode.UserInvalid, ResponseMessageIdentity.INVALID_USER, StatusCodes.Status401Unauthorized);

            if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, account.PasswordHash))
            {
                throw new AppException(ErrorCode.UserPasswordWrong, ResponseMessageIdentity.UNAUTHENTICATED, StatusCodes.Status401Unauthorized);
            }

            account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            account.LastUpdatedTime = CoreHelper.SystemTimeNow;
            account.LastUpdatedBy = account.Id;

            await _userRepository.UpdateAsync(account, cancellationToken);
        }

        public async Task ReSendEmail(ResendEmailRequest model, CancellationToken cancellationToken = default)
        {
            _logger.Information("Resend email: {@model}", model);
            var account = await GetUserByUserName(model.UserName);
            if (account == null) throw new AppException(ErrorCode.UserInvalid, ResponseMessageIdentity.INVALID_USER, StatusCodes.Status400BadRequest);
            if (account.OTP == null) throw new AppException(ErrorCode.Validated, ResponseMessageIdentity.EMAIL_VALIDATED, StatusCodes.Status400BadRequest);

            account.OTP = GenerateOTP();
            await _userRepository.UpdateAsync(account, cancellationToken);

            var mailRequest = new SendMailModel()
            {
                Name = account.NormalizedUserName,
                Email = account.Email,
                Token = account.OTP,
                Type = MailTypeEnum.Verify
            };
            //_emailService.SendMail(mailRequest);
            _emailService.ScheduleEmailJob(mailRequest);
        }

        private async Task<string> GenerateJwtToken(UserEntity loggedUser, IList<string> roles, int hour)
        {
            var claims = new List<Claim>();
            claims.AddRange(
                await _userManager.GetClaimsAsync(loggedUser)
                );
            // Add role claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));

                // Use RoleManager to find the role and add its claims
                var roleEntity = await _roleManager.FindByNameAsync(role);
                if (roleEntity != null)
                {
                    var roleClaims = await _roleManager.GetClaimsAsync(roleEntity);
                    claims.AddRange(roleClaims);
                }
            }

            // Add permission claims
            if (roles.Contains(UserRoleEnum.Staff.ToString()))
            {
                var assignedRole = await _staffAssignmentService.GetByStaffIdAsync(loggedUser.Id);

                if (assignedRole != null)
                {
                    // Add AssignmentRole claim based on assigned role
                    claims.Add(new Claim("AssignmentRole", assignedRole.AssignedRole.ToString()));

                    // Add station claim if user is assigned to a station
                    claims.Add(new Claim("StationId", assignedRole.StationId));
                }
            }

            claims.AddRange(new[]
            {
                new Claim(ClaimTypes.Sid, loggedUser.Id.ToString()),
                new Claim("UserName", loggedUser.UserName ?? string.Empty),
                new Claim(ClaimTypes.Name, loggedUser.FullName ?? string.Empty),
                new Claim(ClaimTypes.Email, loggedUser.Email ?? string.Empty),
                new Claim(ClaimTypes.MobilePhone, loggedUser.PhoneNumber ?? string.Empty),
                new Claim(ClaimTypes.Expired, CoreHelper.SystemTimeNow.AddHours(hour).Date.ToShortDateString())
            });

            return JwtUtils.GenerateToken(claims.Distinct(), hour);
        }

        private async Task GenerateRefreshToken(UserEntity user, int hour)
        {
            var randomByte = new byte[64];
            var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            rngCryptoServiceProvider.GetBytes(randomByte);
            user.RefreshToken = Convert.ToBase64String(randomByte);
            user.RefreshTokenExpiredTime = CoreHelper.SystemTimeNow.AddHours(hour);
            await _userRepository.UpdateAsync(user);
        }

        private async Task<UserEntity> GetUserByRefreshToken(string token)
        {
            var account = await _userRepository.GetSingleAsync(
                y=> y.RefreshToken.Equals(token));
            if (account == null || account.DeletedTime != null)
            {
                throw new AppException(ErrorCode.TokenInvalid, 
                    ResponseMessageIdentity.REFRESH_TOKEN_INVALID, 
                    StatusCodes.Status401Unauthorized);
            }

            if (account.IsRefreshTokenExpired)
            {
                throw new AppException(ErrorCode.TokenExpired, 
                    ResponseMessageIdentity.REFRESH_TOKEN_EXPIRED, 
                    StatusCodes.Status401Unauthorized);
            }

            return account;
        }

        public string GenerateOTP()
        {
            var otp = StringHelper.Generate(6, false, false, true, false);
            return otp;
        }

        private async Task<UserEntity?> GetUserByUserName(string userName, CancellationToken cancellationToken = default)
        {
            userName = _userManager.NormalizeName(userName);
            var user = await _userRepository.GetSingleAsync(_ => _.NormalizedUserName == userName);

            if (user != null && user.DeletedTime != null)
            {
                user = null;
            }

            return user;
        }

        private async Task<UserEntity?> GetUserByEmail(string email, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetSingleAsync(_ => _.Email == email);

            if (user != null && user.DeletedTime != null)
            {
                user = null;
            }

            return user;
        }

        private async Task<GoogleJsonWebSignature.Payload?> ValidateGoogleToken(string idToken)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string>() { GoogleSetting.Instance.ClientID }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                return payload;
            }
            catch (Exception e)
            {
                _logger.Error(e, "An error occurred while validating Google token");
            }

            return null;
        }

        private async Task<UserEntity?> GetUserByPhone(string phoneNumber
            , CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetSingleAsync(_ => _.PhoneNumber == phoneNumber);

            if (user != null && user.DeletedTime != null)
            {
                user = null;
            }

            return user;
        }

        private async Task<LoginResponse> GenerateLoginResponse(UserEntity account, IList<string> roles)
        {
            var token = await GenerateJwtToken(account, roles, 24);
            await GenerateRefreshToken(account, 48);
            var count = await _unitOfWork.SaveChangeAsync();

            var response = _mapper.MapToLoginResponse(account);
            response.Token = token;
            response.Role = roles;
            return response;
        }

        private async Task<string> GenerateOTPByTwilio (string phoneNumber)
        {
            var otp = StringHelper.Generate(6, false, false, true, false);
            var message = $"Your OTP is: {otp}";

            return otp;
        }

        public async Task VerifyPhone(PhoneLoginRequest request, CancellationToken cancellationToken = default)
        {
            _logger.Information("Verify phone: {@request}", request);

            /*account.Verified = CoreHelper.SystemTimeNow;
            account.OTP = null;
            await _userRepository.UpdateAsync(account, cancellationToken);
            await _userRepository.SaveChangeAsync();*/
        }

        public async Task<LoginResponse> AuthenticateByPhone(PhoneLoginRequest request)
        {
            _logger.Information("Authenticate user by phone: {@request}", request.PhoneNumber);
            AuthValidator.ValidatePhoneLogin(request);
            var account = await GetUserByPhone(request.PhoneNumber);
            if (account == null)
            {
                throw new AppException(HttpResponseCodeConstants.NOT_FOUND, 
                    ResponseMessageIdentity.INVALID_USER, StatusCodes.Status401Unauthorized);
            }

            try
            {
                return await GenerateLoginResponse(account, new List<string> { UserRoleEnum.Customer.ToString() });
            }
            catch (Exception e)
            {
                throw new AppException(HttpResponseCodeConstants.FAILED, e.Message, StatusCodes.Status400BadRequest);
            }
        }

        #region RefreshToken with a separate table
        /*public async Task<LoginResponse> Authenticate(LoginRequest request)
        {
            _logger.Information("Authenticate user: {@request}", request.Username);
            var account = await GetUserByUserName(request.Username);
            AuthValidator.ValidateLogin(request, account);

            try
            {
                var roles = await _userManager.GetRolesAsync(account);
                var token = await GenerateJwtToken(account, roles, 24);
                var refreshToken = GenerateRefreshToken(account.Id, 48);
                RemoveOldRefreshTokens(account.RefreshTokens);
                await _refreshTokenRepository.AddAsync(refreshToken);
                var count = await _unitOfWork.SaveChangeAsync();

                var response = _mapper.MapToLoginResponse(account);
                response.Token = token;
                response.RefreshToken = refreshToken.Token;
                response.RefreshTokenExpiredTime = refreshToken.Expires;
                response.Role = roles;
                return response;
            }
            catch (Exception e)
            {
                throw new AppException(HttpResponseCodeConstants.FAILED, e.Message, StatusCodes.Status400BadRequest);
            }
        }
        public async Task<LoginResponse> RefreshToken(string token)
        {
            _logger.Information("Refresh token: {@token}", token);
            var (refreshToken, account) = await GetRefreshToken(token);
            refreshToken.Expires = CoreHelper.SystemTimeNow;
            _refreshTokenRepository.Update(refreshToken);
            var newRefreshToken = GenerateRefreshToken(account.Id, 48);

            newRefreshToken.UserId = account.Id;
            await _refreshTokenRepository.AddAsync(newRefreshToken);
            RemoveOldRefreshTokens(account.RefreshTokens);
            var count = await _unitOfWork.SaveChangeAsync();

            try
            {
                var roles = await _userManager.GetRolesAsync(account);
                var jwtToken = await GenerateJwtToken(account, roles, 24);
                var response = _mapper.MapToLoginResponse(account);
                response.Token = jwtToken;
                response.RefreshToken = newRefreshToken.Token;
                response.RefreshTokenExpiredTime = refreshToken.Expires;
                response.Role = roles;
                return response;
            }
            catch (Exception e)
            {
                throw new AppException(HttpResponseCodeConstants.FAILED, e.Message, StatusCodes.Status400BadRequest);
            }
        }

        private static RefreshToken GenerateRefreshToken(string userId, int hour)
        {
            var randomByte = new byte[64];
            var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            rngCryptoServiceProvider.GetBytes(randomByte);
            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = Convert.ToBase64String(randomByte),
                Expires = CoreHelper.SystemTimeNow.AddHours(hour),
            };
            return refreshToken;
        }

        private async Task<UserEntity?> GetUserByPhone(string phoneNumber, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetSingleAsync(_ => _.PhoneNumber == phoneNumber
                ,x => x.RefreshTokens);

            if (user != null && user.DeletedTime != null)
            {
                user = null;
            }

            return user;
        }
        private async Task<UserEntity?> GetUserByEmail(string email, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetSingleAsync(_ => _.Email == email
            , x => x.RefreshTokens);

            if (user != null && user.DeletedTime != null)
            {
                user = null;
            }

            return user;
        }

        private async Task<UserEntity?> GetUserByUserName(string userName, CancellationToken cancellationToken = default)
        {
            userName = _userManager.NormalizeName(userName);
            var user = await _userRepository.GetSingleAsync(_ => _.NormalizedUserName == userName
            ,x => x.RefreshTokens);

            if (user != null && user.DeletedTime != null)
            {
                user = null;
            }

            return user;
        }

        /// <summary>
        /// Remove old refresh token from database which is inactive and expired more than 2 days
        /// </summary>
        /// <param name="refreshTokens"></param>
        private void RemoveOldRefreshTokens(ICollection<RefreshToken> refreshTokens)
        {
            var removeList = refreshTokens.Where(x => !x.IsActive
                 && x.CreatedAt.AddDays(2) <= CoreHelper.SystemTimeNow).ToList();
            if (removeList.Any())
            {
                _refreshTokenRepository.DeleteRange(removeList);
            }
        }

        private async Task<(RefreshToken, UserEntity)> GetRefreshToken(string token)
        {
            var account = await _userRepository.GetSingleAsync(y
                                => y.RefreshTokens.Any(t => t.Token == token)
                            , _ => _.RefreshTokens);
            if (account == null || account.DeletedTime != null)
            {
                throw new AppException(ErrorCode.TokenInvalid, ResponseMessageIdentity.OTP_INVALID, StatusCodes.Status401Unauthorized);
            }

            var refreshToken = account.RefreshTokens.Single(x => x.Token == token);
            if (!refreshToken.IsActive)
            {
                throw new AppException(ErrorCode.TokenExpired, ResponseMessageIdentity.OTP_INVALID, StatusCodes.Status401Unauthorized);
            }

            return (refreshToken, account);
        }

        private async Task<LoginResponse> GenerateLoginResponse (UserEntity account, IList<string> roles)
        {
            var token = await GenerateJwtToken(account, roles, 24);
            var refreshToken = GenerateRefreshToken(account.Id, 48);
            RemoveOldRefreshTokens(account.RefreshTokens);
            await _refreshTokenRepository.AddAsync(refreshToken);
            var count = await _unitOfWork.SaveChangeAsync();

            var response = _mapper.MapToLoginResponse(account);
            response.Token = token;
            response.RefreshToken = refreshToken.Token;
            response.RefreshTokenExpiredTime = refreshToken.Expires;
            response.Role = roles;
            return response;
        }*/
        #endregion
    }
}