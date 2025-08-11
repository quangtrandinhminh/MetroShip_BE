using System.Linq;
using System.Linq.Expressions;
using MetroShip.Repository.Base;
using MetroShip.Repository.Extensions;
using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using MetroShip.Utility.Helpers;
using Microsoft.EntityFrameworkCore;

namespace MetroShip.Repository.Repositories
{
    public class ParcelCategoryRepository : BaseRepository<ParcelCategory>,IParcelCategoryRepository
    {
        private readonly AppDbContext _context;

        public ParcelCategoryRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<PaginatedList<ParcelCategory>>
            GetPaginatedListForListResponseAsync(
                int pageNumber,
                int pageSize,
                bool isIncludeAllCategoryInsurances = false,
                Expression<Func<ParcelCategory, bool>> predicate = null,
                Expression<Func<ParcelCategory, object>> orderBy = null,
                bool? isDesc = false)
        {
            IQueryable<ParcelCategory> q = _context.ParcelCategories;

            // Apply filtering
            if (predicate is not null)
                q = q.Where(predicate);

            // Apply ordering
            q = orderBy is not null
                ? isDesc.HasValue && isDesc.Value
                    ? q.OrderByDescending(orderBy)
                    : q.OrderBy(orderBy)
                : q.OrderByDescending(s => s.CreatedAt);

            // Project into your DTO, grabbing only the first & last itinerary
            // Project into your DTO
            var projected = q.Select(s => new ParcelCategory
            {
                Id = s.Id,
                CategoryName = s.CategoryName,
                Description = s.Description,
                WeightLimitKg = s.WeightLimitKg,
                HeightLimitCm = s.HeightLimitCm,
                WidthLimitCm = s.WidthLimitCm,
                LengthLimitCm = s.LengthLimitCm,
                VolumeLimitCm3 = s.VolumeLimitCm3,
                TotalSizeLimitCm = s.TotalSizeLimitCm,
                IsInsuranceRequired = s.IsInsuranceRequired,
                IsActive = s.IsActive,
                CategoryInsurances = s.CategoryInsurances
                    .Where(ci => isIncludeAllCategoryInsurances || ci.IsActive) // Simple conditional
                    .Select(ci => new CategoryInsurance
                    {
                        Id = ci.Id,
                        InsurancePolicy = new InsurancePolicy
                        {
                            Id = ci.InsurancePolicy.Id,
                            Name = ci.InsurancePolicy.Name,
                            BaseFeeVnd = ci.InsurancePolicy.BaseFeeVnd,
                            MaxParcelValueVnd = ci.InsurancePolicy.MaxParcelValueVnd,
                            InsuranceFeeRateOnValue = ci.InsurancePolicy.InsuranceFeeRateOnValue,
                            StandardCompensationValueVnd = ci.InsurancePolicy.StandardCompensationValueVnd,
                            MaxInsuranceRateOnValue = ci.InsurancePolicy.MaxInsuranceRateOnValue,
                            MinInsuranceRateOnValue = ci.InsurancePolicy.MinInsuranceRateOnValue,
                            ValidFrom = ci.InsurancePolicy.ValidFrom,
                            ValidTo = ci.InsurancePolicy.ValidTo,
                            IsActive = ci.InsurancePolicy.IsActive,
                        }
                    }).ToList()
            });

            // Use your existing paging helper on the projection
            return await PaginatedList<ParcelCategory>
                .CreateAsync(projected, pageNumber, pageSize);
        }
    }
}
