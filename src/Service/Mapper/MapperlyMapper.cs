using System.Text.Json;
using MetroShip.Repository.Extensions;
using MetroShip.Repository.Models;
using MetroShip.Repository.Models.Identity;
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

    // transaction
    public partial Transaction MapToTransactionEntity(TransactionRequest request);

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

    /// <summary>
    /// mapper for parcel category
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public ParcelCategory MapToParcelCategoryEntity(ParcelCategoryCreateRequest request)
    {
        return new ParcelCategory
        {
            Id = Guid.NewGuid().ToString(),
            CategoryName = request.CategoryName,
            Description = request.Description,
            IsBulk = request.IsBulk,
            WeightLimitKg = request.WeightLimitKg,
            VolumeLimitCm3 = request.VolumeLimitCm3,
            LengthLimitCm = request.LengthLimitCm,
            WidthLimitCm = request.WidthLimitCm,
            HeightLimitCm = request.HeightLimitCm,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MapParcelCategoryUpdateRequestToEntity(ParcelCategoryUpdateRequest request, ParcelCategory entity)
    {
        entity.CategoryName = request.CategoryName;
        entity.Description = request.Description;
        entity.IsBulk = request.IsBulk;
        entity.WeightLimitKg = request.WeightLimitKg;
        entity.VolumeLimitCm3 = request.VolumeLimitCm3;
        entity.LengthLimitCm = request.LengthLimitCm;
        entity.WidthLimitCm = request.WidthLimitCm;
        entity.HeightLimitCm = request.HeightLimitCm;
        entity.IsActive = request.IsActive;
    }

    public ParcelCategoryResponse MapToParcelCategoryResponse(ParcelCategory entity)
    {
        return new ParcelCategoryResponse
        {
            Id = Guid.Parse(entity.Id),
            CategoryName = entity.CategoryName,
            Description = entity.Description,
            IsBulk = entity.IsBulk,
            WeightLimitKg = entity.WeightLimitKg,
            VolumeLimitCm3 = entity.VolumeLimitCm3,
            LengthLimitCm = entity.LengthLimitCm,
            WidthLimitCm = entity.WidthLimitCm,
            HeightLimitCm = entity.HeightLimitCm,
            IsActive = entity.IsActive
        };
    }

    public PaginatedListResponse<ParcelCategoryResponse> MapToParcelCategoryPaginatedList(PaginatedList<ParcelCategory> entityList)
    {
        return new PaginatedListResponse<ParcelCategoryResponse>
        {
            Items = entityList.Items.Select(MapToParcelCategoryResponse).ToList(),
            PageNumber = entityList.PageNumber,
            TotalCount = entityList.TotalCount,
            TotalPages = entityList.TotalPages
        };
    }
    public CreateParcelResponse MapToParcelResponse(Parcel entity)
    {
        return new CreateParcelResponse
        {
            Id = Guid.Parse(entity.Id),
            VolumeCm3 = entity.VolumeCm3,
            ChargeableWeightKg = entity.ChargeableWeightKg
        };
    }
    public PaginatedListResponse<CreateParcelResponse> MapToParcelPaginatedList(PaginatedList<Parcel> entityList)
    {
        return new PaginatedListResponse<CreateParcelResponse>
        {
            Items = entityList.Items.Select(MapToParcelResponse).ToList(),
            PageNumber = entityList.PageNumber,
            TotalCount = entityList.TotalCount,
            TotalPages = entityList.TotalPages
        };
    }
    public EnumResponse MapToEnumResponse(Enum enumValue)
    {
        return new EnumResponse
        {
            Id = (int)(object)enumValue,
            Value = enumValue.ToString(),
        };
    }

    public partial PaginatedListResponse<TransactionResponse> MapToTransactionPaginatedList(PaginatedList<Transaction> source);

    public partial TransactionResponse MapToTransactionResponse(Transaction transaction);
}