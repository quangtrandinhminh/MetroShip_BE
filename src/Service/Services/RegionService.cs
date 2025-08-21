using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Region;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Mapper;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Exceptions;
using Microsoft.AspNetCore.Http;
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

    // create region
    public async Task<string> CreateRegionAsync(CreateRegionRequest request)
    {
        _logger.Information("Creating a new region with name: {RegionName}", request.RegionName);

        // Check if region with the same name or code already exists
        var existingRegion = await _regionRepository.IsExistAsync(
                       r => r.RegionName == request.RegionName || r.RegionCode == request.RegionCode
                              );
        if (existingRegion)
        {
            throw new AppException(
                ErrorCode.BadRequest,
                RegionMessageConstants.REGION_EXISTED,
                StatusCodes.Status400BadRequest);
        }

        var region = _mapper.MapToRegionEntity(request);
        await _regionRepository.AddAsync(region);
        await _unitOfWork.SaveChangeAsync();
        return RegionMessageConstants.REGION_CREATE_SUCCESS;
    }
}