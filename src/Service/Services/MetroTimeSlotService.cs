using MetroShip.Repository.Base;
using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels.MetroTimeSlot;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Mapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.Services
{
    public class MetroTimeSlotService(IServiceProvider serviceProvider) : IMetroTimeSlotService
    {
        private readonly IBaseRepository<MetroTimeSlot> _metroTimeSlotRepository = serviceProvider.GetRequiredService<IBaseRepository<MetroTimeSlot>>();
        private readonly IMapperlyMapper _mapper = serviceProvider.GetRequiredService<IMapperlyMapper>();
        private readonly ILogger<MetroTimeSlotService> _logger = serviceProvider.GetRequiredService<ILogger<MetroTimeSlotService>>();
        private readonly IUnitOfWork _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();

        public async Task<IEnumerable<MetroTimeSlotResponse>> GetAllForMetroTimeSlot()
        {
            _logger.LogInformation("Getting all MetroTimeSlots for dropdown.");

            var timeSlots = await _metroTimeSlotRepository.GetAllAsync();
            timeSlots = timeSlots
                .Where(s => s.DeletedAt == null)
                .OrderBy(s => s.OpenTime)
                .ToList();

            return timeSlots.Select(slot => new MetroTimeSlotResponse
            {
                Id = slot.Id,
                DayOfWeek = slot.DayOfWeek,
                SpecialDate = slot.SpecialDate,
                OpenTime = slot.OpenTime,
                CloseTime = slot.CloseTime,
                Shift = slot.Shift,
                IsAbnormal = slot.IsAbnormal
            }).OrderBy(slot => slot.OpenTime).ToList();
        }

    }
}
