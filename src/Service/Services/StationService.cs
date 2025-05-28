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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.Services
{
    public class StationService(IServiceProvider serviceProvider) : IStationService
    {
        private readonly IBaseRepository<Station> _stationRepository = serviceProvider.GetRequiredService<IBaseRepository<Station>>();
        private readonly IMapperlyMapper _mapper = serviceProvider.GetRequiredService<IMapperlyMapper>();
        private readonly ILogger<StationService> _logger = serviceProvider.GetRequiredService<ILogger<StationService>>();
        private readonly IUnitOfWork _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
        private readonly ISystemConfigRepository _systemConfigRepository = serviceProvider.GetRequiredService<ISystemConfigRepository>();
        private readonly IHttpContextAccessor _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();

        public async Task<PaginatedListResponse<StationListResponse>> GetAllStationsAsync(PaginatedListRequest request)
        {
            _logger.LogInformation("Fetching all stations with request: {@Request}", request);

            var stations = await _stationRepository.GetAllPaginatedQueryable(
                request.PageNumber,
                request.PageSize,
                x => x.DeletedAt == null);

            var stationList = _mapper.MapToStationListResponsePaginatedList(stations);
            return stationList;
        }

        public async Task<StationDetailResponse> GetStationByIdAsync(Guid id)
        {
            _logger.LogInformation("Fetching station by Id: {Id}", id);

            var station = await _stationRepository.GetByIdAsync(id);
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

            var station = await _stationRepository.GetByIdAsync(id);
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

            var station = await _stationRepository.GetByIdAsync(id);
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
