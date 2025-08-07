using MetroShip.Repository.Base;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using MetroShip.Repository.Repositories;
using MetroShip.Service.ApiModels.MetroLine;
using MetroShip.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetroShip.Repository.Infrastructure;
using MetroShip.Service.BusinessModels;
using MetroShip.Service.Mapper;
using MetroShip.Service.Validations;
using MetroShip.Utility.Enums;
using MetroShip.Utility.Exceptions;
using MetroShip.Utility.Constants;
using Microsoft.AspNetCore.Http;
using MetroShip.Utility.Helpers;
using MetroShip.Service.ApiModels.PaginatedList;
using System.Linq.Expressions;

namespace MetroShip.Service.Services
{
    public class MetroRouteService(IServiceProvider serviceProvider) : IMetroRouteService
    {
        private readonly IMetroRouteRepository _metroRouteRepository = serviceProvider.GetRequiredService<IMetroRouteRepository>();
        private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
        private readonly IMapperlyMapper _mapper = serviceProvider.GetRequiredService<IMapperlyMapper>();
        private readonly IUnitOfWork _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
        private readonly IStationRepository _stationRepository = serviceProvider.GetRequiredService<IStationRepository>();
        private readonly IBaseRepository<Route> _routeStationRepository = serviceProvider.GetRequiredService<IBaseRepository<Route>>();
        private readonly IRegionRepository _regionRepository = serviceProvider.GetRequiredService<IRegionRepository>();

        public async Task<List<MetroLineItineraryResponse>> GetAllMetroRouteDropdown(string? stationId)
        {
            _logger.Information("Getting all MetroLines for dropdown.");
            Expression<Func<MetroLine, bool>> predicate = line => line.IsActive && line.DeletedAt == null;

            if (!string.IsNullOrEmpty(stationId))
            {
                predicate = line => line.IsActive && line.DeletedAt == null
                    && line.Routes.Any(s => s.FromStationId == stationId || s.ToStationId == stationId);
            }

            var metroLines = await _metroRouteRepository.GetAll()
                .Where(predicate)
                .Select(line => new MetroLineItineraryResponse
                {
                    Id = line.Id,
                    LineNameVi = line.LineNameVi,
                    LineNameEn = line.LineNameEn,
                    ColorHex = line.ColorHex,
                })
                .OrderBy(line => line.LineNameVi)
                .ToListAsync();
            return metroLines;
        }

        public async Task<List<MetroRouteResponse>> GetAllMetroRoutes(PaginatedListRequest request)
        {
            throw new NotImplementedException("This method is not implemented");
        }

        public async Task<MetroRouteResponseDetails> GetMetroRouteById(string metroRouteId)
        {
            throw new NotImplementedException("This method is not implemented");
        }

        public async Task<List<MetrolineGetByRegionResponse>> GetAllMetroLineByRegion(string? regionId)
        {
            _logger.Information("Getting all MetroLines for dropdown by region.");
            var metroLines = await _metroRouteRepository.GetAllWithBasePriceByRegionAsync(regionId);
            return metroLines
                .Select(line => new MetrolineGetByRegionResponse
                {
                    Id = line.Id,
                    LineNameVi = line.LineNameVi,
                    LineNameEn = line.LineNameEn,
                    regionCode = line.RegionId
                })
                .OrderBy(line => line.LineNameVi)
                .ToList();
        }

