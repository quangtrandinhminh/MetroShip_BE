using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.SupportTicket;

namespace MetroShip.Service.Interfaces;

public interface ISupportTicketService
{
    Task<SupportTicketResponse> GetTicketByIdAsync(string ticketId);

    Task<PaginatedListResponse<SupportTicketResponse>> GetAllTicketsAsync(
        PaginatedListRequest request);

    Task<string> CreateTicketAsync(SupportTicketRequest request, CancellationToken token = default);

    Task<string> ResolveTicketAsync(ResolveTicketRequest request);
    Task<string> CloseTicketAsync(string ticketId);
}