using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Train;

namespace MetroShip.Service.Interfaces;

public interface ITrainService
{
    Task<PaginatedListResponse<TrainListResponse>> GetAllTrainsAsync(PaginatedListRequest request,
        string? lineId = null, string? timeSlotId = null, DateTimeOffset? date = null);

    Task<IList<object>> GetTrainSystemConfigAsync();
    Task<bool> IsShipmentDeliveredAsync(string trackingCode);
    Task<IList<string>> GetTrackingCodesByTrainAsync(string trainId);
    Task UpdateTrainLocationAsync(string trainId, double lat, double lng, string stationId);
}