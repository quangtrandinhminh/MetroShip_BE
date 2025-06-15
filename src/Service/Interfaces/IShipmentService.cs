using MetroShip.Service.ApiModels.Graph;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Shipment;
using MetroShip.Utility.Enums;

namespace MetroShip.Service.Interfaces;

public interface IShipmentService
{
    Task<PaginatedListResponse<ShipmentListResponse>> GetAllShipments(PaginatedListRequest request);
    Task<ShipmentDetailsResponse?> GetShipmentByTrackingCode(string trackingCode);
    Task<PaginatedListResponse<ShipmentListResponse>> GetShipmentsHistory(PaginatedListRequest request, ShipmentStatusEnum? status);
    Task<string> BookShipment(ShipmentRequest request, CancellationToken cancellationToken = default);
    Task<BestPathGraphResponse> FindPathAsync(BestPathRequest request);
    Task<TotalPriceResponse> GetItineraryAndTotalPrice(TotalPriceCalcRequest request);
    Task<PaginatedListResponse<ShipmentListResponse>> GetShipmentByLineAndDate(PaginatedListRequest request,
        string lineCode, DateTimeOffset date, string? regionCode, ShiftEnum? shift);
}