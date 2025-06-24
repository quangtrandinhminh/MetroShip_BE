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

namespace MetroShip.Service.Services
{
    public class MetroLineService : IMetroLineService
    {
        private readonly IMetroLineRepository _metroLineRepository;
        private readonly ILogger _logger;

        public MetroLineService(IServiceProvider serviceProvider)
        {
            _metroLineRepository = serviceProvider.GetRequiredService<IMetroLineRepository>();
            _logger = serviceProvider.GetRequiredService<ILogger>();
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
    }
}
