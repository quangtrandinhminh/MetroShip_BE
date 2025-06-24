using System.Linq.Expressions;
using MetroShip.Repository.Base;
using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Mapper;
using MetroShip.Utility.Config;
using MetroShip.Utility.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace MetroShip.Service.Services;

public class TrainService(IServiceProvider serviceProvider) : ITrainService
{
    private readonly IBaseRepository<MetroTrain> _trainRepository = 
        serviceProvider.GetRequiredService<IBaseRepository<MetroTrain>>();
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
    private readonly IMapperlyMapper _mapper = serviceProvider.GetRequiredService<IMapperlyMapper>();
    private readonly IUnitOfWork _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
    private readonly ISystemConfigRepository _systemConfigRepository = 
        serviceProvider.GetRequiredService<ISystemConfigRepository>();

    public async Task<PaginatedListResponse<TrainListResponse>> GetAllTrainsAsync(
        PaginatedListRequest request,
        string? lineId = null
        )
    {
        _logger.Information("Get all trains with request: {@request}", request);

        Expression<Func<MetroTrain, bool>> predicate = t => t.IsActive && t.DeletedAt == null;
        if (!string.IsNullOrEmpty(lineId) && Guid.TryParse(lineId, out _))
        {
            predicate.And(t => t.LineId == lineId);
        }

        /*return await _trainRepository.GetAllWithCondition(predicate)
            .Select(t => _mapper.MapToTrainListResponse(t)
            ).ToListAsync();*/

        var paginatedList = await _trainRepository.GetAllPaginatedQueryable(
            request.PageNumber, request.PageSize,
            predicate);

        return _mapper.MapToTrainListResponsePaginatedList(paginatedList);
    }

    // get system config related to train
    public async Task<IList<object>> GetTrainSystemConfigAsync()
    {
        _logger.Information("Get train system config");
        var configKeys = new[]
        {
            nameof(SystemConfigSetting.MAX_CAPACITY_PER_LINE_M3),
            nameof(SystemConfigSetting.MAX_CAPACITY_PER_LINE_KG),
        };

        var configs = await _systemConfigRepository.GetAllSystemConfigs(ConfigKeys: configKeys);
        return [configs.Select(c => new
        {
            c.ConfigKey,
            c.ConfigValue
        }).ToList()];
    }
}