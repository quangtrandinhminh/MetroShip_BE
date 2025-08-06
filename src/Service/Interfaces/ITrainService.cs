using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Train;
using MetroShip.Utility.Enums;

namespace MetroShip.Service.Interfaces;

public interface ITrainService
{
    Task<IList<TrainCurrentCapacityResponse>> GetAllTrainsByLineSlotDateAsync(LineSlotDateFilterRequest request);

    Task<PaginatedListResponse<TrainListResponse>> PaginatedListResponse(
        TrainListFilterRequest request);

    Task<IList<object>> GetTrainSystemConfigAsync();
    Task<bool> IsShipmentDeliveredAsync(string trackingCode);
    Task<IList<string>> GetTrackingCodesByTrainAsync(string trainId);
    Task UpdateTrainLocationAsync(string trainId, double lat, double lng, string stationId);

    Task<string> AddShipmentItinerariesForTrain(AddTrainToItinerariesRequest request);

    Task<TrainPositionResult> GetTrainPositionAsync(string trainId);
    Task<TrainPositionResult> GetTrainPositionByTrackingCodeAsync(string trackingCode);
    Task StartOrContinueSimulationAsync(string trainId, DirectionEnum direction);
    Task ConfirmTrainArrivedAsync(string trainId, string stationId);
}