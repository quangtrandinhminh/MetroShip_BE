using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Station;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.Interfaces
{
    public interface IStationService
    {
        Task<IEnumerable<StationListResponse>> GetAllStationsAsync(string? regionId);
        Task<StationDetailResponse> GetStationByIdAsync(Guid id);
        Task<StationDetailResponse> CreateStationAsync(CreateStationRequest request);
        Task<StationDetailResponse> UpdateStationAsync(Guid id, UpdateStationRequest request);
        Task DeleteStationAsync(Guid id);
    }
}
