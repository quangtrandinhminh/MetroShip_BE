using MetroShip.Repository.Base;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using AppDbContext = MetroShip.Repository.Infrastructure.AppDbContext;

namespace MetroShip.Repository.Repositories;

public class InsuranceRepository : BaseRepository<InsurancePolicy>, IInsuranceRepository
{
    public InsuranceRepository(AppDbContext context) : base(context)
    {
    }
}