        // create a new metro line, if the station is exist, update IsMultiline to true,
        // else add new station, then create routes for each pairs of stations
        public async Task<int> CreateMetroRoute(MetroRouteRequest request)
        {
            _logger.Information("Creating new MetroLine: {LineNameVi}", request.LineNameVi);

            try
            {
                request.ValidateMetroLineCreateRequest();
                _logger.Information("Validation passed");

                // Check if transaction is available
                await using var transaction = await _unitOfWork.BeginTransactionAsync();
                _logger.Information("Transaction started");

                try
                {
                    var region = await _regionRepository.GetAll()
                        .Where(s => s.Id == request.RegionId)
                        .Select(s => new { s.Id, s.RegionCode })
                        .FirstOrDefaultAsync();
                    if (region == null)
                    {
                        throw new AppException(
                            ErrorCode.BadRequest,
                            RegionMessageConstants.REGION_NOT_FOUND,
                            StatusCodes.Status400BadRequest);
                    }

                    var metroRoute = _mapper.MapToMetroLineEntity(request);
                    metroRoute.LineCode = string.IsNullOrEmpty(request.LineCode) ?
                                MetroCodeGenerator.GenerateMetroLineCode(
                                region.RegionCode,
                                request.LineNumber) : request.LineCode;

                    // find existing metro line with same code
                    var existingLine = await _metroRouteRepository.GetAll()
                        .FirstOrDefaultAsync(l => l.LineCode == metroRoute.LineCode);
                    if (existingLine != null)
                    {
                        throw new AppException(
                            ErrorCode.BadRequest,
                            RouteMessageConstants.ROUTE_EXISTED + $" with MetroRouteCode: {metroRoute.LineCode}",
                            StatusCodes.Status400BadRequest);
                    }

                    metroRoute.TotalKm = request.Stations.Sum(s => s.ToNextStationKm);
                    metroRoute.TotalStations = request.Stations.Count;
                    _logger.Information("MetroRoute entity mapped: {LineCode}", metroRoute.LineCode);

                    // Handle existing stations (none in your case since all IDs are null)
                    var existingStationIds = request.Stations
                        .Where(st => !string.IsNullOrEmpty(st.Id))
                        .Select(st => st.Id)
                        .ToList();

                    _logger.Information("Found {Count} existing stations", existingStationIds.Count);

                    var existingStations = new List<Station>();
                    if (existingStationIds.Any())
                    {
                        // Retrieve existing stations from the database
                        existingStations = await _stationRepository.GetAll()
                            .Where(s => existingStationIds.Contains(s.Id))
                            .ToListAsync();

                        _logger.Information("Retrieved {Count} existing stations from database", 
                            existingStations.Count);
                        // Update IsMultiLine for existing stations
                        foreach (var station in existingStations)
                        {
                            var order = request.Stations.FirstOrDefault(
                                s => s.Id == station.Id);

                            var stationCodeList = station.StationCodeList;
                            if (stationCodeList.Any()) station.IsMultiLine = true;
                            stationCodeList.Add(new StationCodeListItem
                            {
                                RouteId = metroRoute.Id,
                                StationCode = MetroCodeGenerator.GenerateStationCode(
                                    request.Stations.IndexOf(order) + 1,
                                    request.LineNumber,
                                    region.RegionCode)
                            });

                            station.StationCodeListJSON = stationCodeList.ToJsonString();
                            _stationRepository.Update(station);
                            _logger.Information("Updated IsMultiLine for station: {StationNameEn}",
                                station.StationNameEn);
                        }
                    }

                    // Create new stations (all stations in your case)
                    var newStations = request.Stations
                        .Where(st => string.IsNullOrEmpty(st.Id))
                        .Select(st => {
                            var station = _mapper.MapToStationEntity(st);
                            return station;
                        })
                        .ToList();

                    foreach (var station in newStations)
                    {
                        var order = request.Stations.FirstOrDefault(
                            s => s.StationNameEn != null &&
                            s.StationNameEn.Equals(station.StationNameEn));

                        station.RegionId = region.Id;

                        station.StationCode = MetroCodeGenerator.GenerateStationCode(
                            request.Stations.IndexOf(order) + 1,
                            request.LineNumber,
                            region.RegionCode);

                        station.StationCodeListJSON =
                            System.Text.Json.JsonSerializer.Serialize(
                                new List<StationCodeListItem>
                                {
                                    new StationCodeListItem
                                    {
                                        RouteId = metroRoute.Id,
                                        StationCode = station.StationCode
                                    }
                                }
                        );
                        _logger.Information("Generated station code: {StationCode}", station.StationCode);
                    }
                    _logger.Information("Creating {Count} new stations", newStations.Count);

                    // Validate that at least 2 stations are provided
                    if (newStations.Count < 2 && existingStations.Count < 2)
                    {
                        throw new AppException(
                        ErrorCode.BadRequest,
                        RouteMessageConstants.METROLINE_STATION_COUNT_LESS_THAN_2,
                        StatusCodes.Status400BadRequest);
                    }

                    // Create ordered station list same as request order
                    var orderedStations = new List<Station>();
                    var newStationIndex = 0;

                    for (int i = 0; i < request.Stations.Count; i++)
                    {
                        var requestStation = request.Stations[i];

                        if (!string.IsNullOrEmpty(requestStation.Id))
                        {
                            var existingStation = existingStations.FirstOrDefault(s => s.Id == requestStation.Id);
                            if (existingStation != null)
                            {
                                orderedStations.Add(existingStation);
                            }
                        }
                        else
                        {
                            if (newStationIndex < newStations.Count)
                            {
                                orderedStations.Add(newStations[newStationIndex]);
                                newStationIndex++;
                            }
                        }
                    }

                    _logger.Information("Ordered stations count: {Count}", orderedStations.Count);

                    // serialize station list to JSON
                    metroRoute.StationListJSON = System.Text.Json.JsonSerializer.Serialize(
                                               orderedStations.Select(s => new StationListItem
                                               {
                                                   StationId = s.Id,
                                                   StationCode = s.StationCode
                                               }).ToList());
                    // Add metro line first
                    await _metroRouteRepository.AddAsync(metroRoute);
                    _logger.Information("MetroLine added to context");

                    // Add new stations
                    if (newStations.Any())
                    {
                        await _stationRepository.AddRangeAsync(newStations);
                        _logger.Information("Stations added to context");
                    }

                    // Save metroLine & station to get IDs
                    var saveResult1 = await _unitOfWork.SaveChangeAsync();
                    _logger.Information("First save completed with result: {Result}", saveResult1);

                    // Find active station indices
                    var activeIndices = request.Stations
                        .Select((s, idx) => new { s, idx })
                        //.Where(x => x.s.IsActive.GetValueOrDefault())
                        .Select(x => x.idx)
                        .ToList();

                    _logger.Information("Active station indices: [{Indices}]", string.Join(", ", activeIndices));

                    // Validate active stations
                    if (!activeIndices.Any() || activeIndices.First() != 0 || activeIndices.Last() != request.Stations.Count - 1)
                    {
                        throw new Exception("First and last station must be active.");
                    }

                    // Create routes
                    var routes = new List<Route>();
                    int legOrder = 1;

                    for (int i = 0; i < activeIndices.Count - 1; i++)
                    {
                        int fromIdx = activeIndices[i];
                        int toIdx = activeIndices[i + 1];

                        // Calculate length
                        double lengthKm = 0;
                        for (int j = fromIdx; j < toIdx; j++)
                        {
                            lengthKm += (double)(request.Stations[j].ToNextStationKm);
                        }

                        var fromStation = orderedStations[fromIdx];
                        var toStation = orderedStations[toIdx];

                        _logger.Information("Creating route from {From} to {To}, length: {Length}km",
                            fromStation.StationNameVi, toStation.StationNameVi, lengthKm);

                        // Forward route
                        routes.Add(new Route
                        {
                            LineId = metroRoute.Id,
                            FromStationId = fromStation.Id,
                            ToStationId = toStation.Id,
                            LengthKm = (decimal)lengthKm,
                            SeqOrder = legOrder,
                            Direction = DirectionEnum.Forward,
                            RouteNameVi = $"{fromStation.StationNameVi} – {toStation.StationNameVi}",
                            RouteNameEn = $"{fromStation.StationNameEn} – {toStation.StationNameEn}",
                            RouteCode = MetroCodeGenerator.GenerateRouteCode(
                                metroRoute.LineCode, fromStation.StationCode, toStation.StationCode),
                        });

                        // Reverse route
                        routes.Add(new Route
                        {
                            LineId = metroRoute.Id,
                            FromStationId = toStation.Id,
                            ToStationId = fromStation.Id,
                            LengthKm = (decimal)lengthKm,
                            SeqOrder = activeIndices.Count - legOrder, // Reverse order
                            Direction = DirectionEnum.Backward,
                            RouteNameVi = $"{toStation.StationNameVi} – {fromStation.StationNameVi}",
                            RouteNameEn = $"{toStation.StationNameEn} – {fromStation.StationNameEn}",
                            RouteCode = MetroCodeGenerator.GenerateRouteCode(
                                metroRoute.LineCode, toStation.StationCode, fromStation.StationCode)
                        });

                        legOrder++;
                    }

                    _logger.Information("Created {Count} routes", routes.Count);

                    // Add routes
                    if (routes.Any())
                    {
                        await _routeStationRepository.AddRangeAsync(routes);
                        _logger.Information("Routes added to context");
                    }

                    // Final save
                    var finalResult = await _unitOfWork.SaveChangeAsync();
                    _logger.Information("Final save completed with result: {Result}", finalResult);

                    await transaction.CommitAsync();
                    _logger.Information("Transaction committed successfully");

                    return finalResult;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error in transaction, rolling back");
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating MetroLine: {Message}", ex.Message);
                throw;
            }
        }
    }
}
