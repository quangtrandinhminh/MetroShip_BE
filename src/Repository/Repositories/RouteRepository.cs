using FluentValidation;
using MetroShip.Repository.Base;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Twilio.Jwt.AccessToken;
using AppDbContext = MetroShip.Repository.Infrastructure.AppDbContext;

namespace MetroShip.Repository.Repositories;

public class RouteRepository : BaseRepository<Route>, IRouteRepository
{
    private readonly AppDbContext _context;
    public RouteRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }
}