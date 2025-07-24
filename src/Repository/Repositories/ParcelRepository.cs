using MetroShip.Repository.Base;
using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Repository.Repositories
{
    public class ParcelRepository : BaseRepository<Parcel>, IParcelRepository
    {
        private readonly AppDbContext _context;

        public ParcelRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Parcel>> GetByIdsAsync(IEnumerable<string> parcelIds)
        {
            return await _context.Parcels
                .Where(p => parcelIds.Contains(p.Id))
                .ToListAsync();
        }
    }
}
