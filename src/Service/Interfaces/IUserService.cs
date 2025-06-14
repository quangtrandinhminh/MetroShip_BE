﻿using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.User;
using MetroShip.Utility.Enums;

namespace MetroShip.Service.Interfaces;

public interface IUserService
{
    Task<PaginatedListResponse<UserResponse>> GetAllUsersAsync(int pageNumber, int pageSize, UserRoleEnum? role);
    Task CreateUserAsync(UserCreateRequest request, CancellationToken cancellationToken = default);
    Task UpdateUserAsync(UserUpdateRequest request);
    Task<UserResponse> GetByIdAsync(object id);
    Task DeleteUserAsync(object id);
}