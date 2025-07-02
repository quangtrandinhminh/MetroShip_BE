using MetroShip.Repository.Base;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using MetroShip.Utility.Enums;
using Microsoft.EntityFrameworkCore;
using AppDbContext = MetroShip.Repository.Infrastructure.AppDbContext;

namespace MetroShip.Repository.Repositories;

public class MetroTimeSlotRepository : BaseRepository<MetroTimeSlot>, IMetroTimeSlotRepository
{
    private readonly AppDbContext _context;
    public MetroTimeSlotRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    /*public async Task<MetroTimeSlot> GetPreviousTimeSlot(
               ShiftEnum currentShift,
               string lineId, string trainId, DateTimeOffset date,
                      CancellationToken cancellationToken = default)
    {
        var previousShift = (currentShift == ShiftEnum.Morning) ? ShiftEnum.Night : currentShift - 1;

        return await _context.MetroTimeSlots
            .Where(ts => ts.Shift == previousShift &&
                                   
            .OrderByDescending(ts => ts.OpenTime)
            .FirstOrDefaultAsync(cancellationToken);
    }*/
}