using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels.InsurancePolicy;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Mapper;
using MetroShip.Service.Validations;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Exceptions;
using MetroShip.Utility.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Linq.Expressions;

namespace MetroShip.Service.Services;

public class InsuranceService(IServiceProvider serviceProvider) : IInsuranceService
{
    private readonly IInsuranceRepository _insuranceRepository = serviceProvider.GetRequiredService<IInsuranceRepository>();
    private readonly IUnitOfWork _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
    private readonly IMapperlyMapper _mapper = serviceProvider.GetRequiredService<IMapperlyMapper>();
    private readonly IHttpContextAccessor _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>(); 

    // get all insurance policies
    public async Task<PaginatedListResponse<InsurancePolicyResponse>> GetAllPoliciesPaginatedList (
        PaginatedListRequest request, bool? isActive = null)
    {
        _logger.Information("Getting all insurance policies with pagination: {@Request}", request);
        Expression<Func<InsurancePolicy, bool>> predicate = x => x.DeletedAt == null;
        if (isActive.HasValue)
        {
            predicate = predicate.And(x => x.IsActive == isActive.Value);
        }

        var paginatedList = await _insuranceRepository.GetAllPaginatedQueryable(
            request.PageNumber, request.PageSize, predicate);

        // order by isActive first, then by LastUpdatedAt descending
        paginatedList.Items = paginatedList.Items
            .OrderByDescending(ip => ip.IsActive)
            .ThenByDescending(ip => ip.LastUpdatedAt)
            .ToList();
        var response = _mapper.MapToInsurancePolicyPaginatedList(paginatedList);
        return response;
    }

    // get insurance policy by id
    public async Task<InsurancePolicyResponse?> GetPolicyById(string id)
    {
        _logger.Information("Getting insurance policy by id: {Id}", id);
        var entity = await GetPolicyAsync(id);
        var response = _mapper.MapToInsurancePolicyResponse(entity);
        return response;
    }

    private async Task<InsurancePolicy> GetPolicyAsync(string id)
    {
        var entity = await _insuranceRepository.GetSingleAsync(e => e.Id == id, false,
            x => x.CategoryInsurances);
        if (entity == null)
        {
            throw new AppException(
                ErrorCode.BadRequest,
                ResponseMessageInsurancePolicy.INSURANCE_POLICY_NOT_FOUND,
                StatusCodes.Status400BadRequest);
        }
        return entity;
    }

    public async Task<string> CreatePolicy(InsurancePolicyRequest request)
    {
        _logger.Information("Creating insurance policy: {@Request}", request);
        InsurancePolicyValidator.ValidatePricingConfigRequest(request);

        var entity = _mapper.MapToInsurancePolicy(request);
        
        if (entity.IsActive)
        {
            DateOnly today = DateOnly.FromDateTime(CoreHelper.SystemTimeNow.DateTime.UtcToSystemTime());
            entity.ValidFrom = today;
        }

        await _insuranceRepository.AddAsync(entity);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);

        return ResponseMessageInsurancePolicy.INSURANCE_POLICY_CREATE_SUCCESS;
    }

    public async Task<string> ActivatePolicy(string id)
    {
        _logger.Information("Activating insurance policy with id: {Id}", id);
        var entity = await GetPolicyAsync(id);
        if (entity.IsActive)
        {
            throw new AppException(
            ErrorCode.BadRequest,
            ResponseMessageInsurancePolicy.INSURANCE_POLICY_ALREADY_ACTIVATED,
            StatusCodes.Status400BadRequest);
        }

        if (entity.ValidTo != null)
        {
            throw new AppException(
            ErrorCode.BadRequest,
            ResponseMessageInsurancePolicy.INSURANCE_POLICY_EXPIRED,
            StatusCodes.Status400BadRequest);
        }

        DateOnly today = DateOnly.FromDateTime(CoreHelper.SystemTimeNow.DateTime.UtcToSystemTime());
        entity.IsActive = true;
        entity.ValidFrom = today;

        _insuranceRepository.Update(entity);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);

        return ResponseMessageInsurancePolicy.INSURANCE_POLICY_ACTIVATE_SUCCESS;
    }

    public async Task<string> DeactivatePolicy(string id)
    {
        _logger.Information("Deactivating insurance policy with id: {Id}", id);
        var entity = await GetPolicyAsync(id);

        if (!entity.IsActive)
        {
            throw new AppException(
            ErrorCode.BadRequest,
            ResponseMessageInsurancePolicy.INSURANCE_POLICY_ALREADY_DEACTIVATED,
            StatusCodes.Status400BadRequest);
        }

        if (entity.CategoryInsurances.Any(ci => ci.IsActive))
        {
            throw new AppException(
            ErrorCode.BadRequest,
            ResponseMessageInsurancePolicy.INSURANCE_POLICY_IN_USE,
            StatusCodes.Status400BadRequest);
        }

        DateOnly today = DateOnly.FromDateTime(CoreHelper.SystemTimeNow.DateTime.UtcToSystemTime());
        entity.IsActive = false;
        entity.ValidTo = today;

        _insuranceRepository.Update(entity);
        await _unitOfWork.SaveChangeAsync(_httpContextAccessor);

        return ResponseMessageInsurancePolicy.INSURANCE_POLICY_DEACTIVATE_SUCCESS;
    }

    // get all active insurance policies dropdown
    public async Task<IList<InsurancePolicyResponse>> GetAllActivePoliciesDropdown()
    {
        _logger.Information("Getting all active insurance policies for dropdown");
        var entities = await _insuranceRepository.FindByConditionAsync(e => e.IsActive && e.DeletedAt == null);
        var response = _mapper.MapToInsurancePolicyResponseList(entities);
        return response;
    }

    // delete insurance policy
    public async Task<string> DeletePolicy(string id)
    {
        _logger.Information("Deleting insurance policy with id: {Id}", id);
        var entity = await _insuranceRepository.GetSingleAsync(e => e.Id == id);
        if (entity == null)
        {
            throw new AppException(
            ErrorCode.BadRequest,
            ResponseMessageInsurancePolicy.INSURANCE_POLICY_NOT_FOUND,
            StatusCodes.Status400BadRequest);
        }

        if (entity.IsActive)
        {
            throw new AppException(
            ErrorCode.BadRequest,
            ResponseMessageInsurancePolicy.INSURANCE_POLICY_ACTIVE_CANNOT_DELETE,
            StatusCodes.Status400BadRequest);
        }

        var isCategoryInsuranceExist = await _insuranceRepository.IsExistAsync(
            i => i.CategoryInsurances.Any() && i.Id == entity.Id);
        if (isCategoryInsuranceExist)
        {
            _logger.Information("Insurance policy with ID: {Id} has associated CategoryInsurances. Performing soft delete.", id);
            entity.DeletedAt = CoreHelper.SystemTimeNow;
            _insuranceRepository.Update(entity);
            await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
        }
        else
        {
            _logger.Information("Insurance policy with ID: {Id} has no associated CategoryInsurances. Performing hard delete.", id);
            _insuranceRepository.Delete(entity);
            await _unitOfWork.SaveChangeAsync(_httpContextAccessor);
        }

        return ResponseMessageInsurancePolicy.INSURANCE_POLICY_DELETE_SUCCESS;
    }
}