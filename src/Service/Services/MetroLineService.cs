using MetroShip.Repository.Base;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using MetroShip.Repository.Repositories;
using MetroShip.Service.ApiModels.MetroLine;
using MetroShip.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.Services
{
    public class MetroLineService(IServiceProvider serviceProvider) : IMetroLineService
    {
        private readonly IMetroLineRepository _metroLineRepository = serviceProvider.GetRequiredService<IMetroLineRepository>();
        private readonly ILogger<MetroLineService> _logger = serviceProvider.GetRequiredService<ILogger<MetroLineService>>();

        public async Task<IEnumerable<MetrolineResponse>> GetAllMetroLine()
        {
            _logger.LogInformation("Getting all MetroLines for dropdown.");
            var metroLines = await _metroLineRepository.GetAllAsync();
            return metroLines.Select(line => new MetrolineResponse
            {
                Id = line.Id,
                LineNameVi = line.LineNameVi,
                LineNameEn = line.LineNameEn
            }).OrderBy(line => line.LineNameVi);
        }
        public async Task<IEnumerable<MetrolineResponse>> GetAllMetroLineByRegion(string? regionId, string? regionCode)
        {
            _logger.LogInformation("Getting all MetroLines for dropdown by region.");
            var metroLines = await _metroLineRepository.GetAllWithBasePriceByRegionAsync(regionId, regionCode);
            return metroLines.Select(line => new MetrolineResponse
            {
                Id = line.Id,
                LineNameVi = line.LineNameVi,
                LineNameEn = line.LineNameEn
            }).OrderBy(line => line.LineNameVi);
        }
    }
}
