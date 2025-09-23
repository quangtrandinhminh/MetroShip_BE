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
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MetroShip.Utility.Config;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Utility.Exceptions;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Enums;
using Microsoft.AspNetCore.Http;

namespace MetroShip.Service.Services
{
    public class MetroTimeSlotService(IServiceProvider serviceProvider) : IMetroTimeSlotService
    {
        private readonly IMetroTimeSlotRepository _metroTimeSlotRepository = serviceProvider.GetRequiredService<IMetroTimeSlotRepository>();
        private readonly IMapperlyMapper _mapper = serviceProvider.GetRequiredService<IMapperlyMapper>();
        private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
        private readonly IUnitOfWork _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
        private readonly IHttpContextAccessor _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();

        public async Task<IList<MetroTimeSlotResponse>> GetAllForMetroTimeSlot()
        {
            _logger.Information("Getting all MetroTimeSlots for dropdown.");

            var timeSlots = await _metroTimeSlotRepository.GetAll()
                .Where(s => s.DeletedAt == null)
                .OrderBy(s => s.OpenTime)
                .Select(slot => //_mapper.MapToMetroTimeSlotResponse(slot) 
                new MetroTimeSlotResponse
                {
                    Id = slot.Id,
                    DayOfWeek = slot.DayOfWeek,
                    SpecialDate = slot.SpecialDate,
                    OpenTime = slot.OpenTime,
                    CloseTime = slot.CloseTime,
                    Shift = slot.Shift,
                    IsAbnormal = slot.IsAbnormal,
                    StartReceivingTime = slot.StartReceivingTime,
                    CutOffTime = slot.CutOffTime,
                }
                ).ToListAsync();
            return timeSlots;
        }

        public async Task<PaginatedListResponse<MetroTimeSlotResponse>> GetAllMetroTimeSlot (PaginatedListRequest request)
        {
            _logger.Information("Getting all MetroTimeSlots with pagination.");

            Expression<Func<MetroTimeSlot, bool>> query = slot => slot.DeletedAt == null;

            var slots = await _metroTimeSlotRepository.GetAllPaginatedQueryable
                ( request.PageNumber, request.PageSize, query, x => x.Shift, true);

            return _mapper.MapToPaginatedListResponse(slots);
        }

        public async Task<string> UpdateMetroTimeSlot(MetroTimeSlotUpdateRequest request)
        {
            _logger.Information("Updating MetroTimeSlot with ID: {Id}", request.Id);

            var existingTimeSlot = await _metroTimeSlotRepository.GetAll()
                .FirstOrDefaultAsync(s => s.Id == request.Id && s.DeletedAt == null);

            if (existingTimeSlot == null)
            {
                throw new AppException(
                    ErrorCode.BadRequest,
                    ResponseMessageTimeSlot.TIMESLOT_NOT_FOUND,
                    StatusCodes.Status400BadRequest);
            }

            // Validate time slot times
            ValidateTimeSlotTimes(request, existingTimeSlot.Shift);

            // Check for overlapping time slots
            await ValidateTimeSlotOverlap(request, existingTimeSlot);

            _mapper.MapToMetroTimeSlotEntity(request, existingTimeSlot);
            _metroTimeSlotRepository.Update(existingTimeSlot);
            await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
            return ResponseMessageTimeSlot.TIMESLOT_UPDATE_SUCCESS;
        }

        private void ValidateTimeSlotTimes(MetroTimeSlotUpdateRequest request, ShiftEnum shift)
        {
            // For non-night shifts: OpenTime must be before CloseTime
            if (shift != ShiftEnum.Night && request.OpenTime >= request.CloseTime)
            {
                throw new AppException(
                    ErrorCode.BadRequest,
                    ResponseMessageTimeSlot.OPEN_TIME_MUST_BE_BEFORE_CLOSE_TIME,
                    StatusCodes.Status400BadRequest);
            }

            // For night shifts: Allow OpenTime > CloseTime (crossing midnight)
            // But validate that it makes sense (e.g., OpenTime should be evening/night hours)
            if (shift == ShiftEnum.Night)
            {
                // Optional: Add validation for reasonable night shift hours
                // e.g., OpenTime should be after 18:00 or CloseTime should be before 12:00
                if (request.OpenTime.Hour < 18 && request.CloseTime.Hour > 6)
                {
                    throw new AppException(
                        ErrorCode.BadRequest,
                        ResponseMessageTimeSlot.INVALID_NIGHT_SHIFT_TIMES,
                        StatusCodes.Status400BadRequest);
                }
            }

            // Validate optional times only if they are provided
            if (request.StartReceivingTime >= request.CutOffTime)
            {
                throw new AppException(
                    ErrorCode.BadRequest,
                    ResponseMessageTimeSlot.START_TIME_MUST_BE_BEFORE_CUT_OFF_TIME,
                    StatusCodes.Status400BadRequest);
            }

            if (request.StartReceivingTime >= request.OpenTime)
            {
                throw new AppException(
                    ErrorCode.BadRequest,
                    ResponseMessageTimeSlot.START_TIME_MUST_BE_BEFORE_OPEN_TIME,
                    StatusCodes.Status400BadRequest);
            }

            if (request.CutOffTime >= request.OpenTime)
            {
                throw new AppException(
                    ErrorCode.BadRequest,
                    ResponseMessageTimeSlot.CUT_OFF_TIME_MUST_BE_BEFORE_OPEN_TIME,
                    StatusCodes.Status400BadRequest);
            }
        }

