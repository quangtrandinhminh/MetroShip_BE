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

namespace MetroShip.Service.Services;

public class ParcelCategoryService(IServiceProvider serviceProvider) : IParcelCategoryService
{
    private readonly IParcelCategoryRepository _parcelCategoryRepository = serviceProvider.GetRequiredService<IParcelCategoryRepository>();
    private readonly IMapperlyMapper _mapper = serviceProvider.GetRequiredService<IMapperlyMapper>();
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
    private readonly IUnitOfWork _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();

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

        var category = await _parcelCategoryRepository.GetSingleAsync(c => c.Id == id.ToString());

        if (category == null)
        {
            throw new AppException(
                HttpResponseCodeConstants.NOT_FOUND,
                ResponseMessageConstantsParcelCategory.NOT_FOUND,
                StatusCodes.Status404NotFound);
        }

        return _mapper.MapToParcelCategoryResponse(category);
    }

    public async Task<ParcelCategoryResponse> CreateAsync(ParcelCategoryCreateRequest request)
    {
        _logger.Information("Create parcel category {@request}", request);

        // Correctly map the request to the ParcelCategory entity
        var entity = _mapper.MapToParcelCategoryEntity(request);

        entity.CategoryInsurances.Add(new CategoryInsurance
        {
            InsurancePolicyId = request.InsurancePolicyId,
            ParcelCategoryId = entity.Id,
        });

        // Pass the correctly mapped entity to the repository
        await _parcelCategoryRepository.AddAsync(entity);
        await _unitOfWork.SaveChangeAsync();

        // Return the created entity mapped to a response
        return _mapper.MapToParcelCategoryResponse(entity);
    }

    public async Task<ParcelCategoryResponse> UpdateAsync(string id, ParcelCategoryUpdateRequest request)
    {
        _logger.Information("Update parcel category {@request}", request);

        var entity = await GetParcelCategoryById(Guid.Parse(id)); // Convert string to Guid

        _mapper.MapParcelCategoryUpdateRequestToEntity(request, entity);
        _parcelCategoryRepository.Update(entity);
        await _unitOfWork.SaveChangeAsync();

        // Return the updated entity mapped to a response
        return _mapper.MapToParcelCategoryResponse(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        _logger.Information("Soft delete parcel category by id {id}", id);

        var entity = await GetParcelCategoryById(id);
        entity.DeletedAt = CoreHelper.SystemTimeNow;

        _parcelCategoryRepository.Update(entity);
        await _unitOfWork.SaveChangeAsync();
    }

    private async Task<ParcelCategory> GetParcelCategoryById(Guid id)
    {
        var entity = await _parcelCategoryRepository.GetSingleAsync(
            c => c.Id == id.ToString(), false,
            c => c.CategoryInsurances);

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