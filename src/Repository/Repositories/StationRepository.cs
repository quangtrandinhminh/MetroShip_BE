using MetroShip.Repository.Base;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using AppDbContext = MetroShip.Repository.Infrastructure.AppDbContext;

namespace MetroShip.Repository.Repositories;

public class StationRepository : BaseRepository<Station>, IStationRepository
{
    public StationRepository(AppDbContext context) : base(context)
    {
    }
}