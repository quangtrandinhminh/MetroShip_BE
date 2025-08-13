using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;
using MetroShip.Repository.Extensions;
using MetroShip.Repository.Models.Identity;

namespace MetroShip.Repository.Interfaces
{
    public interface IUserRepository : IUserStore<UserEntity>
    {
        Task<int> SaveChangeAsync();
        Task<IdentityResult> CreateUserAsync(UserEntity userEntity, CancellationToken cancellationToken = default);
        Task<IdentityResult> UpdateAsync(UserEntity userEntity);
        Task<UserEntity?> GetSingleAsync(Expression<Func<UserEntity, bool>>? predicate = null, params Expression<Func<UserEntity, object>>[] includeProperties);
        IQueryable<UserEntity> GetAllWithCondition(Expression<Func<UserEntity, bool>> predicate = null, params Expression<Func<UserEntity, object>>[] includeProperties);
        Task<UserEntity?> GetUserByIdAsync(object userId);

        Task<PaginatedList<UserEntity>> GetAllPaginatedQueryable(int pageNumber,
            int pageSize,
            Expression<Func<UserEntity, bool>> predicate = null,
            Expression<Func<UserEntity, object>>? orderBy = null,
            params Expression<Func<UserEntity, object>>[]? includeProperties);

        Task<int> CountAsync(Expression<Func<UserEntity, bool>> predicate);

        Task AddUserToRoleAsync(string userId, List<string> lisRoleId, CancellationToken cancellationToken = default);

        Task<bool> IsExistAsync(Expression<Func<UserEntity, bool>> predicate);

        Task<string?> GetUserNameByIdAsync(string userId);
    }
}