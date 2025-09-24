using MetroShip.Repository.Base;
using MetroShip.Repository.Extensions;
using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.SupportTicket;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Mapper;
using MetroShip.Service.Utils;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Enums;
using MetroShip.Utility.Exceptions;
using MetroShip.Utility.Helpers;
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

    public async Task<string> CreateTicketAsync(SupportTicketRequest request, CancellationToken token = default)
    {
        var userId = JwtClaimUltils.GetUserId(_httpContextAccessor);
        _logger.Information("Creating support ticket for shipment {ShipmentId}", request.ShipmentId);

        // check if shipment only has one opened ticket with type CompensationRequired
        if (request.SupportType == SupportTypeEnum.CompensationRequired)
        {
            var existingTicket = await _supportingTicketRepository.IsExistAsync(
                    t => t.ShipmentId == request.ShipmentId &&
                    t.SupportType == SupportTypeEnum.CompensationRequired &&
                    t.Status == SupportTicketStatusEnum.Opened);

            if (existingTicket)
            {
                throw new AppException(
                ErrorCode.BadRequest,
                "Một phiếu hỗ trợ yêu cầu bồi thường đã được mở cho đơn hàng này.",
                StatusCodes.Status400BadRequest);
            }
        }


        var ticket = _mapper.MapToSupportTicketEntity(request);
        ticket.OpenById = userId;
        await _supportingTicketRepository.AddAsync(ticket, token);
        await _unitOfWork.SaveChangeAsync();
        return ResponseMessageSupportTicket.TICKET_CREATE_SUCCESS;
    }

    public async Task<SupportTicketResponse> GetTicketByIdAsync(string ticketId)
    {
        _logger.Information("Fetching support ticket with ID {TicketId}", ticketId);
        var ticket = await GetTicketById(ticketId);

        return _mapper.MapToSupportTicketResponse(ticket);
    }

    // get all paginated
    public async Task<PaginatedListResponse<SupportTicketResponse>> GetAllTicketsAsync(
        PaginatedListRequest request)
    {
        _logger.Information("Fetching all support tickets with pagination");
        PaginatedList<SupportTicket> tickets;
        var stationId = JwtClaimUltils.GetUserStation(_httpContextAccessor);
        var role = JwtClaimUltils.GetUserRole(_httpContextAccessor);
        
        if (stationId != null)
        {
            _logger.Information("User is associated with station {StationId}, filtering tickets by station", stationId);
            tickets = await _supportingTicketRepository.GetAllPaginatedQueryable(
                request.PageNumber,
                request.PageSize,
                t => (t.Shipment.DepartureStationId == stationId && t.Shipment.ReturnForShipmentId == null)
                || (t.Shipment.DestinationStationId == stationId && t.Shipment.ReturnForShipmentId != null)
                && t.DeletedAt == null);
        }
        else if (role.Contains(UserRoleEnum.Customer.ToString()))
        {
            _logger.Information("User is a staff member, filtering tickets by opened by user");
            var userId = JwtClaimUltils.GetUserId(_httpContextAccessor);
            tickets = await _supportingTicketRepository.GetAllPaginatedQueryable(
            request.PageNumber,
            request.PageSize,
            t => t.OpenById == userId && t.DeletedAt == null);
        }
        else
        {
            _logger.Information("User is an admin or has no station association, fetching all tickets");
            tickets = await _supportingTicketRepository.GetAllPaginatedQueryable(
            request.PageNumber,
            request.PageSize,
            t => t.DeletedAt == null);
        }

        return _mapper.MapToSupportTicketPaginatedList(tickets);
    }

    private async Task<SupportTicket> GetTicketById(string ticketId)
    {
        _logger.Information("Fetching support ticket with ID {TicketId}", ticketId);
        var ticket = await _supportingTicketRepository.GetSingleAsync(
                                  t => t.Id == ticketId);

        if (ticket == null)
        {
            throw new AppException(
            ErrorCode.NotFound,
            ResponseMessageSupportTicket.TICKET_NOT_FOUND,
            StatusCodes.Status404NotFound);
        }

        return ticket;
    }

    // resolve ticket
    public async Task<string> ResolveTicketAsync(ResolveTicketRequest request)
    {
        var userId = JwtClaimUltils.GetUserId(_httpContextAccessor);

        _logger.Information("Resolving support ticket with ID {TicketId}", request.TicketId);
        var ticket = await GetTicketById(request.TicketId);

        ticket.Status = SupportTicketStatusEnum.Resolved;
        ticket.ResolvedAt = CoreHelper.SystemTimeNow;
        ticket.ResolvedById = userId;
        ticket.ResolvedContent = request.ResolvedContent;
        _supportingTicketRepository.Update(ticket);
        await _unitOfWork.SaveChangeAsync();
        return ResponseMessageSupportTicket.TICKET_STATUS_UPDATE_SUCCESS;
    }

    // close ticket
    public async Task<string> CloseTicketAsync(string ticketId)
    {
        var userId = JwtClaimUltils.GetUserId(_httpContextAccessor);

        _logger.Information("Closing support ticket with ID {TicketId}", ticketId);
        var ticket = await GetTicketById(ticketId);

        ticket.Status = SupportTicketStatusEnum.Closed;
        ticket.ClosedAt = CoreHelper.SystemTimeNow;
        ticket.ClosedBy = userId;
        _supportingTicketRepository.Update(ticket);
        await _unitOfWork.SaveChangeAsync();
        return ResponseMessageSupportTicket.TICKET_STATUS_UPDATE_SUCCESS;
    }
}