using MetroShip.Repository.Base;
using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using Microsoft.EntityFrameworkCore;

namespace MetroShip.Repository.Repositories;

public class MetroLineRepository : BaseRepository<MetroLine>, IMetroLineRepository
{
    private readonly AppDbContext _context;

    public MetroLineRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    // get base price by line id & schedule time
    public async Task<MetroBasePrice> GetBasePriceByLineIdAndTimeSlotAsync(string lineId, string timeSlotId)
    {
        return await _context.MetroBasePrices
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.LineId == lineId && x.TimeSlotId == timeSlotId);
    }

    public async Task<IEnumerable<MetroLine>> GetAllWithBasePriceByRegionAsync(string? regionId, string? regionCode)
    {
        var query = _context.MetroLines
            .Include(x => x.BasePriceVndPerKm)
            .Include(x => x.Region) // assuming navigation to Region
            .AsQueryable();

        if (!string.IsNullOrEmpty(regionId) && Guid.TryParse(regionId, out var guidRegionId))
        {
            query = query.Where(x => x.RegionId == guidRegionId.ToString());
        }
        else if (!string.IsNullOrEmpty(regionCode))
        {
            query = query.Where(x => x.RegionId == regionCode);
        }

        return await query.AsNoTracking().ToListAsync();
    }
}