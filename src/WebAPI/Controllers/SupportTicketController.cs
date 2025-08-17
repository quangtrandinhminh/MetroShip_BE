using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.SupportTicket;
using MetroShip.Service.Helpers;
using MetroShip.Service.Interfaces;
using MetroShip.Utility.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MetroShip.WebAPI.Controllers
{
    [Authorize]
    [Route("api/support-tickets")]
    [ApiController]
    public class SupportTicketController(IServiceProvider serviceProvider) : ControllerBase
    {
        private readonly ISupportTicketService _supportTicketService = serviceProvider.GetRequiredService<ISupportTicketService>();
        private readonly IList<EnumResponse> _enumResponses = EnumHelper.GetEnumList<SupportTicketStatusEnum>();

        [HttpPost]
        public async Task<IActionResult> CreateTicketAsync([FromBody] SupportTicketRequest request, CancellationToken token = default)
        {
            var result = await _supportTicketService.CreateTicketAsync(request, token);
            return Ok(BaseResponse.OkResponseDto(result, null));
        }

        [HttpGet("{ticketId}")]
        public async Task<IActionResult> GetTicketByIdAsync([FromRoute] string ticketId)
        {
            var ticket = await _supportTicketService.GetTicketByIdAsync(ticketId);
            return Ok(BaseResponse.OkResponseDto(ticket));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTicketsAsync([FromQuery] PaginatedListRequest request)
        {
            var tickets = await _supportTicketService.GetAllTicketsAsync(request);
            return Ok(BaseResponse.OkResponseDto(tickets, _enumResponses));
        }

        [HttpGet("status")]
        public IActionResult GetTicketStatusEnum()
        {
            return Ok(BaseResponse.OkResponseDto(_enumResponses));
        }

        [HttpPost("resolve")]
        public async Task<IActionResult> ResolveTicketAsync([FromBody] ResolveTicketRequest request)
        {
            var result = await _supportTicketService.ResolveTicketAsync(request);
            return Ok(BaseResponse.OkResponseDto(result, null));
        }

        [HttpPost("close/{ticketId}")]
        public async Task<IActionResult> CloseTicketAsync([FromRoute] string ticketId)
        {
            var message = await _supportTicketService.CloseTicketAsync(ticketId);
            return Ok(BaseResponse.OkResponseDto(message, null));
        }
    }
}
