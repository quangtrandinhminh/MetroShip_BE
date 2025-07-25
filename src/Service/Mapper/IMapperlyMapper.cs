﻿using MetroShip.Repository.Extensions;
using MetroShip.Repository.Models;
using MetroShip.Repository.Models.Identity;
using MetroShip.Repository.Repositories;
using MetroShip.Service.ApiModels.MetroLine;
using MetroShip.Service.ApiModels.MetroTimeSlot;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Parcel;
using MetroShip.Service.ApiModels.ParcelCategory;
using MetroShip.Service.ApiModels.Pricing;
using MetroShip.Service.ApiModels.Region;
using MetroShip.Service.ApiModels.Route;
using MetroShip.Service.ApiModels.Shipment;
using MetroShip.Service.ApiModels.StaffAssignment;
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
    ShipmentDetailsResponse MapToShipmentDetailsResponse(Shipment entity);
    Shipment MapToShipmentEntity(ShipmentRequest request);
    ItineraryResponse MapToShipmentItineraryResponse(ShipmentItinerary entity);
    /*List<ShipmentAvailableTimeSlotResponse> MapToAvailableTimeSlotResponseList(
    List<(DateTimeOffset, DateTimeOffset, MetroTimeSlotResponse, decimal, decimal, ShipmentStatusEnum, List<string>)> slots);*/
    List<ItineraryResponse> MapToListShipmentItineraryResponse(List<ShipmentItinerary> entity);

    // station
    StationResponse MapToStationResponse(Station entity);
    PaginatedListResponse<StationListResponse> MapToStationListResponsePaginatedList(PaginatedList<Station> entity);
    StationListResponse MapToStationListResponse(Station entity);
    StationDetailResponse MapToStationDetailResponse(Station entity);
    Station MapToStationEntity(CreateStationRequest request);
    ICollection<Station> MapToStationEntityList(IList<CreateStationRequest> requestList);
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
    MetroLine MapToMetroLineEntity(MetroLineCreateRequest request);

    // transaction
    Transaction MapToTransactionEntity(TransactionRequest request);

    PaginatedListResponse<TransactionResponse> MapToTransactionPaginatedList(PaginatedList<Transaction> source);

    TransactionResponse MapToTransactionResponse(Transaction transaction);

    // train
    PaginatedListResponse<TrainListResponse> MapToTrainListResponsePaginatedList(PaginatedList<MetroTrain> entity);
    IList<TrainListResponse> MapToTrainListResponse(ICollection<MetroTrain> entity);
    IList<TrainCurrentCapacityResponse> MapToTrainCurrentCapacityResponse(ICollection<MetroTrain> entity);
    TrainResponse MapToTrainResponse(MetroTrain request);

    // time slot
    MetroTimeSlotResponse MapToMetroTimeSlotResponse(MetroTimeSlot entity);

    // media
    ShipmentMedia MapToShipmentMediaEntity(ShipmentMediaRequest request);

    // staff assignment
    StaffAssignmentResponse MapToStaffAssignmentResponse(StaffAssignment entity);
    List<StaffAssignmentResponse> MapToStaffAssignmentResponseList(ICollection<StaffAssignment> entity);

    // pricing config
    PricingTableResponse MapToPricingTableResponse(PricingConfig entity);

    // region
    PaginatedListResponse<RegionResponse> MapToRegionPaginatedList(PaginatedList<Region> entityList);
    RegionResponse MapToRegionResponse(Region entity);
}