using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.SupportTicket;
using MetroShip.Service.Helpers;
using MetroShip.Service.Interfaces;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MetroShip.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    public class SupportTicketController(IServiceProvider serviceProvider) : ControllerBase
    {
        private readonly ISupportTicketService _supportTicketService = serviceProvider.GetRequiredService<ISupportTicketService>();
        private readonly IList<EnumResponse> _statusEnumResponses = EnumHelper.GetEnumList<SupportTicketStatusEnum>();
        private readonly IList<EnumResponse> _supportTypeResponses = EnumHelper.GetEnumList<SupportTypeEnum>();

        [HttpPost(WebApiEndpoint.SupportTicketEndpoint.CreateTicket)]
        public async Task<IActionResult> CreateTicketAsync([FromBody] SupportTicketRequest request, CancellationToken token = default)
        {
            var result = await _supportTicketService.CreateTicketAsync(request, token);
            return Ok(BaseResponse.OkResponseDto(result, null));
        }

        [HttpGet(WebApiEndpoint.SupportTicketEndpoint.GetTicketById)]
        public async Task<IActionResult> GetTicketByIdAsync([FromRoute] string ticketId)
        {
            var ticket = await _supportTicketService.GetTicketByIdAsync(ticketId);
            return Ok(BaseResponse.OkResponseDto(ticket));
        }

        [HttpGet(WebApiEndpoint.SupportTicketEndpoint.GetAllTickets)]
        public async Task<IActionResult> GetAllTicketsAsync([FromQuery] PaginatedListRequest request)
        {
            var tickets = await _supportTicketService.GetAllTicketsAsync(request);
            return Ok(BaseResponse.OkResponseDto(tickets, _statusEnumResponses));
        }

        [HttpGet(WebApiEndpoint.SupportTicketEndpoint.GetTicketStatusEnum)]
        public IActionResult GetTicketStatusEnum()
        {
            return Ok(BaseResponse.OkResponseDto(_statusEnumResponses));
        }

        [HttpGet(WebApiEndpoint.SupportTicketEndpoint.GetSupportTypeEnum)]
        public IActionResult GetSupportTypeEnum()
        {
            return Ok(BaseResponse.OkResponseDto(_supportTypeResponses));
        }

        [HttpPost(WebApiEndpoint.SupportTicketEndpoint.ResolveTicket)] 
        public async Task<IActionResult> ResolveTicketAsync([FromBody] ResolveTicketRequest request)
        {
            var result = await _supportTicketService.ResolveTicketAsync(request);
            return Ok(BaseResponse.OkResponseDto(result, null));
        }

        [HttpPost(WebApiEndpoint.SupportTicketEndpoint.CloseTicket)]
        public async Task<IActionResult> CloseTicketAsync([FromRoute] string ticketId)
        {
            var message = await _supportTicketService.CloseTicketAsync(ticketId);
            return Ok(BaseResponse.OkResponseDto(message, null));
        }
    }
}
