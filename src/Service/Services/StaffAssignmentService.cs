using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels.StaffAssignment;
using MetroShip.Service.Interfaces;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Exceptions;
using MetroShip.Utility.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace MetroShip.Service.Services;

public class StaffAssignmentService(IServiceProvider serviceProvider) : IStaffAssignmentService
{
    private readonly IStaffAssignmentRepository _staffAssignmentRepository = serviceProvider.GetRequiredService<IStaffAssignmentRepository>();
    private readonly IUserRepository _userRepository = serviceProvider.GetRequiredService<IUserRepository>();
    private readonly IStationRepository _stationRepository = serviceProvider.GetRequiredService<IStationRepository>();
    private readonly IHttpContextAccessor _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    private readonly IUnitOfWork _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();

    public Task<StaffAssignment?> GetByStaffIdAsync(string staffId)
    {
        return _staffAssignmentRepository.GetSingleAsync(
                       x => x.StaffId == staffId && x.IsActive);
    }

    public async Task<string> AssignAsync(StaffAssignmentRequest request)
    { 
        var staff = await _userRepository.GetUserByIdAsync(request.StaffId);
        if (staff is null)
        {
            throw new AppException(
                ErrorCode.BadRequest,
                ResponseMessageConstantsUser.USER_NOT_FOUND,
                StatusCodes.Status400BadRequest);
        }

        var station = await _stationRepository.GetSingleAsync(x => x.Id == request.StationId && x.IsActive);
        if (station is null)
        {
            throw new AppException(
                ErrorCode.BadRequest,
                ResponseMessageStation.STATION_NOT_FOUND,
                StatusCodes.Status400BadRequest);
        }

        // Check if there is an existing active assignment for the same staff
        // CURRENT: only 1 assignment per staff is allowed
        var existingAssignment = _staffAssignmentRepository.GetAllWithCondition(
                       x => x.StaffId == request.StaffId && x.IsActive);

        if (existingAssignment.Any())
        {
            foreach (var assignment in existingAssignment)
            {
                // Deactivate all existing assignments
                assignment.IsActive = false;
                assignment.ToTime = CoreHelper.SystemTimeNow;
                _staffAssignmentRepository.Update(assignment);
            }
        }    

        var staffAssignment = new StaffAssignment
        {
            StaffId = request.StaffId,
            StationId = request.StationId,
            AssignedRole = request.AssignedRole,
            FromTime = request.FromTime,
            ToTime = request.ToTime,
            Description = request.Description,
            IsActive = true
        };

        _staffAssignmentRepository.Add(staffAssignment);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
        return ResponseMessageAssignment.ASSIGNMENT_CREATE_SUCCESS;
    }

    public async Task<string> ActivateAssignment(string assignmentId)
    {
        var assignment = await _staffAssignmentRepository.GetByIdAsync(assignmentId);
        if (assignment == null)
        {
            throw new AppException
            (
                ErrorCode.BadRequest,
                ResponseMessageAssignment.ASSIGNMENT_NOT_FOUND,
                StatusCodes.Status400BadRequest
            );
        }

        // Deactivate other assignments for the same staff
        var existingAssignments = _staffAssignmentRepository.GetAllWithCondition(
                       x => x.StaffId == assignment.StaffId && x.IsActive && x.Id != assignmentId);

        foreach (var existing in existingAssignments)
        {
            existing.IsActive = false;
            existing.ToTime = CoreHelper.SystemTimeNow;
            _staffAssignmentRepository.Update(existing);
        }

        // Activate the specified assignment
        assignment.IsActive = true;
        assignment.FromTime = CoreHelper.SystemTimeNow;
        assignment.ToTime = null; // Clear ToTime when activating
        _staffAssignmentRepository.Update(assignment);

        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
        return assignment.Id;
    }

    public async Task<string> DeactivateAssignment(string assignmentId)
    {
        var assignment = await _staffAssignmentRepository.GetByIdAsync(assignmentId);
        if (assignment == null)
        {
            throw new AppException
            (
                ErrorCode.BadRequest,
                ResponseMessageAssignment.ASSIGNMENT_NOT_FOUND,
                StatusCodes.Status400BadRequest
            );
        }

        if (!assignment.IsActive)
        {
            throw new AppException
            (
                ErrorCode.BadRequest,
                ResponseMessageAssignment.ASSIGNMENT_ALREADY_INACTIVE,
                StatusCodes.Status400BadRequest
            );
        }

        assignment.IsActive = false;
        assignment.ToTime = CoreHelper.SystemTimeNow;
        _staffAssignmentRepository.Update(assignment);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);

        return ResponseMessageAssignment.ASSIGNMENT_DEACTIVATE_SUCCESS;
    }

    public async Task<string> DeactivateAssignmentByStaffId(string staffId)
    {
        var staff = await _userRepository.GetUserByIdAsync(staffId);
        if (staff is null)
        {
            throw new AppException(
                ErrorCode.BadRequest,
                ResponseMessageConstantsUser.USER_NOT_FOUND,
                StatusCodes.Status400BadRequest);
        }

        var existingAssignments = _staffAssignmentRepository.GetAllWithCondition(
                                  x => x.StaffId == staffId && x.IsActive);

        if (!existingAssignments.Any())
        {
            throw new AppException
            (
                ErrorCode.BadRequest,
                ResponseMessageAssignment.ASSIGNMENT_NOT_FOUND,
                StatusCodes.Status400BadRequest
            );
        }

        foreach (var assignment in existingAssignments)
        {
            assignment.IsActive = false;
            assignment.ToTime = CoreHelper.SystemTimeNow;
            _staffAssignmentRepository.Update(assignment);
        }

        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
        return ResponseMessageAssignment.ASSIGNMENT_DEACTIVATE_SUCCESS;
    }
}