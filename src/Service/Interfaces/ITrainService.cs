using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.PaginatedList;

namespace MetroShip.Service.Interfaces;

public interface ITrainService
{
    Task<PaginatedListResponse<TrainListResponse>> GetAllTrainsAsync(
        PaginatedListRequest request,
        string? lineId = null
    );

    Task<IList<object>> GetTrainSystemConfigAsync();
}