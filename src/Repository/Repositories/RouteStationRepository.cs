using System.Collections;
using FluentValidation;
using MetroShip.Repository.Base;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Twilio.Jwt.AccessToken;
using AppDbContext = MetroShip.Repository.Infrastructure.AppDbContext;
using System.Linq.Expressions;

namespace MetroShip.Repository.Repositories;

public class RouteStationRepository : BaseRepository<Route>, IRouteStationRepository
{
    private readonly AppDbContext _context;
    public RouteStationRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    /*public class RoutesForGraph : Route
    {
        public decimal MetroBasePriceVndPerKm { get; set; }
    }

    public async Task<IDictionary<string, RoutesForGraph>>
        GetRouteListByLineIdAsync(Expression<Func<Route, bool>> predicate = null)
    {
        var query = _context.Routes
            .AsNoTracking();

        if (predicate is not null)
            query = query.Where(predicate);

        var routes = await query
            .Select(x => new RoutesForGraph
            {
                Id = x.Id,
                FromStationId = x.FromStationId,
                ToStationId = x.ToStationId,
                SeqOrder = x.SeqOrder,
                TravelTimeMin = x.TravelTimeMin,
                Direction = x.Direction,
                LengthKm = x.LengthKm,
                MetroBasePriceVndPerKm = x.MetroLine.BasePriceVndPerKm,
            })
            .ToListAsync();

        var result = new Dictionary<string, RoutesForGraph>();
        foreach (var route in routes)
        {
            if (!result.ContainsKey(route.Id))
            {
                result[route.Id] = route;
            }
        }

        return result;
    }*/
}