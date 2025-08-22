using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels.InsurancePolicy;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Mapper;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Exceptions;
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
        var entity = await _insuranceRepository.GetByIdAsync(id);
        if (entity == null)
        {
            throw new AppException(
                ErrorCode.BadRequest,
                ResponseMessageInsurancePolicy.INSURANCE_POLICY_NOT_FOUND,
                StatusCodes.Status400BadRequest);
        }
        return entity;
    }
}