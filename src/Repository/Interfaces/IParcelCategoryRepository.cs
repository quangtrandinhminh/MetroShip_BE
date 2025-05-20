using MetroShip.Repository.Extensions;
using MetroShip.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Repository.Interfaces
{
    public interface IParcelCategoryRepository
    {
        Task<int> SaveChangesAsync();
        Task<ParcelCategory?> GetByIdAsync(Guid id, params Expression<Func<ParcelCategory, object>>[] includeProperties);
        IQueryable<ParcelCategory> GetAllWithCondition(Expression<Func<ParcelCategory, bool>>? predicate = null, params Expression<Func<ParcelCategory, object>>[] includeProperties);
        Task<PaginatedList<ParcelCategory>> GetAllPaginatedQueryable(int pageNumber, int pageSize, Expression<Func<ParcelCategory, bool>>? predicate = null, Expression<Func<ParcelCategory, object>>? orderBy = null, params Expression<Func<ParcelCategory, object>>[]? includeProperties);
        Task<ParcelCategory?> GetSingleAsync(Expression<Func<ParcelCategory, bool>>? predicate = null, params Expression<Func<ParcelCategory, object>>[] includeProperties);
        IQueryable<ParcelCategory> Get(Expression<Func<ParcelCategory, bool>>? predicate = null, params Expression<Func<ParcelCategory, object>>[] includeProperties);
        Task CreateAsync(ParcelCategory category);
        Task UpdateAsync(ParcelCategory category);
        Task DeleteAsync(ParcelCategory category);
    }
}
