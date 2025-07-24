using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Storage;

namespace MetroShip.Repository.Infrastructure
{
    public interface IUnitOfWork : IDisposable
    {
        int SaveChange();
        Task<int> SaveChangeAsync(IHttpContextAccessor? accessor = null);

        Task<IDbContextTransaction> BeginTransactionAsync();
        IDbContextTransaction BeginTransaction();
        Task RollbackTransactionAsync(IDbContextTransaction transaction);
        void RollbackTransaction(IDbContextTransaction transaction);
    }
}
