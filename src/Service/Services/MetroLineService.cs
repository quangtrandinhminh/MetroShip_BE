using MetroShip.Repository.Base;
using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels.MetroLine;
using MetroShip.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.Services
{
    public class MetroLineService(IServiceProvider serviceProvider) : IMetroLineService
    {
        private readonly IBaseRepository<MetroLine> _metroLineRepository = serviceProvider.GetRequiredService<IBaseRepository<MetroLine>>();
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
    }
}
