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
using MetroShip.Service.Mapper;
using MetroShip.Service.Validations;

namespace MetroShip.Service.Services
{
    public class MetroLineService : IMetroLineService
    {
        private readonly IMetroLineRepository _metroLineRepository;
        private readonly ILogger _logger;
        private readonly IMapperlyMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStationRepository _stationRepository;
        private readonly IBaseRepository<Route> _routeRepository;

        public MetroLineService(IServiceProvider serviceProvider)
        {
            _metroLineRepository = serviceProvider.GetRequiredService<IMetroLineRepository>();
            _logger = serviceProvider.GetRequiredService<ILogger>();
            _mapper = serviceProvider.GetRequiredService<IMapperlyMapper>();
            _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
            _stationRepository = serviceProvider.GetRequiredService<IStationRepository>();
            _routeRepository = serviceProvider.GetRequiredService<IBaseRepository<Route>>();
        }

        public async Task<List<MetrolineResponse>> GetAllMetroLine()
        {
            _logger.Information("Getting all MetroLines for dropdown.");
            var metroLines = await _metroLineRepository.GetAll()
                .Select(line => new MetrolineResponse
                {
                    Id = line.Id,
                    LineNameVi = line.LineNameVi,
                    LineNameEn = line.LineNameEn
                })
                .OrderBy(line => line.LineNameVi)
                .ToListAsync();
            return metroLines;
        }

        public async Task<List<MetrolineGetByRegionResponse>> GetAllMetroLineByRegion(string? regionId)
        {
            _logger.Information("Getting all MetroLines for dropdown by region.");
            var metroLines = await _metroLineRepository.GetAllWithBasePriceByRegionAsync(regionId);
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
        public async Task<int> CreateMetroLine(MetroLineCreateRequest request)
        {
            _logger.Information("Creating new MetroLine: {LineNameVi}", request.LineNameVi);
            MetroLineValidator.ValidateMetroLineCreateRequest(request);

            var metroLine = _mapper.MapToMetroLineEntity(request);

            // Fetch existing stations
            var stationIds = request.Stations.Select(st => st.Id).ToList();
            var existingStations = await _stationRepository.GetAll()
                .Where(s => stationIds.Contains(s.Id))
                .ToListAsync();

            var existingStationIds = existingStations.Select(s => s.Id).ToHashSet();

            // Mark existing stations as multiline
            foreach (var station in existingStations)
            {
                if (!station.IsMultiLine)
                {
                    station.IsMultiLine = true;
                    _stationRepository.Update(station);
                }
            }

            // Add new stations if not exist
            var newStations = request.Stations
                .Where(st => !existingStationIds.Contains(st.Id))
                .Select(st => _mapper.MapToStationEntity(st))
                .ToList();

            foreach (var newStation in newStations)
            {
                newStation.IsMultiLine = false; // or true if logic demands
                await _stationRepository.AddAsync(newStation);
            }

            // Collect all station entities for route creation
            var allStations = existingStations.Concat(newStations).ToList();

            // Create metro line
            await _metroLineRepository.AddAsync(metroLine);

            // request.Stations is the ordered list with ToNextStationKm
            var requestStations = request.Stations; // List<StationDTO>, ordered

            // Find indices of all active stations in request
            var activeIndices = requestStations
                .Select((s, idx) => new { s, idx })
                .Where(x => x.s.IsActive.Value)
                .Select(x => x.idx)
                .ToList();

            // Validate: first and last station must be active
            if (activeIndices.First() != 0 || activeIndices.Last() != requestStations.Count - 1)
                throw new Exception("First and last station must be active.");

            // Forward routes (direction=0)
            int legOrder = 1;
            for (int i = 0; i < activeIndices.Count - 1; i++)
            {
                int fromIdx = activeIndices[i];
                int toIdx = activeIndices[i + 1];

                // Sum ToNextStationKm for all stations between fromIdx (inclusive) and toIdx (exclusive)
                double lengthKm = 0;
                for (int j = fromIdx; j < toIdx; j++)
                {
                    lengthKm += (double)requestStations[j].ToNextStationKm;
                }

                var fromStation = requestStations[fromIdx];
                var toStation = requestStations[toIdx];

                // Create forward route (direction=0)
                await _routeRepository.AddAsync(new Route
                {
                    LineId = metroLine.Id,
                    FromStationId = fromStation.Id,
                    ToStationId = toStation.Id,
                    LengthKm = (decimal)lengthKm,
                    SeqOrder = legOrder,
                    Direction = Utility.Enums.DirectionEnum.Forward, // Assuming DirectionEnum has Forward
                    // Names, etc.
                });

                // Create reverse route (direction=1)
                await _routeRepository.AddAsync(new Route
                {
                    LineId = metroLine.Id,
                    FromStationId = toStation.Id,
                    ToStationId = fromStation.Id,
                    LengthKm = (decimal)lengthKm,
                    SeqOrder = legOrder,
                    Direction = Utility.Enums.DirectionEnum.Backward, // Assuming DirectionEnum has Reverse
                    // Names, etc.
                });

                legOrder++;
            }

            return await _unitOfWork.SaveChangeAsync();
        }
    }
}
