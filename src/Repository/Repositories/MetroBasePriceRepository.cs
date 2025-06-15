using MetroShip.Repository.Base;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using AppDbContext = MetroShip.Repository.Infrastructure.AppDbContext;

namespace MetroShip.Repository.Repositories;

public class MetroBasePriceRepository : BaseRepository<MetroBasePrice>, IMetroBasePriceRepository
{
    public MetroBasePriceRepository(AppDbContext context) : base(context)
    {
    }
}