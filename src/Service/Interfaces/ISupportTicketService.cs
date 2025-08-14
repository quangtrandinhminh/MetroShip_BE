using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.SupportTicket;

namespace MetroShip.Service.Interfaces;

public interface ISupportTicketService
{
    Task<SupportTicketResponse> GetTicketByIdAsync(string ticketId);

    Task<PaginatedListResponse<SupportTicketResponse>> GetAllTicketsAsync(
        PaginatedListRequest request);

    Task CreateTicketAsync(SupportTicketRequest request, CancellationToken token = default);
    /*Task<SupportTicketResponse> UpdateSupportTicketAsync(string id, SupportTicketUpdateRequest request);

    Task ResolveSupportTicketAsync(string id, SupportTicketResolveRequest request);

    Task CloseSupportTicketAsync(string id, SupportTicketCloseRequest request);*/
}