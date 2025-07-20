using MetroShip.Repository.Base;
using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;

namespace MetroShip.Repository.Repositories;

public class StaffAssignmentRepository : BaseRepository<StaffAssignment>, IStaffAssignmentRepository
{
    public StaffAssignmentRepository(AppDbContext context) : base(context)
    {
    }
}