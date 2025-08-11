using MetroShip.Repository.Base;
using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Repository.Repositories
{
    public class ShipmentTrackingRepository : BaseRepository<ShipmentTracking>, IShipmentTrackingRepository
    {
        private readonly AppDbContext _context;
        public ShipmentTrackingRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

    }
}
