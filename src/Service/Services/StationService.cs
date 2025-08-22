using MetroShip.Repository.Base;
using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using MetroShip.Repository.Repositories;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Parcel;
using MetroShip.Service.ApiModels.Station;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Mapper;
using MetroShip.Utility.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Exceptions;

namespace MetroShip.Service.Services
{
    public class StationService : IStationService
{
    private readonly IStationRepository _stationRepository;
    private readonly IMapperlyMapper _mapper;
    private readonly ILogger _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISystemConfigRepository _systemConfigRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public StationService(
        IStationRepository stationRepository,
        IMapperlyMapper mapper,
        ILogger logger,
        IUnitOfWork unitOfWork,
        ISystemConfigRepository systemConfigRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _stationRepository = stationRepository;
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _systemConfigRepository = systemConfigRepository;
        _httpContextAccessor = httpContextAccessor;
    }
        public async Task<IEnumerable<StationListResponse>> GetAllStationsAsync(string? regionId)
        {
            var stations = string.IsNullOrWhiteSpace(regionId)
                ? _stationRepository.GetAll()
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.StationCode)
                    .ToList()
                : _stationRepository.GetStationsByRegion(regionId)
                    .OrderBy(s => s.StationCode)
                    .ToList();

            return stations.Select(_mapper.MapToStationListResponse).ToList();
        }

        public async Task<StationDetailResponse> GetStationByIdAsync(Guid id)
        {
            _logger.Information("Fetching station by Id: {Id}", id);

            var station = await _stationRepository.GetByIdAsync(id.ToString());
            if (station == null || station.DeletedAt != null)
            {
                return null; // or return a default StationDetailResponse, as appropriate
            }

            return _mapper.MapToStationDetailResponse(station);
        }

        public async Task<StationDetailResponse> CreateStationAsync(CreateStationRequest request)
        {
            _logger.Information("Creating new station: {@Request}", request);

            var station = _mapper.MapToStationEntity(request);
            station.Id = Guid.NewGuid().ToString();
            station.CreatedAt = DateTime.UtcNow;

            await _stationRepository.AddAsync(station);
            await _unitOfWork.SaveChangeAsync();

            return _mapper.MapToStationDetailResponse(station);
        }

        public async Task<StationDetailResponse> UpdateStationAsync(Guid id, UpdateStationRequest request)
        {
            _logger.Information("Updating station with Id: {Id}, {@Request}", id, request);

            var station = await _stationRepository.GetByIdAsync(id.ToString());
            if (station == null || station.DeletedAt != null)
            {
                return null; // or handle as appropriate
            }

            _mapper.MapToExistingStation(request, station);
            station.LastUpdatedAt = DateTime.UtcNow;

            _stationRepository.Update(station);
            await _unitOfWork.SaveChangeAsync();

            return _mapper.MapToStationDetailResponse(station);
        }

        public async Task DeleteStationAsync(Guid id)
        {
            _logger.Information("Deleting station with Id: {Id}", id);

            var station = await _stationRepository.GetByIdAsync(id.ToString());
            if (station == null || station.DeletedAt != null)
            {
                return; // or handle as appropriate
            }

            station.DeletedAt = DateTime.UtcNow;
            _stationRepository.Update(station);
            await _unitOfWork.SaveChangeAsync();
        }

        // get station near user location
        public async Task<List<StationResponse>> GetStationsNearUsers(NearbyStationsRequest request)
        {
            _logger.Information("Fetching stations near user at latitude: {Latitude}, longitude: {Longitude}",
                request.UserLatitude, request.UserLongitude);

            var initialMaxDistance = int.Parse(_systemConfigRepository.GetSystemConfigValueByKey(nameof(SystemConfigSetting.MAX_DISTANCE_IN_METERS))); // 5000m
            var maxCount = int.Parse(_systemConfigRepository.GetSystemConfigValueByKey(nameof(SystemConfigSetting.MAX_COUNT_STATION_NEAR_USER))); // 3
            var maxAllowedDistance = initialMaxDistance * 10;

            _logger.Information("Max distance in meters: {MaxDistance}, Max count: {MaxCount}",
                               initialMaxDistance, maxCount);

            var maxDistanceInMeters = initialMaxDistance;
            var stationIds = await _stationRepository.GetAllStationIdNearUser(
                request.UserLatitude,
                request.UserLongitude,
                maxDistanceInMeters,
                maxCount);

            while (!stationIds.Any() && maxDistanceInMeters < maxAllowedDistance)
            {
                maxDistanceInMeters *= 2;
                if (maxDistanceInMeters > maxAllowedDistance)
                    maxDistanceInMeters = maxAllowedDistance;

                stationIds = await _stationRepository.GetAllStationIdNearUser(
                    request.UserLatitude,
                    request.UserLongitude,
                    maxDistanceInMeters,
                    maxCount);
            }

            if (!stationIds.Any())
            {
                _logger.Warning("No stations found near user within {MaxAllowedDistance} meters.", maxAllowedDistance);
                throw new AppException(
                    ErrorCode.BadRequest,
                    ResponseMessageStation.NO_STATION_NEAR_USER + maxAllowedDistance + "m. ",
                    StatusCodes.Status400BadRequest
                    );
            }

            var stations = _stationRepository.GetAll()
                .Where(s => stationIds.Select(_ => _.StationId).Contains(s.Id) && s.IsActive)
                .ToList();

            var response = _mapper.MapToStationResponseList(stations);
            foreach (var station in response)
            {
                var nearbyStation = stationIds.FirstOrDefault(s => s.StationId == station.StationId);
                if (nearbyStation != null)
                {
                    station.DistanceMeters = nearbyStation.DistanceMeters;
                }
            }

            response = new List<StationResponse>(response.OrderBy(s => s.DistanceMeters));
            return response;
        }
    }
}
