using System.Text.Json;
using MetroShip.Repository.Extensions;
using MetroShip.Repository.Models;
using MetroShip.Repository.Models.Identity;
using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.Graph;
using MetroShip.Service.ApiModels.MetroLine;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Parcel;
using MetroShip.Service.ApiModels.Route;
using MetroShip.Service.ApiModels.Shipment;
using MetroShip.Service.ApiModels.Station;
using MetroShip.Service.ApiModels.User;
using MetroShip.Service.BusinessModels;
using Microsoft.CodeAnalysis;
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

    // shipment
    public partial PaginatedListResponse<ShipmentListResponse> MapToShipmentListResponsePaginatedList(PaginatedList<Shipment> entity);
    public partial ShipmentListResponse MapToShipmentListResponse(Shipment entity);
    public partial ShipmentDetailsResponse MapToShipmentDetailsResponse(Shipment entity);
    public partial Shipment MapToShipmentEntity(ShipmentRequest request);
    protected partial ShippingInformation MapToShippingInformation(Shipment entity);
    protected partial ShipmentTrackingResponse MapToShipmentItinerary(Shipment entity);
    protected partial ShipmentItinerary MapToShipmentItinerary(ShipmentItineraryRequest request);
    public partial ItineraryResponse MapToShipmentItineraryRequest(ShipmentItinerary entity);

    // graph


    // station
    [MapProperty(nameof(Station.Id), nameof(StationResponse.StationId))]
    public partial StationResponse MapToStationResponse(Station entity);

    // route
    [MapProperty(nameof(Route.Id), nameof(RouteResponse.RouteId))]
    public partial RouteResponse MapToRouteResponse(Route entity);

    // metroline
    [MapProperty(nameof(MetroLine.Id), nameof(MetroLineItineraryResponse.LineId))]
    public partial MetroLineItineraryResponse MapToMetroLineResponse(MetroLine entity);

    // parcel
    public partial Parcel MapToParcelEntity(ParcelRequest request);


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

    public EnumResponse MapToEnumResponse(Enum enumValue)
    {
        return new EnumResponse
        {
            Id = (int)(object)enumValue,
            Value = enumValue.ToString(),
        };
    }
}