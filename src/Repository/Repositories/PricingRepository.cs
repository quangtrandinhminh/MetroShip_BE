using MetroShip.Repository.Base;
using AppDbContext = MetroShip.Repository.Infrastructure.AppDbContext;

namespace MetroShip.Repository.Repositories;

public class PricingRepository : BaseRepository<Models.PricingConfig>, Interfaces.IPricingRepository
{
    public PricingRepository(AppDbContext context) : base(context)
    {
    }
}