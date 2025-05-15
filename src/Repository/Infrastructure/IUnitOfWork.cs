using Microsoft.AspNetCore.Http;

namespace MetroShip.Repository.Infrastructure
{
    public interface IUnitOfWork : IDisposable
    {
        int SaveChange();
        Task<int> SaveChangeAsync(IHttpContextAccessor? accessor = null);
    }
}
