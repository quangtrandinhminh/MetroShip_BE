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

namespace MetroShip.Service.Services;

public class InsuranceService(IServiceProvider serviceProvider) : IInsuranceService
{
    private readonly IInsuranceRepository _insuranceRepository = serviceProvider.GetRequiredService<IInsuranceRepository>();
    private readonly IUnitOfWork _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
    private readonly IMapperlyMapper _mapper = serviceProvider.GetRequiredService<IMapperlyMapper>();
    private readonly IHttpContextAccessor _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>(); 

    // get all insurance policies
    public async Task<PaginatedListResponse<InsurancePolicyResponse>> GetAllPoliciesPaginatedList (PaginatedListRequest request)
    {
        _logger.Information("Getting all insurance policies with pagination: {@Request}", request);

        var paginatedList = await _insuranceRepository.GetAllPaginatedQueryable(request.PageNumber, request.PageSize);
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
}