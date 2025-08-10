using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.User;
using MetroShip.Utility.Enums;

namespace MetroShip.Service.Interfaces;

public interface IUserService
{
    Task<UserListWithStatsResponse> GetAllUsersAsync(PaginatedListRequest paginatedRequest, UserRoleEnum? role, string? searchKeyword = null,
    DateTimeOffset? createdFrom = null, DateTimeOffset? createdTo = null, OrderByRequest? orderByRequest = null);
    Task<string> CreateUserAsync(UserCreateRequest request, CancellationToken cancellationToken = default);
    Task UpdateUserAsync(UserUpdateRequest request);
    Task<UserResponse> GetByIdAsync(object id);
    Task BanUserAsync(object id);
}