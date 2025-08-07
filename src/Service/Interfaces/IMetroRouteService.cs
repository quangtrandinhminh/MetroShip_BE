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
        Task<List<MetroRouteResponse>> GetAllMetroRoutes(PaginatedListRequest request);
        Task<List<MetroLineItineraryResponse>> GetAllMetroRouteDropdown(string? stationId);
        Task<MetroRouteResponseDetails> GetMetroRouteById(string metroRouteId);
        Task<List<MetrolineGetByRegionResponse>> GetAllMetroLineByRegion(string? regionId);
        Task<int> CreateMetroRoute(MetroRouteRequest request);
    }
}
