using System.Linq.Expressions;
using MetroShip.Repository.Extensions;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AppDbContext = MetroShip.Repository.Infrastructure.AppDbContext;

namespace MetroShip.Repository.Repositories
{
    public class UserRepository : UserStore<UserEntity, RoleEntity, AppDbContext>, IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<int> SaveChangeAsync() => await Context.SaveChangesAsync();

        public IQueryable<UserEntity> GetAllWithCondition(Expression<Func<UserEntity, bool>> predicate = null,
            params Expression<Func<UserEntity, object>>[] includeProperties)
        {
            var dbSet = _context.Set<UserEntity>();
            IQueryable<UserEntity> queryable = dbSet.AsNoTracking();
            includeProperties = includeProperties?.Distinct().ToArray();
            if (includeProperties?.Any() ?? false)
            {
                Expression<Func<UserEntity, object>>[] array = includeProperties;
                foreach (Expression<Func<UserEntity, object>> navigationPropertyPath in array)
                {
                    queryable = queryable.Include(navigationPropertyPath);
                }
            }

            return predicate == null ? queryable : queryable.Where(predicate);
        }

        public async Task<PaginatedList<UserEntity>> GetAllPaginatedQueryable(
            int pageNumber,
            int pageSize,
            Expression<Func<UserEntity, bool>> predicate = null,
            Expression<Func<UserEntity, object>>? orderBy = null,
            params Expression<Func<UserEntity, object>>[]? includeProperties
        )
        {
            IQueryable<UserEntity> queryable = _context.Set<UserEntity>().AsNoTracking();
            includeProperties = includeProperties?.Distinct().ToArray();

            // Include related entities
            if (includeProperties?.Any() ?? false)
            {
                foreach (var navigationPropertyPath in includeProperties)
                {
                    // if x => x.UserRoles, then include Roles
                    if (navigationPropertyPath.Body is MemberExpression memberExpression &&
                        memberExpression.Member.Name == "UserRoles")
                    {
                        queryable = queryable.Include("UserRoles.Role");
                    }
                    else
                    {
                        // For other navigation properties, use the provided expression
                        queryable = queryable.Include(navigationPropertyPath);
                    }
                }
            }

            // Apply the predicate
            queryable = predicate != null ? queryable.Where(predicate) : queryable;
            queryable = orderBy != null ? queryable.OrderBy(orderBy) : queryable.OrderByDescending(p => p.CreatedTime);

            // Create the paginated list with the projected query
            var paginatedList = await PaginatedList<UserEntity>.CreateAsync(queryable, pageNumber, pageSize);
            return paginatedList;
        }

        public async Task<IdentityResult> CreateUserAsync(UserEntity user, CancellationToken cancellationToken = default)
        {
            await _context.Users.AddAsync(user, cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(UserEntity user)
        {
            _context.Users.Update(user);
            return IdentityResult.Success;
        }

        public async Task<UserEntity?> GetSingleAsync(Expression<Func<UserEntity, bool>>? predicate = null,
            params Expression<Func<UserEntity, object>>[] includeProperties)
        => await Get(predicate, includeProperties).FirstOrDefaultAsync();

        public IQueryable<UserEntity> Get(Expression<Func<UserEntity, bool>>? predicate = null, params Expression<Func<UserEntity, object>>[] includeProperties)
        {
            IQueryable<UserEntity> reault = _context.Users.AsNoTracking();
            if (predicate != null)
            {
                reault = reault.Where(predicate);
            }

            includeProperties = includeProperties?.Distinct().ToArray();
            if (includeProperties?.Any() ?? false)
            {
                Expression<Func<UserEntity, object>>[] array = includeProperties;
                foreach (Expression<Func<UserEntity, object>> navigationPropertyPath in array)
                {
                    reault = reault.Include(navigationPropertyPath);
                }
            }

            return reault.Where(x => x.DeletedTime == null);
        }

        public async Task<UserEntity?> GetUserByIdAsync(object userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task AddUserToRoleAsync(string userId, List<string> lisRoleId, CancellationToken cancellationToken = default)
        {
            var userRoles = new List<UserRoleEntity>();
            foreach (var roleId in lisRoleId)
            {
                var userRole = new UserRoleEntity
                {
                    UserId = userId,
                    RoleId = roleId,
                };
                userRoles.Add(userRole);
            }

            await Context.UserRoles.AddRangeAsync(userRoles, cancellationToken);
        }

        public async Task<bool> IsExistAsync(Expression<Func<UserEntity, bool>> predicate)
        {
            return await _context.Users.AnyAsync(predicate);
        }
    }
}