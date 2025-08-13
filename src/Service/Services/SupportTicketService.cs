using MetroShip.Repository.Base;
using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.SupportTicket;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Mapper;
using MetroShip.Service.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace MetroShip.Service.Services;

public class SupportTicketService(IServiceProvider serviceProvider) : ISupportTicketService
{
    private readonly IBaseRepository<SupportTicket> _supportingTicketRepository = serviceProvider.GetRequiredService<IBaseRepository<SupportTicket>>();
    private readonly IUnitOfWork _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
    private readonly IMapperlyMapper _mapper = serviceProvider.GetRequiredService<IMapperlyMapper>();
    private readonly IHttpContextAccessor _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();

    public async Task CreateTicketAsync(SupportTicketRequest request, CancellationToken token = default)
    {
        var userId = JwtClaimUltils.GetUserId(_httpContextAccessor);
        _logger.Information("Creating support ticket for shipment {ShipmentId}", request.ShipmentId);
        var ticket = _mapper.MapToSupportTicketEntity(request);
        ticket.OpenById = userId;
        await _supportingTicketRepository.AddAsync(ticket, token);
        await _unitOfWork.SaveChangeAsync();
    }

    public async Task<SupportTicketResponse> GetTicketByIdAsync(string ticketId)
    {
        _logger.Information("Fetching support ticket with ID {TicketId}", ticketId);
        var ticket = await _supportingTicketRepository.GetSingleAsync(
                       t => t.Id == ticketId);

        return _mapper.MapToSupportTicketResponse(ticket);
    }

    // get all paginated
    public async Task<PaginatedListResponse<SupportTicketResponse>> GetAllTicketsAsync(
        PaginatedListRequest request)
    {
        _logger.Information("Fetching all support tickets with pagination");
        var tickets = await _supportingTicketRepository.GetAllPaginatedQueryable(
            request.PageNumber, request.PageSize);

        return _mapper.MapToSupportTicketPaginatedList(tickets);
    }
}