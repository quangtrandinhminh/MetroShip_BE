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
    public class ParcelCategoryRepository : IParcelCategoryRepository
    {
        private readonly AppDbContext _context;

        public ParcelCategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public async Task<ParcelCategory?> GetByIdAsync(Guid id,
            params Expression<Func<ParcelCategory, object>>[] includeProperties)
        {
            return await Get(pc => Guid.Parse(pc.Id) == id, includeProperties).FirstOrDefaultAsync();
        }

        public IQueryable<ParcelCategory> GetAllWithCondition(
            Expression<Func<ParcelCategory, bool>>? predicate = null,
            params Expression<Func<ParcelCategory, object>>[] includeProperties)
        {
            IQueryable<ParcelCategory> query = _context.ParcelCategories.AsNoTracking();

            if (includeProperties?.Any() ?? false)
            {
                foreach (var include in includeProperties.Distinct())
                {
                    query = query.Include(include);
                }
            }

            return predicate == null ? query : query.Where(predicate);
        }

        public async Task<PaginatedList<ParcelCategory>> GetAllPaginatedQueryable(
            int pageNumber,
            int pageSize,
            Expression<Func<ParcelCategory, bool>>? predicate = null,
            Expression<Func<ParcelCategory, object>>? orderBy = null,
            params Expression<Func<ParcelCategory, object>>[]? includeProperties)
        {
            IQueryable<ParcelCategory> query = _context.ParcelCategories.AsNoTracking();

            if (includeProperties?.Any() ?? false)
            {
                foreach (var include in includeProperties.Distinct())
                {
                    query = query.Include(include);
                }
            }

            query = predicate != null ? query.Where(predicate) : query;
            query = orderBy != null ? query.OrderBy(orderBy) : query.OrderByDescending(x => x.CreatedAt);

            return await PaginatedList<ParcelCategory>.CreateAsync(query, pageNumber, pageSize);
        }

        public async Task<ParcelCategory?> GetSingleAsync(
            Expression<Func<ParcelCategory, bool>>? predicate = null,
            params Expression<Func<ParcelCategory, object>>[] includeProperties)
        {
            return await Get(predicate, includeProperties).FirstOrDefaultAsync();
        }

        public IQueryable<ParcelCategory> Get(
            Expression<Func<ParcelCategory, bool>>? predicate = null,
            params Expression<Func<ParcelCategory, object>>[] includeProperties)
        {
            IQueryable<ParcelCategory> query = _context.ParcelCategories.AsNoTracking();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (includeProperties?.Any() ?? false)
            {
                foreach (var include in includeProperties.Distinct())
                {
                    query = query.Include(include);
                }
            }

            return query.Where(x => x.DeletedAt == null); // kế thừa từ BaseEntity
        }

        public async Task CreateAsync(ParcelCategory category)
        {
            await _context.ParcelCategories.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ParcelCategory category)
        {
            _context.ParcelCategories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ParcelCategory category)
        {
            category.DeletedAt = DateTime.UtcNow;
            _context.ParcelCategories.Update(category);
            await _context.SaveChangesAsync();
        }
    }
}
