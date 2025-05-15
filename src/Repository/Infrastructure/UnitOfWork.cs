using System.Security.Claims;
using MetroShip.Repository.Models.Base;
using MetroShip.Utility.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MetroShip.Repository.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _dbContext;
        private bool disposed = false;
        private IHttpContextAccessor? _httpContextAccessor;
        public UnitOfWork(IServiceProvider serviceProvider)
        {
            _dbContext = serviceProvider.GetRequiredService<AppDbContext>();
        }
        #region Save
        public int SaveChange()
        {
            StandardizeEntities();
            return _dbContext.SaveChanges();
        }

        public Task<int> SaveChangeAsync(IHttpContextAccessor? accessor = null)
        {
            _httpContextAccessor = accessor;
            StandardizeEntities();
            return _dbContext.SaveChangesAsync();
        }

        private void StandardizeEntities()
        {
            var listState = new List<EntityState>
            {
                EntityState.Added,
                EntityState.Modified,
                EntityState.Deleted
            };

            var listEntry = _dbContext.ChangeTracker.Entries()
                .Where(x => x.Entity is BaseEntity && listState.Contains(x.State))
                .Select(x => x).ToList();

            var dateTimeNow = CoreHelper.SystemTimeNow;

            foreach (var entry in listEntry)
            {
                if (entry.Entity is BaseEntity baseEntity)
                {
                    if (entry.State == EntityState.Added)
                    {
                        baseEntity.DeletedAt = null;

                        baseEntity.LastUpdatedAt = baseEntity.CreatedAt = dateTimeNow;
                    }
                    else
                    {
                        if (baseEntity.DeletedAt != null)
                            baseEntity.DeletedAt =
                                ObjHelper.ReplaceNullOrDefault(baseEntity.DeletedAt, dateTimeNow);
                        else
                            baseEntity.LastUpdatedAt = dateTimeNow;
                    }
                }

                if (!(entry.Entity is BaseEntity entity)) continue;

                // Get the logged in user id from the claim and set it to the entity
                var loginUserId = _httpContextAccessor?.HttpContext?.User.FindFirst(ClaimTypes.Sid);
                if (loginUserId != null)
                {
                    //int? loggedInUserId = int.Parse(loginUserId.Value);
                    string? loggedInUserId = loginUserId.Value;

                    if (entry.State == EntityState.Added)
                    {
                        entity.CreatedBy = entity.LastUpdatedBy = entity.CreatedBy ?? loggedInUserId;
                    }
                    else
                    {
                        if (entity.DeletedAt != null)
                            entity.DeletedBy ??= loggedInUserId;
                        else
                            entity.LastUpdatedBy ??= loggedInUserId;
                    }
                }
            }
        }
        #endregion Save
        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _dbContext.Dispose();
                }
            }
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion Dispose
    }
}