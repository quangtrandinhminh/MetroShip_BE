using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels.StaffAssignment;
using MetroShip.Service.Interfaces;
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

    public async Task<int> AssignAsync(StaffAssignmentRequest request)
    { 
        var staff = await _userRepository.GetUserByIdAsync(request.StaffId);
        if (staff is null)
        {
            throw new ArgumentException($"Staff with ID {request.StaffId} does not exist.");
        }

        var station = await _stationRepository.GetSingleAsync(x => x.Id == request.StationId && x.IsActive);
        if (station is null)
        {
            throw new ArgumentException($"Station with ID {request.StationId} does not exist.");
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

        _staffAssignmentRepository.AddAsync(staffAssignment);
        return await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
    }
}