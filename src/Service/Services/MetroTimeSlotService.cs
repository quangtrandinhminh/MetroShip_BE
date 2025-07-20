using MetroShip.Repository.Base;
using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels.MetroTimeSlot;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Mapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MetroShip.Utility.Config;

namespace MetroShip.Service.Services
{
    public class MetroTimeSlotService(IServiceProvider serviceProvider) : IMetroTimeSlotService
    {
        private readonly IMetroTimeSlotRepository _metroTimeSlotRepository = serviceProvider.GetRequiredService<IMetroTimeSlotRepository>();
        private readonly IMapperlyMapper _mapper = serviceProvider.GetRequiredService<IMapperlyMapper>();
        private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
        private readonly IUnitOfWork _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
        private readonly ISystemConfigRepository _systemConfigRepository = serviceProvider.GetRequiredService<ISystemConfigRepository>();
        private readonly SystemConfigSetting _systemConfigSetting;

        public async Task<IList<MetroTimeSlotResponse>> GetAllForMetroTimeSlot()
        {
            _logger.Information("Getting all MetroTimeSlots for dropdown.");

            var shiftConfig = int.Parse(_systemConfigRepository
                .GetSystemConfigValueByKey(nameof(_systemConfigSetting.SCHEDULE_BEFORE_SHIFT_MINUTES)));
            var timeSlots = await _metroTimeSlotRepository.GetAll()
                .Where(s => s.DeletedAt == null)
                .OrderBy(s => s.OpenTime)
                .Select(slot => _mapper.MapToMetroTimeSlotResponse(slot) 
                /*new MetroTimeSlotResponse
                {
                    Id = slot.Id,
                    DayOfWeek = slot.DayOfWeek,
                    SpecialDate = slot.SpecialDate,
                    OpenTime = slot.OpenTime,
                    CloseTime = slot.CloseTime,
                    Shift = slot.Shift,
                    IsAbnormal = slot.IsAbnormal,
                    ScheduleBeforeShiftMinutes = shiftConfig
                }*/
                ).ToListAsync();
            return timeSlots;
        }
    }
}
