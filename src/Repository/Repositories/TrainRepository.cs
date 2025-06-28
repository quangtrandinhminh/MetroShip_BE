using System.Linq.Expressions;
using MetroShip.Repository.Base;
using MetroShip.Repository.Extensions;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using Microsoft.EntityFrameworkCore;
using AppDbContext = MetroShip.Repository.Infrastructure.AppDbContext;

namespace MetroShip.Repository.Repositories;

public class TrainRepository : BaseRepository<MetroTrain>, ITrainRepository
{
    private readonly AppDbContext _context;

    public TrainRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }
}