using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Region;

namespace MetroShip.Service.Interfaces;

public interface IRegionService
{
    Task<PaginatedListResponse<RegionResponse>> GetAllRegionsAsync(PaginatedListRequest request);

    Task<string> CreateRegionAsync(CreateRegionRequest request);

    Task<string> UpdateRegionAsync(UpdateRegionRequest request);
}