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
}