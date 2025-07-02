using System.Text.Json;
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

namespace MetroShip.Service.Mapper;

[Mapper]
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
    public partial ShipmentDetailsResponse MapToShipmentDetailsResponse(ShipmentDto entity);
    public partial Shipment MapToShipmentEntity(ShipmentRequest request);
    // protected partial ShippingInformation MapToShippingInformation(Shipment entity);
    // protected partial ShipmentTrackingResponse MapToShipmentItinerary(Shipment entity);
    protected partial ShipmentItinerary MapToShipmentItinerary(ShipmentItineraryRequest request);
    public partial ItineraryResponse MapToShipmentItineraryRequest(ShipmentItinerary entity);
    public partial ShipmentDetailsResponse MapToShipmentListResponse(ShipmentDto entity);
    public partial PaginatedListResponse<ShipmentListResponse> MapToShipmentListResponsePaginatedList(
        PaginatedList<ShipmentDto> entity);
    // Explicitly specify the namespace for 'AvailableTimeSlotDto' in the method signature
    [MapProperty("Item1", "StartDate")]
    [MapProperty("Item2", "Date")]
    [MapProperty("Item3.Id", "TimeSlotId")]
    [MapProperty("Item3.Shift", "TimeSlotName")]
    [MapProperty("Item4", "RemainingWeightKg")]
    [MapProperty("Item5", "RemainingVolumeM3")]
    public partial List<ShipmentAvailableTimeSlotResponse> MapToAvailableTimeSlotResponseList(
        List<(DateTimeOffset, DateTimeOffset, MetroTimeSlot, decimal, decimal)> slots);

    // station
    [MapProperty(nameof(Station.Id), nameof(StationResponse.StationId))]
    public partial StationResponse MapToStationResponse(Station entity);

    public partial PaginatedListResponse<StationListResponse> MapToStationListResponsePaginatedList(PaginatedList<Station> entity);

    public partial StationListResponse MapToStationListResponse(Station entity);

    public partial StationDetailResponse MapToStationDetailResponse(Station entity);

    public partial Station MapToStationEntity(CreateStationRequest request);

    public partial void MapToExistingStation(UpdateStationRequest request, Station entity);

    // route
    [MapProperty(nameof(Route.Id), nameof(RouteResponse.RouteId))]
    [MapProperty(nameof(Route.RouteNameVi), nameof(RouteResponse.RouteName))]
    public partial RouteResponse MapToRouteResponse(Route entity);

    [MapProperty(nameof(Route.Id), nameof(RouteResponse.RouteId))]
    [MapProperty(nameof(Route.RouteNameVi), nameof(RouteResponse.RouteName))]
    public partial RouteResponse MapToRouteResponse(RouteDto entity);

    // metroline
    [MapProperty(nameof(MetroLine.Id), nameof(MetroLineItineraryResponse.LineId))]
    public partial MetroLineItineraryResponse MapToMetroLineResponse(MetroLine entity);

    // parcel
    public partial Parcel MapToParcelEntity(ParcelRequest request);
    public partial PaginatedListResponse<ParcelResponse> MapToParcelPaginatedList(PaginatedList<Parcel> entityList);

    public partial ParcelResponse MapToParcelResponse(Parcel entity);

    // parcel category
    public partial ParcelCategory MapToParcelCategoryEntity(ParcelCategoryCreateRequest request);
    public partial void MapParcelCategoryUpdateRequestToEntity(ParcelCategoryUpdateRequest request, ParcelCategory entity);
    public partial ParcelCategoryResponse MapToParcelCategoryResponse(ParcelCategory entity);
    public partial PaginatedListResponse<ParcelCategoryResponse> MapToParcelCategoryPaginatedList(PaginatedList<ParcelCategory> entityList);

    // transaction
    public partial Transaction MapToTransactionEntity(TransactionRequest request);
    public partial PaginatedListResponse<TransactionResponse> MapToTransactionPaginatedList(PaginatedList<Transaction> source);
    public partial TransactionResponse MapToTransactionResponse(Transaction transaction);

    // metro train
    public partial PaginatedListResponse<TrainListResponse> MapToTrainListResponsePaginatedList(PaginatedList<MetroTrain> entity);

    public partial TrainListResponse MapToTrainListResponse(MetroTrain entity);
    public partial TrainResponse MapToTrainResponse(MetroTrain request);

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