using MetroShip.Service.ApiModels.InsurancePolicy;
using MetroShip.Service.ApiModels.PaginatedList;

namespace MetroShip.Service.Interfaces;

public interface IInsuranceService
{
    Task<PaginatedListResponse<InsurancePolicyResponse>> GetAllPoliciesPaginatedList(PaginatedListRequest request);

    Task<InsurancePolicyResponse?> GetPolicyById(string id);

    Task<string> CreatePolicy(InsurancePolicyRequest request);

    Task<string> ActivatePolicy(string id);

    Task<string> DeactivatePolicy(string id);
}