using MetroShip.Repository.Extensions;
using MetroShip.Repository.Models;
using MetroShip.Repository.Models.Identity;
using MetroShip.Repository.Repositories;
using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.Graph;
using MetroShip.Service.ApiModels.Insurance;
using MetroShip.Service.ApiModels.InsurancePolicy;
using MetroShip.Service.ApiModels.MetroLine;
using MetroShip.Service.ApiModels.MetroTimeSlot;
using MetroShip.Service.ApiModels.Notification;
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
using MetroShip.Service.ApiModels.Transaction;
using MetroShip.Service.ApiModels.User;
using MetroShip.Service.BusinessModels;
using MetroShip.Utility.Enums;
using MetroShip.Service.ApiModels.MetroTimeSlot;
using MetroShip.Service.ApiModels.Pricing;
using MetroShip.Service.ApiModels.Region;
using MetroShip.Service.ApiModels.StaffAssignment;
using MetroShip.Service.ApiModels.SupportTicket;
using MetroShip.Utility.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Riok.Mapperly.Abstractions;
using System.Text.Json;
using MetroShip.Service.ApiModels.SystemConfig;
using static MetroShip.Repository.Repositories.ShipmentRepository;

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
    public partial void MapBankInfoRequestToEntity(BankInfoRequest request, UserEntity entity);

    // shipment
    public partial PaginatedListResponse<ShipmentListResponse> MapToShipmentListResponsePaginatedList(PaginatedList<Shipment> entity);
    public partial ShipmentListResponse MapToShipmentListResponse(Shipment entity);

    public partial ShipmentDetailsResponse MapToShipmentDetailsResponse(Shipment entity);
    public partial Shipment MapToShipmentEntity(ShipmentRequest request);
    protected partial ShipmentItinerary MapToShipmentItinerary(ShipmentItineraryRequest request);
    public partial ItineraryResponse MapToShipmentItineraryResponse(ShipmentItinerary entity);
    public partial List<ShipmentTrackingResponse> MapToParcelTrackingResponseList(ICollection<ShipmentTrackingResponse> entity);
    public partial ShipmentTrackingResponse MapToShipmentTrackingResponse(ShipmentTracking entity);

    public partial Shipment CloneToNewShipment(Shipment origin);

    public partial void MapToShipmentEntity(Shipment entity, Shipment request);

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

    public partial List<StationResponse> MapToStationResponseList(ICollection<Station> entity);

    public partial ICollection<Station> MapToStationEntityList(IList<CreateStationRequest> requestList);

    public partial PaginatedListResponse<StationListResponse> MapToStationListResponsePaginatedList(PaginatedList<Station> entity);

    [MapProperty(nameof(Station.Id), nameof(StationListResponse.StationId))]
    public partial StationListResponse MapToStationListResponse(Station entity);

    public partial StationDetailResponse MapToStationDetailResponse(Station entity);

    public partial Station MapToStationEntity(CreateStationRequest request);

    public partial void MapToExistingStation(UpdateStationRequest request, Station entity);

    public partial ICollection<Station> MapToStationEntityList(ICollection<CreateStationRequest> request);

    [MapProperty(nameof(StationListItem.StationId),nameof(Station.Id))]
    public partial void MapToStationEntityList(ICollection<StationListItem> requestList, ICollection<Station> entity);

    // route
    [MapProperty(nameof(Route.Id), nameof(RouteStationResponse.RouteId))]
    [MapProperty(nameof(Route.RouteNameVi), nameof(RouteStationResponse.RouteName))]
    public partial RouteStationResponse MapToRouteStationResponse(Route entity);

    // metroline
    public partial MetroLineItineraryResponse MapToMetroLineItineraryResponse(MetroLine entity);
    public partial MetroLine MapToMetroLineEntity(MetroRouteRequest request);
    public partial MetroRouteResponse MapToMetroLineResponse(MetroLine entity);
    public partial PaginatedListResponse<MetroRouteResponse> MapToMetroLinePaginatedList(PaginatedList<MetroLine> entityList);
    public partial MetroRouteResponseDetails MapToMetroLineResponseDetails(MetroLine entity);
    public partial void MapToMetroLineEntity(MetroRouteUpdateRequest request, MetroLine entity);

    // parcel
    public partial Parcel MapToParcelEntity(ParcelRequest request);
    public partial PaginatedListResponse<ParcelResponse> MapToParcelPaginatedList(PaginatedList<Parcel> entityList);
    public partial ParcelResponse MapToParcelResponse(Parcel entity);
    public partial void CloneToParcelRequestList(List<ParcelRequest> origin, List<ParcelRequest> clone);
    public partial List<ParcelTrackingResponse> MapToParcelTrackingResponseList(ICollection<ParcelTracking> entity);
    public partial ParcelTrackingResponse MapToParcelTrackingResponse(ParcelTracking entity);

    // parcel category
    public partial ParcelCategory MapToParcelCategoryEntity(ParcelCategoryCreateRequest request);
    public partial void MapParcelCategoryUpdateRequestToEntity(ParcelCategoryUpdateRequest request, ParcelCategory entity);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public partial ParcelCategoryResponse MapToParcelCategoryResponse(ParcelCategory entity);

    // category insurance
    public partial CategoryInsuranceResponse MapToCategoryInsuranceResponse(CategoryInsurance entity);

    public partial List<CategoryInsuranceResponse> MapToCategoryInsuranceResponseList(ICollection<CategoryInsurance> entity);

    // insurance policy
    public partial InsuranceResponse MapToInsuranceResponse(InsurancePolicy entity);

    public partial PaginatedListResponse<ParcelCategoryResponse> MapToParcelCategoryPaginatedList(PaginatedList<ParcelCategory> entityList);

    public partial InsurancePolicy MapToInsurancePolicy(InsurancePolicyRequest request);
    public partial IList<InsurancePolicyResponse> MapToInsurancePolicyResponseList(ICollection<InsurancePolicy> entity);

    // transaction
    public partial Transaction MapToTransactionEntity(TransactionRequest request);
    public partial PaginatedListResponse<TransactionResponse> MapToTransactionPaginatedList(PaginatedList<Transaction> source);
    public partial TransactionResponse MapToTransactionResponse(Transaction transaction);

    // metro train
    public partial PaginatedListResponse<TrainListResponse> MapToTrainListResponsePaginatedList(PaginatedList<MetroTrain> entity);
    public partial IList<TrainListResponse> MapToTrainListResponse(ICollection<MetroTrain> entity);
    public partial IList<TrainCurrentCapacityResponse> MapToTrainCurrentCapacityResponse(ICollection<MetroTrain> entity);
    public partial TrainResponse MapToTrainResponse(MetroTrain request);
    public partial MetroTrain MapToMetroTrainEntity(CreateTrainRequest request);

    // time slot
    public partial MetroTimeSlotResponse MapToMetroTimeSlotResponse(MetroTimeSlot entity);

    // media
    public partial ShipmentMedia MapToShipmentMediaEntity(ShipmentMediaRequest request);
    public partial ShipmentMediaResponse MapToShipmentMediaResponse(ShipmentMedia entity);
    public partial List<ShipmentMediaResponse> MapToShipmentMediaResponseList(ICollection<ShipmentMedia> entity);
    public partial ParcelMedia MapToParcelMediaEntity(ParcelMediaRequest request);
    public partial ParcelMediaResponse MapToParcelMediaResponse(ParcelMedia entity);
    public partial List<ParcelMediaResponse> MapToParcelMediaResponseList(ICollection<ParcelMedia> entity);

    // staff assignment
    public partial StaffAssignmentResponse MapToStaffAssignmentResponse(StaffAssignment entity);
    public partial List<StaffAssignmentResponse> MapToStaffAssignmentResponseList(ICollection<StaffAssignment> entity);

    // pricing config
    public partial PricingTableResponse MapToPricingTableResponse(PricingConfig entity);
    public partial PaginatedListResponse<PricingTableResponse> MapToPricingTablePaginatedList(PaginatedList<PricingConfig> entityList);
    public partial PricingConfig MapToPricingConfigEntity(PricingConfigRequest request);

    // region
    public partial PaginatedListResponse<RegionResponse> MapToRegionPaginatedList(PaginatedList<Region> entityList);
    public partial RegionResponse MapToRegionResponse(Region entity);

    public partial Region MapToRegionEntity(CreateRegionRequest request);

    // support ticket
    public partial SupportTicketResponse MapToSupportTicketResponse(SupportTicket entity);
    public partial PaginatedListResponse<SupportTicketResponse> MapToSupportTicketPaginatedList(PaginatedList<SupportTicket> entityList);
    public partial SupportTicket MapToSupportTicketEntity(SupportTicketRequest request);

    // insurance policy
    public partial InsurancePolicyResponse MapToInsurancePolicyResponse(InsurancePolicy entity);
    public partial PaginatedListResponse<InsurancePolicyResponse> MapToInsurancePolicyPaginatedList(PaginatedList<InsurancePolicy> entityList);
    // notification
    public partial NotificationDto MapNotification(Notification notification);
    public partial Notification MapNotificationRequest(NotificationCreateRequest request);
    public partial void MapNotificationUpdate(NotificationUpdateRequest request, Notification notification);
    public partial PaginatedListResponse<NotificationDto> MapNotificationList(PaginatedList<Notification> paginatedList);

    // config
    public partial SystemConfig MapToSystemConfigEntity(SystemConfigRequest request);
    public partial SystemConfigResponse MapToSystemConfigResponse(SystemConfig entity);
    public partial PaginatedListResponse<SystemConfigResponse> MapToSystemConfigPaginatedList(PaginatedList<SystemConfig> entityList);
    public partial void MapToSystemConfigEntity(SystemConfigRequest request, SystemConfig entity);

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
