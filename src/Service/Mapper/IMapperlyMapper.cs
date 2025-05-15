using MetroShip.Repository.Extensions;
using MetroShip.Repository.Models.Identity;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.User;

namespace MetroShip.Service.Mapper;

public interface IMapperlyMapper
{
    // user
    IList<RoleResponse> MapToRoleResponseList(IList<RoleEntity> entity);
    UserEntity MapToUserEntity(RegisterRequest request);
    LoginResponse MapToLoginResponse(UserEntity entity);

    UserEntity MapToUserEntity(UserCreateRequest request);
    void MapUserRequestToEntity(UserUpdateRequest request, UserEntity entity);

    IList<UserResponse> MapToUserResponseList(IList<UserEntity> entity);
    IQueryable<UserResponse> MapToUserResponseList(IQueryable<UserEntity> entity);
    PaginatedListResponse<UserResponse> MapToUserResponsePaginatedList(PaginatedList<UserEntity> entity);
    UserResponse MapToUserResponse(UserEntity entity);
    void MapRegisterRequestToEntity(RegisterRequest request, UserEntity entity);

    int? MapToVoucherId(int? voucherId);

    DateOnly MapDateTimeOffsetToDateOnly(DateTimeOffset dateTimeOffset);
    DateOnly MapDateTimeToDateOnly(DateTime dateTime);

    IList<double> MapJsonStringToDoubleList(string jsonString);

    IList<string?> MapRoleToRoleName(IEnumerable<UserRoleEntity> entity);
}