        private async Task ValidateTimeSlotOverlap(MetroTimeSlotUpdateRequest request, MetroTimeSlot existingTimeSlot)
        {
            // Only normal time slots (IsAbnormal = false) cannot overlap
            if (existingTimeSlot.IsAbnormal)
            {
                return; // Abnormal slots can overlap
            }

            var allActiveTimeSlots = await _metroTimeSlotRepository.GetAll()
                .Where(s => s.Id != request.Id && s.DeletedAt == null && !s.IsAbnormal)
                .ToListAsync();

            foreach (var timeSlot in allActiveTimeSlots)
            {
                bool isOverlapping = false;

                if (existingTimeSlot.Shift == ShiftEnum.Night)
                {
                    // Night shift logic (can cross midnight)
                    isOverlapping = CheckNightShiftOverlap(request, timeSlot);
                }
                else
                {
                    // Regular shift logic (same day)
                    isOverlapping = CheckRegularShiftOverlap(request, timeSlot);
                }

                if (isOverlapping)
                {
                    throw new AppException(
                        ErrorCode.BadRequest,
                        ResponseMessageTimeSlot.OVERLAPPING_TIMESLOT,
                        StatusCodes.Status400BadRequest);
                }
            }
        }

        private bool CheckRegularShiftOverlap(MetroTimeSlotUpdateRequest request, MetroTimeSlot existingSlot)
        {
            // Standard overlap check for same-day shifts
            return request.OpenTime < existingSlot.CloseTime && request.CloseTime > existingSlot.OpenTime;
        }

        private bool CheckNightShiftOverlap(MetroTimeSlotUpdateRequest request, MetroTimeSlot existingSlot)
        {
            bool requestCrossesMidnight = request.OpenTime > request.CloseTime;
            bool existingCrossesMidnight = existingSlot.Shift == ShiftEnum.Night && existingSlot.OpenTime > existingSlot.CloseTime;

            switch (requestCrossesMidnight)
            {
                case true when existingCrossesMidnight:
                    // Both cross midnight - they overlap if either part overlaps
                    return (request.OpenTime < existingSlot.CloseTime.AddHours(24) && request.CloseTime.AddHours(24) > existingSlot.OpenTime) ||
                           (request.OpenTime < existingSlot.CloseTime && request.CloseTime.AddHours(24) > existingSlot.OpenTime);
                case true when !existingCrossesMidnight:
                    // Request crosses midnight, existing doesn't
                    // Check if request's first part (same day) overlaps with existing
                    // or if request's second part (next day) overlaps with existing
                    return (request.OpenTime < existingSlot.CloseTime && TimeOnly.MaxValue > existingSlot.OpenTime) ||
                           (TimeOnly.MinValue < existingSlot.CloseTime && request.CloseTime > existingSlot.OpenTime);
                case false when existingCrossesMidnight:
                    // Request doesn't cross midnight, existing does
                    // Check if request overlaps with either part of existing slot
                    return (request.OpenTime < existingSlot.CloseTime && request.CloseTime > TimeOnly.MinValue) ||
                           (request.OpenTime < TimeOnly.MaxValue && request.CloseTime > existingSlot.OpenTime);
                default:
                    // Neither crosses midnight - standard overlap check
                    return request.OpenTime < existingSlot.CloseTime && request.CloseTime > existingSlot.OpenTime;
            }
        }
    }
}
