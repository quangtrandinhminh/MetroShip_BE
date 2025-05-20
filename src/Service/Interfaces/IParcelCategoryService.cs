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
        Task<PaginatedListResponse<ParcelCategoryResponse>> GetAllAsync(bool? isActive, int pageNumber, int pageSize);

        Task<ParcelCategoryResponse> GetByIdAsync(Guid id);

        Task<ParcelCategoryResponse> CreateAsync(ParcelCategoryCreateRequest request);

        Task UpdateAsync(ParcelCategoryUpdateRequest request);

        Task DeleteAsync(Guid id);
    }
}
