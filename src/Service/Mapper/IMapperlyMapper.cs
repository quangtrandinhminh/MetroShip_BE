using MetroShip.Repository.Extensions;
using MetroShip.Repository.Models;
using MetroShip.Repository.Models.Identity;
using MetroShip.Repository.Repositories;
using MetroShip.Service.ApiModels.MetroLine;
using MetroShip.Service.ApiModels.MetroTimeSlot;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Parcel;
using MetroShip.Service.ApiModels.ParcelCategory;
using MetroShip.Service.ApiModels.Route;
using MetroShip.Service.ApiModels.Shipment;
using MetroShip.Service.ApiModels.Station;
using MetroShip.Service.ApiModels.Train;
using MetroShip.Service.ApiModels.Transaction;
using MetroShip.Service.ApiModels.User;
using MetroShip.Utility.Enums;

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
    ShipmentDetailsResponse MapToShipmentDetailsResponse(ShipmentRepository.ShipmentDto entity);
    Shipment MapToShipmentEntity(ShipmentRequest request);
    ItineraryResponse MapToShipmentItineraryResponse(ShipmentItinerary entity);
    PaginatedListResponse<ShipmentListResponse> MapToShipmentListResponsePaginatedList(PaginatedList<ShipmentRepository.ShipmentDto> entity);
    List<ShipmentAvailableTimeSlotResponse> MapToAvailableTimeSlotResponseList(
    List<(DateTimeOffset, DateTimeOffset, MetroTimeSlotResponse, decimal, decimal, ShipmentStatusEnum, List<string>)> slots);
    List<ItineraryResponse> MapToListShipmentItineraryResponse(List<ShipmentItinerary> entity);

    // station
    StationResponse MapToStationResponse(Station entity);
    PaginatedListResponse<StationListResponse> MapToStationListResponsePaginatedList(PaginatedList<Station> entity);
    StationListResponse MapToStationListResponse(Station entity);
    StationDetailResponse MapToStationDetailResponse(Station entity);
    Station MapToStationEntity(CreateStationRequest request);
    void MapToExistingStation(UpdateStationRequest request, Station entity);

    // route
    RouteResponse MapToRouteResponse(Route entity);

    IList<string?> MapRoleToRoleName(IList<RoleEntity> entity);

    // parcel category
    ParcelCategory MapToParcelCategoryEntity(ParcelCategoryCreateRequest request);

    void MapParcelCategoryUpdateRequestToEntity(ParcelCategoryUpdateRequest request, ParcelCategory entity);
    
    ParcelCategoryResponse MapToParcelCategoryResponse(ParcelCategory entity);
    
    PaginatedListResponse<ParcelCategoryResponse> MapToParcelCategoryPaginatedList(PaginatedList<ParcelCategory> entityList);

    // parcel
    PaginatedListResponse<ParcelResponse> MapToParcelPaginatedList(PaginatedList<Parcel> entityList);
    ParcelResponse MapToParcelResponse(Parcel entity);
    void CloneToParcelRequestList(List<ParcelRequest> origin, List<ParcelRequest> clone);

    // metroline
    MetroLineItineraryResponse MapToMetroLineResponse(MetroLine entity);

    // transaction
    Transaction MapToTransactionEntity(TransactionRequest request);

    PaginatedListResponse<TransactionResponse> MapToTransactionPaginatedList(PaginatedList<Transaction> source);

    TransactionResponse MapToTransactionResponse(Transaction transaction);

    // train
    PaginatedListResponse<TrainListResponse> MapToTrainListResponsePaginatedList(PaginatedList<MetroTrain> entity);
    TrainListResponse MapToTrainListResponse (MetroTrain entity);
    TrainResponse MapToTrainResponse(MetroTrain request);

}