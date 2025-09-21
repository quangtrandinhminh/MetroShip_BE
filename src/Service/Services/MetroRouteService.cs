using MetroShip.Repository.Base;
using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using MetroShip.Repository.Repositories;
using MetroShip.Service.ApiModels.MetroLine;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.BusinessModels;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Mapper;
using MetroShip.Service.Validations;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Enums;
using MetroShip.Utility.Exceptions;
using MetroShip.Utility.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MetroShip.Service.ApiModels.Station;

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
        private readonly IHttpContextAccessor _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
        private readonly IMemoryCache _memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
        private const string CACHE_KEY = nameof(MetroGraph);

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

        public async Task<PaginatedListResponse<MetroRouteResponse>> GetAllMetroRoutes(PaginatedListRequest request, MetroRouteFilterRequest filter)
        {
            _logger.Information("Getting all MetroLines with pagination. PageNumber: {PageNumber}, PageSize: {PageSize}",
                               request.PageNumber, request.PageSize);

            Expression<Func<MetroLine, bool>> predicate = BuildFilterExpression(filter);

            var paginatedLines = await _metroRouteRepository.GetAllPaginatedQueryable(
            request.PageNumber,
            request.PageSize,
            predicate,
            line => line.LineCode,
            true,
            line => line.Region
            );

            return _mapper.MapToMetroLinePaginatedList(paginatedLines);
        }

        private Expression<Func<MetroLine, bool>> BuildFilterExpression(MetroRouteFilterRequest filter)
        {
            Expression<Func<MetroLine, bool>> predicate = line => line.DeletedAt == null;

            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.LineNameVi))
                {
                    predicate = predicate.And(x =>
                                           EF.Functions.ILike(x.LineNameVi, $"%{filter.LineNameVi}%"));
                }

                if (!string.IsNullOrEmpty(filter.LineNameEn))
                {
                    predicate = predicate.And(x =>
                                                              EF.Functions.ILike(x.LineNameEn, $"%{filter.LineNameEn}%"));
                }

                if (!string.IsNullOrEmpty(filter.LineCode))
                {
                    predicate = predicate.And(x =>
                                           EF.Functions.ILike(x.LineCode, $"%{filter.LineCode}%"));
                }

                if (!string.IsNullOrEmpty(filter.RegionId))
                {
                    predicate = predicate.And(line => line.RegionId == filter.RegionId);
                }
                if (filter.IsActive.HasValue)
                {
                    predicate = predicate.And(line => line.IsActive == filter.IsActive.Value);
                }
            }

            return predicate;
        }

        public async Task<MetroRouteResponseDetails> GetMetroRouteById(string metroRouteId)
        {
            _logger.Information("Getting MetroLine details by ID: {MetroLineId}", metroRouteId);

            var metroLine = await _metroRouteRepository.GetSingleAsync(
            line => line.Id == metroRouteId && line.DeletedAt == null, false,
            line => line.Region,
            line => line.Trains);

            if (metroLine == null)
            {
                throw new AppException(
                ErrorCode.NotFound,
                MetroRouteMessageConstants.METROROUTE_NOT_FOUND,
                StatusCodes.Status404NotFound);
            }

            var routes = await _routeStationRepository.GetAll()
                .Where(r => r.LineId == metroLine.Id && r.Direction == DirectionEnum.Forward)
                .OrderBy(r => r.SeqOrder)
                .ToListAsync();

            var stations = await _stationRepository.GetAll()
                .Where(s => metroLine.StationList.Select(sl => sl.StationId).Contains(s.Id))
                .ToListAsync();

            // Map stations to response with sequence order
            metroLine.StationList.Sort((a, b) =>
                           metroLine.StationList.IndexOf(a).CompareTo(metroLine.StationList.IndexOf(b)));

            var result = _mapper.MapToMetroLineResponseDetails(metroLine);
            foreach (var stationItem in metroLine.StationList)
            {
                var station = stations.FirstOrDefault(s => s.Id == stationItem.StationId);
                
                if (station != null)
                {
                    station.StationCode = stationItem.StationCode;
                    var stationResult = _mapper.MapToStationDetailResponse(station);
                    var route = routes.FirstOrDefault(r => r.ToStationId == station.Id);

                    // sum all km from the first route to this seq order
                    var atKm = routes
                        .Where(r => r.SeqOrder <= (route?.SeqOrder ?? 0))
                        .Sum(r => r.LengthKm);

                    stationResult.AtKm = atKm;
                    result.Stations.Add(stationResult);
                }
            }

            return result;
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
                            MetroRouteMessageConstants.METROROUTE_EXISTED + $" with MetroRouteCode: {metroRoute.LineCode}",
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

                        // check if all existing stations are valid
                        if (existingStations.Count != existingStationIds.Count)
                        {
                            throw new AppException(
                            ErrorCode.BadRequest,
                            "Some existing stations not found or invalid. Please check the provided station IDs.",
                            StatusCodes.Status400BadRequest);
                        }

                        // Update IsMultiLine for existing stations
                        foreach (var station in existingStations)
                        {
                            var order = request.Stations.FirstOrDefault(
                                s => s.Id == station.Id);

                            var stationCodeList = station.StationCodeList;
                            if (stationCodeList.Any()) station.IsMultiLine = true;
                            /*stationCodeList.Add(new StationCodeListItem
                            {
                                RouteId = metroRoute.Id,
                                StationCode = MetroCodeGenerator.GenerateStationCode(
                                    request.Stations.IndexOf(order) + 1,
                                    request.LineNumber,
                                    region.RegionCode)
                            });*/

                            stationCodeList.Add(new StationCodeListItem
                            {
                                RouteId = metroRoute.Id,
                                StationCode = MetroCodeGenerator.GenerateStationCode(
                                    request.Stations.IndexOf(order) + 1,
                                    metroRoute.LineCode
                                    )
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

                        station.RegionId = metroRoute.RegionId;
                        station.IsActive = false;

                        /*station.StationCode = MetroCodeGenerator.GenerateStationCode(
                            request.Stations.IndexOf(order) + 1,
                            request.LineNumber,
                            region.RegionCode);*/

                        station.StationCode = MetroCodeGenerator.GenerateStationCode(
                            request.Stations.IndexOf(order) + 1,
                            metroRoute.LineCode
                            );

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
                        MetroRouteMessageConstants.METROROUTE_STATION_COUNT_LESS_THAN_2,
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
                    var saveResult1 = await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
                    _logger.Information("First save completed with result: {Result}", saveResult1);

                    // Find active station indices
                    var activeIndices = request.Stations
                        .Select((s, idx) => new { s, idx })
                        //.Where(x => x.s.IsActive.GetValueOrDefault())
                        .Select(x => x.idx)
                        .ToList();

                    /*_logger.Information("Active station indices: [{Indices}]", string.Join(", ", activeIndices));

                    // Validate active stations
                    if (!activeIndices.Any() || activeIndices.First() != 0 || activeIndices.Last() != request.Stations.Count - 1)
                    {
                        throw new Exception("First and last station must be active.");
                    }*/

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
                    var finalResult = await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
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

        // activate a metro line, ensure that all stations are active, all routes are active, contains at least 2 metroTrains
        public async Task<string> ActivateMetroLine(string metroRouteId)
        {
            _logger.Information("Activating MetroLine with ID: {MetroLineId}", metroRouteId);

            var metroLine = await _metroRouteRepository.GetSingleAsync(
                    line => line.Id == metroRouteId && !line.IsActive, false,
                    l => l.Trains);
            if (metroLine == null || metroLine.DeletedAt != null)
            {
                throw new AppException(
                ErrorCode.NotFound,
                MetroRouteMessageConstants.METROROUTE_ALREADY_ACTIVATED,
                StatusCodes.Status404NotFound);
            }

            /*if (metroLine.Trains.Count < 2)
            {
                throw new AppException(
                ErrorCode.BadRequest,
                MetroRouteMessageConstants.METROROUTE_NOT_ENOUGH_TRAINS,
                StatusCodes.Status400BadRequest);
            }*/

            var stationIds = metroLine.StationList.Select(sc => sc.StationId).ToList();

            // Check if all stations are active
            var stations = await _stationRepository.GetAll()
                .Where(s => stationIds.Contains(s.Id))
                .ToListAsync();

            // Activate the metro line
            metroLine.IsActive = true;
            _metroRouteRepository.Update(metroLine);
            foreach (var station in stations)
            {
                station.IsActive = true;
                _stationRepository.Update(station);
            }

            await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
            // delete metrograph cache
            if (_memoryCache.TryGetValue(CACHE_KEY, out MetroGraph? metroGraph))
            {
                _memoryCache.Remove(CACHE_KEY);
                _logger.Information("MetroGraph cache cleared");
            }

            return MetroRouteMessageConstants.METROROUTE_ACTIVATE_SUCCESS;
        }

        public async Task<string> UpdateMetroLine(MetroRouteUpdateRequest request)
        {
            _logger.Information("Updating MetroLine with ID: {MetroLineId}", request.Id);

            request.ValidateMetroLineUpdateRequest();
            _logger.Information("Validation passed");

            var metroLine = await _metroRouteRepository.GetSingleAsync(
                               line => line.Id == request.Id && line.DeletedAt == null, false);
            if (metroLine == null)
            {
                throw new AppException(
                ErrorCode.NotFound,
                MetroRouteMessageConstants.METROROUTE_NOT_FOUND,
                StatusCodes.Status404NotFound);
            }

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

            // If LineNumber or RegionId changed, update LineCode
            if (metroLine.RegionId != request.RegionId /*|| metroLine.LineNumber != request.LineNumber*/)
            {
                metroLine.LineCode = string.IsNullOrEmpty(request.LineCode) ?
                    MetroCodeGenerator.GenerateMetroLineCode(
                        region.RegionCode,
                        metroLine.LineNumber.Value) : request.LineCode;

                // check if the new line code is already existed
                var existingLine = await _metroRouteRepository.GetAll()
                    .FirstOrDefaultAsync(l => l.LineCode == metroLine.LineCode && l.Id != metroLine.Id);
                if (existingLine != null)
                {
                    throw new AppException(
                    ErrorCode.BadRequest,
                    MetroRouteMessageConstants.METROROUTE_EXISTED + $" with MetroRouteCode: {metroLine.LineCode}",
                    StatusCodes.Status400BadRequest);
                }
            }
            else if (!string.IsNullOrEmpty(request.LineCode) && metroLine.LineCode != request.LineCode)
            {
                // If LineCode is provided and different, check for uniqueness
                var existingLine = await _metroRouteRepository.GetAll()
                    .FirstOrDefaultAsync(l => l.LineCode == request.LineCode && l.Id != metroLine.Id);
                if (existingLine != null)
                {
                    throw new AppException(
                    ErrorCode.BadRequest,
                    MetroRouteMessageConstants.METROROUTE_EXISTED + $" with MetroRouteCode: {existingLine.LineCode}");
                }
            }

            _mapper.MapToMetroLineEntity(request, metroLine);
            _metroRouteRepository.Update(metroLine);
            await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
            return MetroRouteMessageConstants.ROUTE_UPDATE_SUCCESS;
        }

        public async Task<List<MetroLineDto>> GetAllMetroLinesWithStationsAsync()
        {
            var metroLines = await _metroRouteRepository.GetAll()
                .Include(ml => ml.Routes)
                    .ThenInclude(r => r.FromStation)
                .Include(ml => ml.Routes)
                    .ThenInclude(r => r.ToStation) 
                .Where(ml => ml.IsActive)
                .OrderBy(ml => ml.LineNumber)
                .ToListAsync();

            var result = new List<MetroLineDto>();

            foreach (var metroLine in metroLines)
            {
                var metroLineDto = await MapToMetroLineDtoAsync(metroLine);
                result.Add(metroLineDto);
            }

            return result;
        }

        public async Task<MetroLineDto> GetMetroLineByIdAsync(string lineId)
        {
            var metroLine = await _metroRouteRepository.GetAll()
                .Include(ml => ml.Routes)
                    .ThenInclude(r => r.FromStation)
                .Include(ml => ml.Routes)
                    .ThenInclude(r => r.ToStation)
                .FirstOrDefaultAsync(ml => ml.Id == lineId && ml.IsActive);

            if (metroLine == null)
                return null;

            return await MapToMetroLineDtoAsync(metroLine);
        }

        private async Task<MetroLineDto> MapToMetroLineDtoAsync(MetroLine metroLine)
        {
            var dto = new MetroLineDto
            {
                Id = metroLine.Id,
                RegionId = metroLine.RegionId,
                LineNameVi = metroLine.LineNameVi,
                LineNameEn = metroLine.LineNameEn,
                LineCode = metroLine.LineCode,
                LineNumber = metroLine.LineNumber,
                LineType = metroLine.LineType,
                LineOwner = metroLine.LineOwner,
                TotalKm = metroLine.TotalKm,
                TotalStations = metroLine.TotalStations,
                RouteTimeMin = metroLine.RouteTimeMin,
                DwellTimeMin = metroLine.DwellTimeMin,
                ColorHex = metroLine.ColorHex,
                IsActive = metroLine.IsActive
            };

            // Get all stations for this metro line
            var stationIds = metroLine.StationList
                .Select(s => s.StationId)
                .ToList();

            var stations = await _stationRepository.GetAll()
                .Where(s => stationIds.Contains(s.Id))
                .ToListAsync();

            // Map stations with sequence order
            foreach (var stationItem in metroLine.StationList)
            {
                var station = stations.FirstOrDefault(s => s.Id == stationItem.StationId);
                if (station != null)
                {
                    dto.Stations.Add(new StationDto
                    {
                        Id = station.Id,
                        StationCode = stationItem.StationCode,
                        StationNameVi = station.StationNameVi,
                        StationNameEn = station.StationNameEn,
                        Latitude = station.Latitude.HasValue ? (decimal?)station.Latitude.Value : null,
                        Longitude = station.Longitude.HasValue ? (decimal?)station.Longitude.Value : null,
                        SeqOrder = metroLine.StationList.IndexOf(stationItem) + 1
                    });
                }
            }

            // Order stations by sequence
            dto.Stations = dto.Stations.OrderBy(s => s.SeqOrder).ToList();

            // Generate interpolated paths for all routes
            dto.RoutePaths = GenerateRoutePaths(metroLine.Routes.ToList());

            return dto;
        }

        private List<RoutePathDto> GenerateRoutePaths(List<Route> routes)
        {
            const int steps = 10;
            var routePaths = new List<RoutePathDto>();

            foreach (var route in routes.OrderBy(r => r.SeqOrder))
            {
                if (route.FromStation?.Latitude == null || route.FromStation?.Longitude == null ||
                    route.ToStation?.Latitude == null || route.ToStation?.Longitude == null)
                {
                    continue;
                }

                var interpolatedPoints = Enumerable.Range(0, steps + 1).Select(s =>
                {
                    var progress = s / (double)steps;
                    var (latStep, lngStep) = GeoUtils.Interpolate(
                        (double)route.FromStation.Latitude.Value,
                        (double)route.FromStation.Longitude.Value,
                        (double)route.ToStation.Latitude.Value,
                        (double)route.ToStation.Longitude.Value,
                        progress
                    );
                    return new GeoPoint { Latitude = latStep, Longitude = lngStep };
                }).ToList();

                routePaths.Add(new RoutePathDto
                {
                    FromStationId = route.FromStationId,
                    ToStationId = route.ToStationId,
                    InterpolatedPoints = interpolatedPoints
                });
            }

            return routePaths;
        }
    }
}