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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MetroShip.Service.Services
{
    public class StationService : IStationService
{
    private readonly IStationRepository _stationRepository;
    private readonly IMapperlyMapper _mapper;
    private readonly ILogger<StationService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISystemConfigRepository _systemConfigRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public StationService(
        IStationRepository stationRepository,
        IMapperlyMapper mapper,
        ILogger<StationService> logger,
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
                ? _stationRepository.GetAll().Where(s => s.IsActive).ToList()
                : _stationRepository.GetStationsByRegion(regionId);

            return stations.Select(_mapper.MapToStationListResponse).ToList();
        }

        public async Task<StationDetailResponse> GetStationByIdAsync(Guid id)
        {
            _logger.LogInformation("Fetching station by Id: {Id}", id);

            var station = await _stationRepository.GetByIdAsync(id.ToString());
            if (station == null || station.DeletedAt != null)
            {
                return null; // or return a default StationDetailResponse, as appropriate
            }

            return _mapper.MapToStationDetailResponse(station);
        }

        public async Task<StationDetailResponse> CreateStationAsync(CreateStationRequest request)
        {
            _logger.LogInformation("Creating new station: {@Request}", request);

            var station = _mapper.MapToStationEntity(request);
            station.Id = Guid.NewGuid().ToString();
            station.CreatedAt = DateTime.UtcNow;

            await _stationRepository.AddAsync(station);
            await _unitOfWork.SaveChangeAsync();

            return _mapper.MapToStationDetailResponse(station);
        }

        public async Task<StationDetailResponse> UpdateStationAsync(Guid id, UpdateStationRequest request)
        {
            _logger.LogInformation("Updating station with Id: {Id}, {@Request}", id, request);

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
            _logger.LogInformation("Deleting station with Id: {Id}", id);

            var station = await _stationRepository.GetByIdAsync(id.ToString());
            if (station == null || station.DeletedAt != null)
            {
                return; // or handle as appropriate
            }

            station.DeletedAt = DateTime.UtcNow;
            _stationRepository.Update(station);
            await _unitOfWork.SaveChangeAsync();
        }

    }
}
