using MetroShip.Repository.Extensions;
using MetroShip.Service.ApiModels.MetroLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetroShip.Service.ApiModels.PaginatedList;

namespace MetroShip.Service.Interfaces
{
    public interface IMetroRouteService
    {
        Task<PaginatedListResponse<MetroRouteResponse>> GetAllMetroRoutes(PaginatedListRequest request,
            MetroRouteFilterRequest filter);
        Task<List<MetroLineItineraryResponse>> GetAllMetroRouteDropdown(string? stationId);
        Task<MetroRouteResponseDetails> GetMetroRouteById(string metroRouteId);
        Task<List<MetrolineGetByRegionResponse>> GetAllMetroLineByRegion(string? regionId);
        Task<int> CreateMetroRoute(MetroRouteRequest request);
        Task<string> ActivateMetroLine(string metroRouteId);
        Task<string> UpdateMetroLine(MetroRouteUpdateRequest request);
        Task<List<MetroLineDto>> GetAllMetroLinesWithStationsAsync();
        Task<MetroLineDto> GetMetroLineByIdAsync(string lineId);
    }
}
