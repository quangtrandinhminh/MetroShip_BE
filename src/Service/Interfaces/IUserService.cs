using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.User;
using MetroShip.Utility.Enums;

namespace MetroShip.Service.Interfaces;

public interface IUserService
{
    Task<PaginatedListResponse<UserResponse>> GetAllUsersAsync(UserRoleEnum? role, int pageNumber, int pageSize);
    Task CreateUserAsync(UserCreateRequest request);
    Task UpdateUserAsync(UserUpdateRequest request);
    Task<UserResponse> GetByIdAsync(string id);
    Task DeleteUserAsync(int id);
}