using MetroShip.Service.ApiModels.Graph;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Shipment;
using MetroShip.Utility.Enums;

namespace MetroShip.Service.Interfaces;

public interface IShipmentService
{
    Task<PaginatedListResponse<ShipmentListResponse>> GetAllShipments(PaginatedListRequest request,
        ShipmentFilterRequest? filterRequest = null, OrderByRequest? orderByRequest = null);
    Task<ShipmentDetailsResponse?> GetShipmentByTrackingCode(string trackingCode);
    Task<PaginatedListResponse<ShipmentListResponse>> GetShipmentsHistory(PaginatedListRequest request, ShipmentStatusEnum? status);
    Task<(string, string)> BookShipment(ShipmentRequest request, CancellationToken cancellationToken = default);
    //Task<BestPathGraphResponse> FindPathAsync(BestPathRequest request);
    Task<TotalPriceResponse> GetItineraryAndTotalPrice(TotalPriceCalcRequest request);
    /*Task<PaginatedListResponse<ShipmentListResponse>> GetShipmentByLineAndDate(PaginatedListRequest request,
        string lineCode, DateTimeOffset date, string? regionCode, ShiftEnum? shift);*/
    //Task<List<ItineraryResponse>> CheckAvailableTimeSlotsAsync(ShipmentAvailableTimeSlotsRequest request);

    //Task<List<ItineraryResponse>> CheckAvailableTimeSlotsAsync(string shipmentId, int maxAttempt);
    Task UpdateShipmentStatusNoDropOff(string shipmentId);
    Task ScheduleUpdateNoDropOffJob(string shipmentId, DateTimeOffset scheduledDateTime);
    Task CancelUpdateNoDropOffJob(string shipmentId);
    Task PickUpShipment(ShipmentPickUpRequest request);
    Task RejectShipment(ShipmentRejectRequest request);
    Task CancelShipment(ShipmentRejectRequest request);
    Task FeedbackShipment(ShipmentFeedbackRequest request);
    Task UpdateShipmentStatusUnpaid(string shipmentId);
    Task ApplySurchargeForShipment(string shipmentId);
    Task CompleteShipment(ShipmentPickUpRequest request);
    Task<ShipmentLocationResponse> GetShipmentLocationAsync(string trackingCode);
    Task<UpdateShipmentStatusResponse> UpdateShipmentStatusByStationAsync(UpdateShipmentStatusRequest request, string staffId);
    Task<List<ShipmentItineraryResponseDto>> AssignTrainToShipmentAsync(string trackingCode, string trainId);
}