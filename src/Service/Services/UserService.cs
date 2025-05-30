using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models.Identity;
using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.User;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Mapper;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Enums;
using MetroShip.Utility.Exceptions;
using MetroShip.Utility.Helpers;
using MetroShip.Service.Validations;

namespace MetroShip.Service.Services;

public class UserService(IServiceProvider serviceProvider) : IUserService
{
    private readonly IUserRepository _userRepository = serviceProvider.GetRequiredService<IUserRepository>();
    private readonly IMapperlyMapper _mapper = serviceProvider.GetRequiredService<IMapperlyMapper>();
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
    private readonly UserManager<UserEntity> _userManager = serviceProvider.GetRequiredService<UserManager<UserEntity>>();
    private readonly IUnitOfWork _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
    private readonly IHttpContextAccessor _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    private readonly UserValidator _userValidator = new UserValidator();
    private readonly IEmailService _emailService = serviceProvider.GetRequiredService<IEmailService>();

    public async Task<PaginatedListResponse<UserResponse>> GetAllUsersAsync(int pageNumber, int pageSize, UserRoleEnum? role)
    {
        _logger.Information($"Get all users by role {role.ToString()}");
        Expression<Func<UserEntity, bool>> predicate = v => v.DeletedTime == null;
        if (role != null)
        {
            predicate = predicate.And(x => x.UserRoles.Any(y => y.Role.Name == role.ToString()));
        }
        var users = await _userRepository.GetAllPaginatedQueryable(
            pageNumber, pageSize,
            predicate, null, e => e.UserRoles);

        return _mapper.MapToUserResponsePaginatedList(users);
    }

    public async Task CreateUserAsync(UserCreateRequest request)
    {
        _logger.Information("Create user {@request}", request);
        _userValidator.ValidateUserCreateRequest(request);
        var existingUser = await _userRepository.GetSingleAsync(u => u.UserName == request.UserName);
        existingUser ??= await _userRepository.GetSingleAsync(u => u.Email == request.Email);
        existingUser ??= await _userRepository.GetSingleAsync(u => u.PhoneNumber == request.PhoneNumber);
        if (existingUser != null)
        {
            throw new AppException(
                HttpResponseCodeConstants.DUPLICATE, 
                ResponseMessageConstantsUser.USER_EXISTED, 
                StatusCodes.Status409Conflict);
        }
        var user = _mapper.MapToUserEntity(request);
        user.Verified = CoreHelper.SystemTimeNow;
        await _userRepository.CreateUserAsync(user);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);

        // send email to user
        _logger.Information("Send email to user {@email}", user.Email);
        var sendMailModel = new SendMailModel
        {
            Email = user.Email,
            Type = MailTypeEnum.Account,
            Name = user.FullName,
            UserName = user.UserName,
            Password = request.Password,
            Role = request.Role?.ToString()
        };
        _emailService.SendMail(sendMailModel);
    }

    public async Task UpdateUserAsync(UserUpdateRequest request)
    {
        _logger.Information("Update user {@request}", request);
        _userValidator.ValidateUserUpdateRequest(request);
        var user = await GetUserById(request.Id);
        if (user.UserName != request.UserName)
        {
            var existingUser = await _userRepository.GetSingleAsync(u => u.UserName == request.UserName);
            if (existingUser != null)
            {
                throw new AppException(
                HttpResponseCodeConstants.DUPLICATE,
                ResponseMessageConstantsUser.USER_EXISTED,
                StatusCodes.Status409Conflict);
            }
        }

        _mapper.MapUserRequestToEntity(request, user);

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
    }

    public async Task<UserResponse> GetByIdAsync(object id)
    {
        _logger.Information($"Get user by id {id}");
        var user = await _userRepository.GetSingleAsync(e => e.Id == id,
            x => x.UserRoles
            );
        if (user == null)
        {
            throw new AppException(HttpResponseCodeConstants.NOT_FOUND, ResponseMessageConstantsUser.USER_NOT_FOUND, StatusCodes.Status404NotFound);
        }

        var response = _mapper.MapToUserResponse(user);
        var roles = await _userManager.GetRolesAsync(user);
        response.Role = roles;
        return response;
    }

    public async Task DeleteUserAsync(object id)
    {
        _logger.Information($"Delete user by id {id}");
        var user = await GetUserById(id);
        user.DeletedTime = CoreHelper.SystemTimeNow;
        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
    }

    private async Task<UserEntity> GetUserById(object id)
    {
        var user = await _userRepository.GetSingleAsync(e => e.Id == id);
        if (user == null)
        {
            throw new AppException(HttpResponseCodeConstants.NOT_FOUND, ResponseMessageConstantsUser.USER_NOT_FOUND, StatusCodes.Status404NotFound);
        }

        return user;
    }
}