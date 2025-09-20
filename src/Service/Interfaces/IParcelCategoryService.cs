using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.ParcelCategory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.Interfaces
{
    public interface IParcelCategoryService
    {
        Task<PaginatedListResponse<ParcelCategoryResponse>> GetAllAsync(
            bool? isActive,
            PaginatedListRequest request,
            bool isIncludeAllCategoryInsurances = false
        );

        Task<ParcelCategoryResponse> GetByIdAsync(Guid id);

        Task<string> CreateAsync(ParcelCategoryCreateRequest request);

        Task<string> UpdateAsync(ParcelCategoryUpdateRequest request);

        Task DeleteAsync(Guid id);
    }
}
