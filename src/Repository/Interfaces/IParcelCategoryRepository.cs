using MetroShip.Repository.Extensions;
using MetroShip.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MetroShip.Repository.Base;

namespace MetroShip.Repository.Interfaces
{
    public interface IParcelCategoryRepository : IBaseRepository<ParcelCategory>
    {
        Task<PaginatedList<ParcelCategory>>
            GetPaginatedListForListResponseAsync(
                int pageNumber,
                int pageSize,
                bool isIncludeAllCategoryInsurances = false,
                Expression<Func<ParcelCategory, bool>> predicate = null,
                Expression<Func<ParcelCategory, object>> orderBy = null,
                bool? isDesc = false);
    }
}
