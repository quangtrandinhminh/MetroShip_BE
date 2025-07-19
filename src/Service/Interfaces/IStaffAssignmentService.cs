using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels.StaffAssignment;

namespace MetroShip.Service.Interfaces;

public interface IStaffAssignmentService
{
    Task<StaffAssignment?> GetByStaffIdAsync(string staffId);
    Task<int> AssignAsync(StaffAssignmentRequest request);
}