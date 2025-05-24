using MetroShip.Repository.Extensions;
using MetroShip.Repository.Models;
using MetroShip.Repository.Models.Identity;
using MetroShip.Service.ApiModels.MetroLine;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Parcel;
using MetroShip.Service.ApiModels.ParcelCategory;
using MetroShip.Service.ApiModels.Route;
using MetroShip.Service.ApiModels.Shipment;
using MetroShip.Service.ApiModels.Station;
using MetroShip.Service.ApiModels.Transaction;
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

    // shipment
    PaginatedListResponse<ShipmentListResponse> MapToShipmentListResponsePaginatedList(PaginatedList<Shipment> entity);
    ShipmentListResponse MapToShipmentListResponse(Shipment entity);
    ShipmentDetailsResponse MapToShipmentDetailsResponse(Shipment entity);
    Shipment MapToShipmentEntity(ShipmentRequest request);
    ItineraryResponse MapToShipmentItineraryRequest(ShipmentItinerary entity);

    // station
    StationResponse MapToStationResponse(Station entity);

    // route
    RouteResponse MapToRouteResponse(Route entity);

    IList<string?> MapRoleToRoleName(IEnumerable<UserRoleEntity> entity);

    // parcel category
    ParcelCategory MapToParcelCategoryEntity(ParcelCategoryCreateRequest request);

    void MapParcelCategoryUpdateRequestToEntity(ParcelCategoryUpdateRequest request, ParcelCategory entity);
    
    ParcelCategoryResponse MapToParcelCategoryResponse(ParcelCategory entity);
    
    PaginatedListResponse<ParcelCategoryResponse> MapToParcelCategoryPaginatedList(PaginatedList<ParcelCategory> entityList);

    // parcel
    CreateParcelResponse MapToParcelResponse(Parcel entity);
    PaginatedListResponse<CreateParcelResponse> MapToParcelPaginatedList(PaginatedList<Parcel> entityList);
    // metroline
    MetroLineItineraryResponse MapToMetroLineResponse(MetroLine entity);

    // transaction
    Transaction MapToTransactionEntity(TransactionRequest request);

    PaginatedListResponse<TransactionResponse> MapToTransactionPaginatedList(PaginatedList<Transaction> source);

    TransactionResponse MapToTransactionResponse(Transaction transaction);
}