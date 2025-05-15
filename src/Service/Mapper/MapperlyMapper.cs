using System.Text.Json;
using MetroShip.Repository.Extensions;
using MetroShip.Repository.Models.Identity;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.User;
using Riok.Mapperly.Abstractions;

namespace MetroShip.Service.Mapper;

[Mapper]
public partial class MapperlyMapper : IMapperlyMapper
{
    // user
    public partial IList<RoleResponse> MapToRoleResponseList(IList<RoleEntity> entity);
    public partial UserEntity MapToUserEntity(RegisterRequest request);
    public partial LoginResponse MapToLoginResponse(UserEntity entity);

    public partial UserEntity MapToUserEntity(UserCreateRequest request);
    public partial void MapUserRequestToEntity(UserUpdateRequest request, UserEntity entity);

    public partial IList<UserResponse> MapToUserResponseList(IList<UserEntity> entity);
    public partial IQueryable<UserResponse> MapToUserResponseList(IQueryable<UserEntity> entity);
    public partial PaginatedListResponse<UserResponse> MapToUserResponsePaginatedList(PaginatedList<UserEntity> entity);
    public partial UserResponse MapToUserResponse(UserEntity entity);
    public partial void MapRegisterRequestToEntity(RegisterRequest request, UserEntity entity);

    public int? MapToVoucherId(int? voucherId)
    {
        // if voucherId is null, return null
        if (voucherId == null)
        {
            return null;
        }

        // if voucherId is 0, return null
        if (voucherId == 0)
        {
            return null;
        }

        return voucherId;
    }

    // datetimeoffset to dateonly
    public DateOnly MapDateTimeOffsetToDateOnly(DateTimeOffset dateTimeOffset)
    {
        return DateOnly.FromDateTime(dateTimeOffset.DateTime);
    }

    // datetime to dateonly
    public DateOnly MapDateTimeToDateOnly(DateTime dateTime)
    {
        return DateOnly.FromDateTime(dateTime);
    }

    // jsonstring to list double
    public IList<double> MapJsonStringToDoubleList(string jsonString)
    {
        return JsonSerializer.Deserialize<List<double>>(jsonString);
    }

    // role entity list to role name list<string>
    public IList<string?> MapRoleToRoleName(IEnumerable<UserRoleEntity> entity)
    {
        return entity.Select(x => x.Role.NormalizedName).ToList();
    }
}