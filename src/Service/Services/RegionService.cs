using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Region;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Mapper;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace MetroShip.Service.Services;

public class RegionService(IServiceProvider serviceProvider) : IRegionService
{
    private readonly IRegionRepository _regionRepository = serviceProvider.GetRequiredService<IRegionRepository>();
    private readonly IUnitOfWork _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
    private readonly IMapperlyMapper _mapper = serviceProvider.GetRequiredService<IMapperlyMapper>();

    public async Task<PaginatedListResponse<RegionResponse>> GetAllRegionsAsync(PaginatedListRequest request)
    {
        _logger.Information("Fetching all regions from the repository.");
        var regions = await _regionRepository.GetAllPaginatedQueryable(
                request.PageNumber,
                request.PageSize,
                null,
                r => r.RegionName,
                true
        );

        var regionResponses = _mapper.MapToRegionPaginatedList(regions);
        return regionResponses;
    }
}