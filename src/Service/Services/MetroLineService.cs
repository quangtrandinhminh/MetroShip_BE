using MetroShip.Repository.Base;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels.MetroLine;
using MetroShip.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace MetroShip.Service.Services
{
    public class MetroLineService(IServiceProvider serviceProvider) : IMetroLineService
    {
        private readonly IMetroLineRepository _metroLineRepository = serviceProvider
            .GetRequiredService<IMetroLineRepository>();
        private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();

        public async Task<IEnumerable<MetrolineResponse>> GetAllMetroLine()
        {
            _logger.Information("Getting all MetroLines for dropdown.");
            var metroLines = await _metroLineRepository.GetAll()
                .Select(line => new MetrolineResponse
            {
                Id = line.Id,
                LineNameVi = line.LineNameVi,
                LineNameEn = line.LineNameEn
            }).OrderBy(line => line.LineNameVi).ToListAsync();

            return metroLines;
        }
    }
}
