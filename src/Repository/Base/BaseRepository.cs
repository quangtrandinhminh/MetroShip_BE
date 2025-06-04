using System.Linq.Expressions;
using MetroShip.Repository.Extensions;
using MetroShip.Repository.Models.Base;
using MetroShip.Utility.Helpers;
using Microsoft.EntityFrameworkCore;
using AppDbContext = MetroShip.Repository.Infrastructure.AppDbContext;

namespace MetroShip.Repository.Base
{
    public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity, new()
    {
        private readonly AppDbContext _context;

        public BaseRepository(AppDbContext context)
        {
            _context = context;
        }

        private DbSet<T> _dbSet;

        private DbSet<T> DbSet
        {
            get
            {
                if (_dbSet != null)
                {
                    return _dbSet;
                }

                _dbSet = _context.Set<T>();
                return _dbSet;
            }
        }

        public IQueryable<T> Set() => DbSet.AsNoTracking();

        public virtual void RefreshEntity(T entity)
        {
            _context.Entry(entity).Reload();
        }

        public IQueryable<T?> GetAll()
        {
            return DbSet.AsQueryable().AsNoTracking();
        }

        // get all with paging, return paginated queryable and all page count
        // can only select the columns that are needed in the response
        public async Task<PaginatedList<T>> GetAllPaginatedQueryable(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>> predicate = null,
            Expression<Func<T, object>>? orderBy = null,
            bool isAscending = false,
            params Expression<Func<T, object>>[]? includeProperties
            )
        {
            IQueryable<T> queryable = DbSet.AsNoTracking();
            includeProperties = includeProperties?.Distinct().ToArray();

            // Include related entities
            if (includeProperties?.Any() ?? false)
            {
                foreach (var navigationPropertyPath in includeProperties)
                {
                    queryable = queryable.Include(navigationPropertyPath);
                }
            }

            // Apply the predicate
            queryable = predicate != null ? queryable.Where(predicate) : queryable;
            queryable = orderBy != null ? queryable.OrderByDescending(orderBy) : queryable.OrderByDescending(p => p.CreatedAt);
            // Apply ascending order if specified
            if (isAscending)
            {
                queryable = queryable.OrderBy(orderBy);
            }

            // Create the paginated list with the projected query
            return await PaginatedList<T>.CreateAsync(queryable, pageNumber, pageSize);
        }

        public IQueryable<T> GetAllWithCondition(Expression<Func<T, bool>> predicate = null,
            params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> queryable = DbSet.AsNoTracking();
            includeProperties = includeProperties?.Distinct().ToArray();
            if (includeProperties?.Any() ?? false)
            {
                Expression<Func<T, object>>[] array = includeProperties;
                foreach (Expression<Func<T, object>> navigationPropertyPath in array)
                {
                    queryable = queryable.Include(navigationPropertyPath);
                }

                queryable = queryable.AsSplitQuery();
            }

            return predicate != null ? queryable.Where(predicate) : queryable;
        }

        public async Task<IList<T>?> GetAllAsync()
        {
            return await DbSet.AsQueryable().AsNoTracking().ToListAsync();
        }

        // check if an entity exists
        public async Task<bool> IsExistAsync(Expression<Func<T, bool>> predicate)
        {
            return await DbSet.AnyAsync(predicate);
        }

        public IQueryable<T> Get(Expression<Func<T, bool>> predicate = null
            , bool isIncludeDeleted = false, params Expression<Func<T, object>>[] includeProperties)
        {

            IQueryable<T> source = DbSet.AsNoTracking();
            if (predicate != null)
            {
                source = source.Where(predicate);
            }

            includeProperties = includeProperties?.Distinct().ToArray();
            if (includeProperties?.Any() ?? false)
            {
                Expression<Func<T, object>>[] array = includeProperties;
                foreach (Expression<Func<T, object>> navigationPropertyPath in array)
                {
                    source = source.Include(navigationPropertyPath);
                }
            }

            return isIncludeDeleted ? source.IgnoreQueryFilters() : source.Where((x) => x.DeletedAt == null);
        }

        public T? GetById(object id)
        {
            return DbSet.Find(id);
        }

        public async Task<T?> GetByIdAsync(object id)
        {
            return await DbSet.FindAsync(id);
        }

        /*public  async Task<T> GetSingleAsync(Expression<Func<T, bool>> predicate, 
            bool isIncludeDeleted = false, params Expression<Func<T, object>>[] includeProperties)
        {
            return await Get(predicate, isIncludeDeleted, includeProperties)
                .OrderByDescending(p => p.CreatedTime).FirstOrDefaultAsync();
        }*/

        public async Task<T> GetSingleAsync(Expression<Func<T, bool>> predicate,
            bool isIncludeDeleted = false,
            params Expression<Func<T, object>>[] includeProperties)
        {

            var query = DbSet.AsQueryable();

            if (!isIncludeDeleted)
            {
                query = query.Where(e => e.DeletedAt == null);
            }

            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties)
                {
                    query = query.Include(includeProperty);
                }

                query = query.AsSplitQuery();
            }
            return await query.FirstOrDefaultAsync(predicate);
        }

        public T Add(T entity)
        {
            entity = DbSet.Add(entity).Entity;
            return entity;
        }

        public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            var result = await DbSet.AddAsync(entity);
            return result.Entity;
        }

        public void AddRange(IEnumerable<T> entities)
        {
            _context.AddRange(entities);
        }

        public async Task AddRangeAsync(IEnumerable<T?> entities)
        {
            await DbSet.AddRangeAsync(entities);
            foreach (var entity in entities)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
        }

        public void Update(T entity)
        {
            TryAttach(entity);

            entity.LastUpdatedAt = ObjHelper.ReplaceNullOrDefault(entity.LastUpdatedAt, DateTimeOffset.UtcNow);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void Delete(T entity)
        {
            try
            {
                TryAttach(entity);
                DbSet.Remove(entity);
            }
            catch (Exception)
            {
                RefreshEntity(entity);
                throw;
            }
        }

        public void DeleteRange(ICollection<T> entities)
        {
            try
            {
                TryAttachRange(entities);
                DbSet.RemoveRange(entities);
            }
            catch
            {

            }
        }

        public IQueryable<T?> FindByCondition(Expression<Func<T?, bool>> expression)
        {
            return DbSet.Where(expression).AsQueryable().AsNoTracking();
        }

        public async Task<IList<T?>> FindByConditionAsync(Expression<Func<T?, bool>> expression)
        {
            return await DbSet.Where(expression).AsQueryable().AsNoTracking().ToListAsync();
        }

        public async Task<T?> GetSingleAsync(Expression<Func<T, bool>>? predicate = null,
            params Expression<Func<T, object>>[] includeProperties)
            => await Get(predicate, includeProperties).FirstOrDefaultAsync();

        public IQueryable<T> Get(Expression<Func<T, bool>>? predicate = null, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> reault = DbSet.AsNoTracking();
            if (predicate != null)
            {
                reault = reault.Where(predicate);
            }

            includeProperties = includeProperties?.Distinct().ToArray();
            if (includeProperties?.Any() ?? false)
            {
                Expression<Func<T, object>>[] array = includeProperties;
                foreach (Expression<Func<T, object>> navigationPropertyPath in array)
                {
                    reault = reault.Include(navigationPropertyPath);
                }
            }

            return reault.Where(x => x.DeletedAt == null);
        }

        public void TryAttach(T entity)
        {
            try
            {


                if (_context.Entry(entity).State == EntityState.Detached)
                {
                    DbSet.Attach(entity);
                }
            }
            catch
            {
            }
        }

        public void TryAttachRange(ICollection<T> entities)
        {
            try
            {
                foreach (var entity in entities)
                {
                    if (_context.Entry(entity).State != EntityState.Detached)
                    {
                        entities.Remove(entity);
                    }
                }
                DbSet.AttachRange(entities);
            }
            catch
            {
            }
        }

        public async Task<T?> FindAsync(Expression<Func<T, bool>> predicate)
        {


            return await DbSet.FirstOrDefaultAsync(predicate);
        }

        // save changes
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }
    }
}
