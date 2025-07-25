﻿using System.Text.Json;
using MetroShip.Repository.Extensions;
using MetroShip.Repository.Models;
using MetroShip.Repository.Models.Identity;
using MetroShip.Repository.Repositories;
using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.Graph;
using MetroShip.Service.ApiModels.MetroLine;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.ParcelCategory;
using MetroShip.Service.ApiModels.Parcel;
using MetroShip.Service.ApiModels.Route;
using MetroShip.Service.ApiModels.Shipment;
using MetroShip.Service.ApiModels.Station;
using MetroShip.Service.ApiModels.Transaction;
using MetroShip.Service.ApiModels.User;
using MetroShip.Service.BusinessModels;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using MetroShip.Service.ApiModels.Transaction;
using static MetroShip.Repository.Repositories.ShipmentRepository;
using MetroShip.Service.ApiModels.Train;
using MetroShip.Utility.Helpers;
using MetroShip.Utility.Enums;
using MetroShip.Service.ApiModels.MetroTimeSlot;
using MetroShip.Service.ApiModels.Pricing;
using MetroShip.Service.ApiModels.Region;
using MetroShip.Service.ApiModels.StaffAssignment;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace MetroShip.Service.Mapper;

[Mapper(UseDeepCloning = true)]
public partial class MapperlyMapper : IMapperlyMapper
{
    /// <summary>
    /// mapper for user
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
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
    // protected partial ShippingInformation MapToShippingInformation(Shipment entity);
    // protected partial ShipmentTrackingResponse MapToShipmentItinerary(Shipment entity);
    protected partial ShipmentItinerary MapToShipmentItinerary(ShipmentItineraryRequest request);
    public partial ItineraryResponse MapToShipmentItineraryResponse(ShipmentItinerary entity);
    // Explicitly specify the namespace for 'AvailableTimeSlotDto' in the method signature
    /*[MapProperty("Item1", "StartDate")]
    [MapProperty("Item2", "Date")]
    [MapProperty("Item3", "SlotDetail")]
    [MapProperty("Item4", "RemainingVolumeM3")]
    [MapProperty("Item5", "RemainingWeightKg")]
    [MapProperty("Item6", "ShipmentStatus")]
    [MapProperty("Item7", "ParcelIds")]
    public partial List<ShipmentAvailableTimeSlotResponse> MapToAvailableTimeSlotResponseList(
    List<(DateTimeOffset, DateTimeOffset, MetroTimeSlotResponse, decimal, decimal, ShipmentStatusEnum, List<string>)> slots);*/

    public partial List<ItineraryResponse> MapToListShipmentItineraryResponse(List<ShipmentItinerary> entity);
    // station
    [MapProperty(nameof(Station.Id), nameof(StationResponse.StationId))]
    public partial StationResponse MapToStationResponse(Station entity);

    public partial ICollection<Station> MapToStationEntityList(IList<CreateStationRequest> requestList);

    public partial PaginatedListResponse<StationListResponse> MapToStationListResponsePaginatedList(PaginatedList<Station> entity);

    public partial StationListResponse MapToStationListResponse(Station entity);

    public partial StationDetailResponse MapToStationDetailResponse(Station entity);

    public partial Station MapToStationEntity(CreateStationRequest request);

    public partial void MapToExistingStation(UpdateStationRequest request, Station entity);

    public partial ICollection<Station> MapToStationEntityList(ICollection<CreateStationRequest> request);

    // route
    [MapProperty(nameof(Route.Id), nameof(RouteResponse.RouteId))]
    [MapProperty(nameof(Route.RouteNameVi), nameof(RouteResponse.RouteName))]
    public partial RouteResponse MapToRouteResponse(Route entity);

    // metroline
    [MapProperty(nameof(MetroLine.Id), nameof(MetroLineItineraryResponse.LineId))]
    public partial MetroLineItineraryResponse MapToMetroLineResponse(MetroLine entity);
    public partial MetroLine MapToMetroLineEntity(MetroLineCreateRequest request);

    // parcel
    public partial Parcel MapToParcelEntity(ParcelRequest request);
    public partial PaginatedListResponse<ParcelResponse> MapToParcelPaginatedList(PaginatedList<Parcel> entityList);

    public partial ParcelResponse MapToParcelResponse(Parcel entity);

    public partial void CloneToParcelRequestList(List<ParcelRequest> origin, List<ParcelRequest> clone);

    // parcel category
    public partial ParcelCategory MapToParcelCategoryEntity(ParcelCategoryCreateRequest request);
    public partial void MapParcelCategoryUpdateRequestToEntity(ParcelCategoryUpdateRequest request, ParcelCategory entity);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public partial ParcelCategoryResponse MapToParcelCategoryResponse(ParcelCategory entity);

    public partial PaginatedListResponse<ParcelCategoryResponse> MapToParcelCategoryPaginatedList(PaginatedList<ParcelCategory> entityList);

    // transaction
    public partial Transaction MapToTransactionEntity(TransactionRequest request);
    public partial PaginatedListResponse<TransactionResponse> MapToTransactionPaginatedList(PaginatedList<Transaction> source);
    public partial TransactionResponse MapToTransactionResponse(Transaction transaction);

    // metro train
    public partial PaginatedListResponse<TrainListResponse> MapToTrainListResponsePaginatedList(PaginatedList<MetroTrain> entity);
    public partial IList<TrainListResponse> MapToTrainListResponse(ICollection<MetroTrain> entity);
    public partial IList<TrainCurrentCapacityResponse> MapToTrainCurrentCapacityResponse(ICollection<MetroTrain> entity);
    public partial TrainResponse MapToTrainResponse(MetroTrain request);

    // time slot
    public partial MetroTimeSlotResponse MapToMetroTimeSlotResponse(MetroTimeSlot entity);

    // media
    public partial ShipmentMedia MapToShipmentMediaEntity(ShipmentMediaRequest request);

    // staff assignment
    public partial StaffAssignmentResponse MapToStaffAssignmentResponse(StaffAssignment entity);
    public partial List<StaffAssignmentResponse> MapToStaffAssignmentResponseList(ICollection<StaffAssignment> entity);

    // pricing config
    public partial PricingTableResponse MapToPricingTableResponse(PricingConfig entity);

    // region
    public partial PaginatedListResponse<RegionResponse> MapToRegionPaginatedList(PaginatedList<Region> entityList);
    public partial RegionResponse MapToRegionResponse(Region entity);

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
    public IList<string?> MapRoleToRoleName(IList<RoleEntity> entity)
    {
        return entity.Select(x => x.Name).ToList();
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
