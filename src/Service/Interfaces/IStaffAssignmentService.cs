using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels.StaffAssignment;

namespace MetroShip.Service.Interfaces;

public interface IStaffAssignmentService
{
    Task<StaffAssignment?> GetByStaffIdAsync(string staffId);
    Task<string> AssignAsync(StaffAssignmentRequest request);
    Task<string> ActivateAssignment(string assignmentId);
    Task<string> DeactivateAssignment(string assignmentId);
    Task<string> DeactivateAssignmentByStaffId(string staffId);
}