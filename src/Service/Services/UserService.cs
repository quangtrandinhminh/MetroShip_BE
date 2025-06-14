﻿using Microsoft.AspNetCore.Http;
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
using Microsoft.EntityFrameworkCore;
using Twilio.Rest.Api.V2010.Account;

namespace MetroShip.Service.Services;

public class UserService(IServiceProvider serviceProvider) : IUserService
{
    private readonly IUserRepository _userRepository = serviceProvider.GetRequiredService<IUserRepository>();
    private readonly IMapperlyMapper _mapper = serviceProvider.GetRequiredService<IMapperlyMapper>();
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
    private readonly UserManager<UserEntity> _userManager = serviceProvider.GetRequiredService<UserManager<UserEntity>>();
    private readonly IUnitOfWork _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
    private readonly IHttpContextAccessor _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    private readonly UserValidator _userValidator = new();
    private readonly IEmailService _emailService = serviceProvider.GetRequiredService<IEmailService>();
    private readonly RoleManager<RoleEntity> _roleManager = serviceProvider.GetRequiredService<RoleManager<RoleEntity>>();

    public async Task<PaginatedListResponse<UserResponse>> GetAllUsersAsync(int pageNumber, int pageSize, UserRoleEnum? role)
    {
        _logger.Information($"Get all users by role {role.ToString()}");
        Expression<Func<UserEntity, bool>> predicate = v => v.DeletedTime == null;

        List<RoleEntity> roleEntity;
        if (role != null)
        {
            predicate = predicate.And(x => x.UserRoles.Any(y => y.Role.Name == role.ToString()));
            roleEntity = await _roleManager.Roles.Where(x => x.Name == role.ToString()).ToListAsync();
        }
        else
        {
            roleEntity = await _roleManager.Roles.ToListAsync();
        }

        var users = await _userRepository.GetAllPaginatedQueryable(
            pageNumber, pageSize,
            predicate, null, e => e.UserRoles);

        var userResponse = _mapper.MapToUserResponsePaginatedList(users);
        foreach (var user in userResponse.Items)
        {
            user.Role = _mapper.MapRoleToRoleName(roleEntity);
        }
        return userResponse;
    }

    public async Task CreateUserAsync(UserCreateRequest request, CancellationToken cancellationToken = default)
    {
        _logger.Information("Create user {@request}", request);
        _userValidator.ValidateUserCreateRequest(request);
        // parse role from request
        if (!Enum.TryParse<UserRoleEnum>(request.Role.ToString(), out var role))
        {
            throw new AppException(HttpResponseCodeConstants.BAD_REQUEST, ResponseMessageIdentity.ROLE_INVALID, 
                StatusCodes.Status400BadRequest);
        }
        // check role is valid in system
        var roleEntity = await _roleManager.FindByNameAsync(role.ToString());
        if (roleEntity == null)
        {
            throw new AppException(HttpResponseCodeConstants.BAD_REQUEST, 
                ResponseMessageIdentity.ROLE_INVALID, StatusCodes.Status400BadRequest);
        }

        // get user by name
        var validateUser = await _userManager.FindByNameAsync(request.UserName);
        if (validateUser != null)
        {
            throw new AppException(HttpResponseCodeConstants.EXISTED, 
                ResponseMessageIdentity.EXISTED_USER, StatusCodes.Status400BadRequest);
        }

        var existingUserWithEmail = await _userManager.FindByEmailAsync(request.Email);
        if (existingUserWithEmail != null)
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
            account.Verified = CoreHelper.SystemTimeNow;
            await _userRepository.CreateUserAsync(account, cancellationToken);
            var roleIds = new List<string> { roleEntity.Id };
            await _userRepository.AddUserToRoleAsync(account.Id, roleIds, cancellationToken);
            var count = await _userRepository.SaveChangeAsync();
            if (count > 0)
            {
                // send email to user
                _logger.Information("Send email to user {@email}", account.Email);
                var sendMailModel = new SendMailModel
                {
                    Email = account.Email,
                    Type = MailTypeEnum.Account,
                    Name = account.FullName,
                    UserName = account.UserName,
                    Password = request.Password,
                    Role = role.ToString()
                };
                _emailService.SendMail(sendMailModel);
            }
        }
        catch (Exception e)
        {
            throw new AppException(HttpResponseCodeConstants.FAILED, e.Message, StatusCodes.Status400BadRequest);
        }
    }

    public async Task UpdateUserAsync(UserUpdateRequest request)
    {
        _logger.Information("Update user {@request}", request);
        _userValidator.ValidateUserUpdateRequest(request);
        var user = await GetUserById(request.Id);
        if (user.UserName != request.UserName)
        {
            var validateUser = await _userManager.FindByNameAsync(request.UserName);
            if (validateUser != null)
            {
                throw new AppException(HttpResponseCodeConstants.EXISTED, 
                    ResponseMessageIdentity.EXISTED_USER, 
                    StatusCodes.Status409Conflict);
            }
            /*var existingUser = await _userRepository.GetSingleAsync(u => u.UserName == request.UserName);
            if (existingUser != null)
            {
                throw new AppException(
                HttpResponseCodeConstants.DUPLICATE,
                ResponseMessageConstantsUser.USER_EXISTED,
                StatusCodes.Status409Conflict);
            }*/
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
            throw new AppException(HttpResponseCodeConstants.NOT_FOUND, 
                ResponseMessageConstantsUser.USER_NOT_FOUND, StatusCodes.Status404NotFound);
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
            throw new AppException(HttpResponseCodeConstants.NOT_FOUND, 
                ResponseMessageConstantsUser.USER_NOT_FOUND, StatusCodes.Status404NotFound);
        }

        return user;
    }
}