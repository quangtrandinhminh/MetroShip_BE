using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models.Identity;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.User;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Mapper;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Enums;
using MetroShip.Utility.Exceptions;
using MetroShip.Utility.Helpers;

namespace MetroShip.Service.Services;

public class UserService(IServiceProvider serviceProvider) : IUserService
{
    private readonly IUserRepository _userRepository = serviceProvider.GetRequiredService<IUserRepository>();
    private readonly IMapperlyMapper _mapper = serviceProvider.GetRequiredService<IMapperlyMapper>();
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
    private readonly UserManager<UserEntity> _userManager = serviceProvider.GetRequiredService<UserManager<UserEntity>>();
    private readonly IUnitOfWork _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();

    public async Task<PaginatedListResponse<UserResponse>> GetAllUsersAsync(UserRoleEnum? role, int pageNumber, int pageSize)
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
        var user = _mapper.MapToUserEntity(request);
        await _userRepository.CreateUserAsync(user);
    }

    public async Task UpdateUserAsync(UserUpdateRequest request)
    {
        _logger.Information("Update user {@request}", request);
        var user = await GetUserById(request.Id);
        _mapper.MapUserRequestToEntity(request, user);

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangeAsync();
    }

    public async Task<UserResponse> GetByIdAsync(string id)
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

    public async Task DeleteUserAsync(int id)
    {
        _logger.Information($"Delete user by id {id}");
        var user = await GetUserById(id);
        user.DeletedTime = CoreHelper.SystemTimeNow;
        await _userRepository.UpdateAsync(user);
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