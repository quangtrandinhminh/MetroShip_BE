using System.Linq.Expressions;
using MetroShip.Repository.Base;
using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Train;
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
    private readonly ITrainRepository _trainRepository = 
        serviceProvider.GetRequiredService<ITrainRepository>();
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
    private readonly IMapperlyMapper _mapper = serviceProvider.GetRequiredService<IMapperlyMapper>();
    private readonly IUnitOfWork _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
    private readonly ISystemConfigRepository _systemConfigRepository = 
        serviceProvider.GetRequiredService<ISystemConfigRepository>();
    private readonly IShipmentRepository _shipmentRepository =
        serviceProvider.GetRequiredService<IShipmentRepository>();

    public async Task<PaginatedListResponse<TrainListResponse>> GetAllTrainsAsync(
    PaginatedListRequest request,
    string? lineId = null,
    string? timeSlotId = null,
    DateTimeOffset? date = null)
    {
        _logger.Information("Get all trains with request: {@request}", request);

        // Build the predicate
        Expression<Func<MetroTrain, bool>> predicate = t => t.IsActive && t.DeletedAt == null;

        if (!string.IsNullOrEmpty(lineId) && Guid.TryParse(lineId, out _))
        {
            predicate = predicate.And(t => t.LineId == lineId);
        }

        if (!string.IsNullOrEmpty(timeSlotId) && Guid.TryParse(timeSlotId, out _))
        {
            predicate = predicate.And(t => t.ShipmentItineraries.Any(si => si.TimeSlotId == timeSlotId));
        }

        if (date.HasValue)
        {
            var targetDate = CoreHelper.UtcToSystemTime(date.Value).Date;
            predicate = predicate.And(t => t.ShipmentItineraries.Any(
                si => si.Date.HasValue && si.Date.Value.Date == targetDate));
        }

        // Get paginated trains with shipment itineraries
        var paginatedList = await _trainRepository.GetAllPaginatedQueryable(
            request.PageNumber,
            request.PageSize,
            predicate,
            includeProperties: t => t.ShipmentItineraries);

        // Get all unique shipment IDs
        var shipmentIds = paginatedList.Items
            .SelectMany(t => t.ShipmentItineraries)
            .Select(si => si.ShipmentId)
            .Distinct()
            .ToList();

        // Fetch shipment data in one query
        var shipmentData = await _shipmentRepository.GetAllWithCondition(
            s => shipmentIds.Contains(s.Id))
            .Select(s => new
            {
                s.Id,
                s.TotalVolumeM3,
                s.TotalWeightKg
            })
            .ToListAsync();

        // Create a lookup dictionary for better performance
        var shipmentLookup = shipmentData.ToDictionary(s => s.Id);

        // Map to response
        var response = _mapper.MapToTrainListResponsePaginatedList(paginatedList);

        // Calculate current weight and volume for each train
        foreach (var train in response.Items)
        {
            var trainEntity = paginatedList.Items.FirstOrDefault(t => t.Id == train.Id);

            if (trainEntity?.ShipmentItineraries != null)
            {
                var trainShipmentIds = trainEntity.ShipmentItineraries
                    .Select(si => si.ShipmentId)
                    .Where(shipmentLookup.ContainsKey)
                    .ToList();

                if (trainShipmentIds.Any())
                {
                    train.CurrentWeightKg = trainShipmentIds.Sum(id => shipmentLookup[id].TotalWeightKg ?? 0);
                    train.CurrentVolumeM3 = trainShipmentIds.Sum(id => shipmentLookup[id].TotalVolumeM3 ?? 0);
                }
            }
        }

        return response;
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