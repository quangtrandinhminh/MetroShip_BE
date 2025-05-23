using MetroShip.Repository.Base;
using MetroShip.Repository.Extensions;
using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Repository.Repositories
{
    public class ParcelCategoryRepository : BaseRepository<ParcelCategory>,IParcelCategoryRepository
    {
        private readonly AppDbContext _context;

        public ParcelCategoryRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
