using MetroShip.Repository.Base;
using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using MetroShip.Repository.Repositories;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.ParcelCategory;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Mapper;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Exceptions;
using MetroShip.Utility.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MetroShip.Service.Services;

public class ParcelCategoryService(IServiceProvider serviceProvider) : IParcelCategoryService
{
    private readonly IParcelCategoryRepository _parcelCategoryRepository = serviceProvider.GetRequiredService<IParcelCategoryRepository>();
    private readonly IMapperlyMapper _mapper = serviceProvider.GetRequiredService<IMapperlyMapper>();
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
    private readonly IUnitOfWork _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
    private readonly IHttpContextAccessor _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    private readonly IBaseRepository<CategoryInsurance> _categoryInsuranceRepository = serviceProvider.GetRequiredService<IBaseRepository<CategoryInsurance>>();

    public async Task<PaginatedListResponse<ParcelCategoryResponse>> GetAllAsync(
        bool? isActive,
        PaginatedListRequest request,
        bool isIncludeAllCategoryInsurances = false
        )
    {
        _logger.Information("Get all parcel categories. IsActive: {isActive}", isActive);

        Expression<Func<ParcelCategory, bool>> predicate = c => c.DeletedAt == null;
        if (isActive.HasValue)
        {
            predicate = predicate.And(c => c.IsActive == isActive.Value);
        }

        /*var paginatedCategories = await _parcelCategoryRepository.GetAllPaginatedQueryable(
            pageNumber: 1,
            pageSize: 10,
            predicate: predicate,
            orderBy: c => c.CreatedAt,
            isAscending: true,
            includeProperties: c => c.CategoryInsurances
        );*/

        var paginatedCategories = await _parcelCategoryRepository.GetPaginatedListForListResponseAsync(
                request.PageNumber,
                request.PageSize,
                isIncludeAllCategoryInsurances,
                predicate,
                c => c.CreatedAt,
                true);

        return _mapper.MapToParcelCategoryPaginatedList(paginatedCategories);
    }

    public async Task<ParcelCategoryResponse> GetByIdAsync(Guid id)
    {
        _logger.Information("Get parcel category by id {id}", id);

        var category = await GetParcelCategoryById(id);
        return _mapper.MapToParcelCategoryResponse(category);
    }

    public async Task<string> CreateAsync(ParcelCategoryCreateRequest request)
    {
        _logger.Information("Create parcel category {@request}", request);

        // Correctly map the request to the ParcelCategory entity
        var entity = _mapper.MapToParcelCategoryEntity(request);

        entity.CategoryInsurances.Add(new CategoryInsurance
        {
            InsurancePolicyId = request.InsurancePolicyId,
            ParcelCategoryId = entity.Id,
            IsActive = true
        });

        // Pass the correctly mapped entity to the repository
        await _parcelCategoryRepository.AddAsync(entity);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);

        return ResponseMessageConstantsParcelCategory.CREATE_SUCCESS;
    }

    public async Task<string> UpdateAsync(ParcelCategoryUpdateRequest request)
    {
        _logger.Information("Update parcel category {@request}", request);

        var entity = await GetParcelCategoryById(Guid.Parse(request.Id)); // Convert string to Guid

        var oldPolicy = entity.CategoryInsurances.FirstOrDefault(ci => ci.IsActive);
        if (oldPolicy != null && oldPolicy.InsurancePolicyId != request.InsurancePolicyId)
        {
            // Deactivate the old insurance policy
            oldPolicy.IsActive = false;
            _categoryInsuranceRepository.Update(oldPolicy);

            // Add a new insurance policy
            _categoryInsuranceRepository.Add(new CategoryInsurance
            {
                InsurancePolicyId = request.InsurancePolicyId,
                ParcelCategoryId = entity.Id,
                IsActive = true
            });
        }

        _mapper.MapParcelCategoryUpdateRequestToEntity(request, entity);
        _parcelCategoryRepository.Update(entity);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);

        // Return the updated entity mapped to a response
        //return _mapper.MapToParcelCategoryResponse(entity);

        return ResponseMessageConstantsParcelCategory.UPDATE_SUCCESS;
    }

    public async Task DeleteAsync(Guid id)
    {
        _logger.Information("Soft delete parcel category by id {id}", id);

        var entity = await GetParcelCategoryById(id);
        entity.DeletedAt = CoreHelper.SystemTimeNow;

        // deactivate all associated category insurances
        foreach (var categoryInsurance in entity.CategoryInsurances.Where(ci => ci.IsActive))
        {
            categoryInsurance.IsActive = false;
            _categoryInsuranceRepository.Update(categoryInsurance);
        }

        _parcelCategoryRepository.Update(entity);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
    }

    private async Task<ParcelCategory> GetParcelCategoryById(Guid id)
    {
        var entity = await _parcelCategoryRepository.GetAll()
            .Where(c => c.Id == id.ToString() && c.DeletedAt == null)
            .Include(c => c.CategoryInsurances.Where(ci => ci.DeletedAt == null))
                .ThenInclude(ci => ci.InsurancePolicy)
            .FirstOrDefaultAsync();

        if (entity == null)
        {
            throw new AppException(
                HttpResponseCodeConstants.NOT_FOUND,
                ResponseMessageConstantsParcelCategory.NOT_FOUND,
                StatusCodes.Status404NotFound);
        }

        return entity;
    }
}