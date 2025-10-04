using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Train;
using MetroShip.Utility.Enums;

namespace MetroShip.Service.Interfaces;

public interface ITrainService
{
    Task<List<TrainDropdownResponse>> GetTrainsDropdownAsync(string? lineId = null, string? regionId = null);
    Task<IList<TrainCurrentCapacityResponse>> GetAllTrainsByLineSlotDateAsync(LineSlotDateFilterRequest request);

    Task<PaginatedListResponse<TrainListResponse>> PaginatedListResponse(
        TrainListFilterRequest request);

    Task<IList<object>> GetTrainSystemConfigAsync();
    Task<string> CreateTrainAsync(CreateTrainRequest request);
    Task<bool> IsShipmentDeliveredAsync(string trackingCode);
    Task<IList<string>> GetTrackingCodesByTrainAsync(string trainId);
    Task UpdateTrainLocationAsync(string trainId, double lat, double lng, string stationId);

    Task<string> AddShipmentItinerariesForTrain(AddTrainToItinerariesRequest request);

    Task<TrainPositionResult> GetTrainPositionAsync(string trainId);
    Task<TrainPositionResult> GetTrainPositionByTrackingCodeAsync(string trackingCode);
    Task StartOrContinueSimulationAsync(string trainId);
    Task ConfirmTrainArrivedAsync(string trainId, string stationId);
    Task ConfirmTrainArrivedAsync(string nextStationId);
    Task<TrainDto> ScheduleTrainAsync(string trainIdOrCode, bool startFromEnd = false);
    Task<TrainPositionResult> GetTrainPositionAsync1(string trainId);
    Task<object> GetTrainAdditionalDataAsync(string trainId);
}