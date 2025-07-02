using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Train;

namespace MetroShip.Service.Interfaces;

public interface ITrainService
{
    Task<IList<TrainCurrentCapacityResponse>> GetAllTrainsByLineSlotDateAsync(LineSlotDateFilterRequest request);

    Task<PaginatedListResponse<TrainListResponse>> PaginatedListResponse(
        TrainListFilterRequest request);

    Task<IList<object>> GetTrainSystemConfigAsync();

    Task<string> AddShipmentItinerariesForTrain(AddTrainToItinerariesRequest request);
}