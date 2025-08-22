using MetroShip.Service.ApiModels.InsurancePolicy;
using MetroShip.Service.ApiModels.PaginatedList;

namespace MetroShip.Service.Interfaces;

public interface IInsuranceService
{
    Task<PaginatedListResponse<InsurancePolicyResponse>> GetAllPoliciesPaginatedList(PaginatedListRequest request);

    Task<InsurancePolicyResponse?> GetPolicyById(string id);
}