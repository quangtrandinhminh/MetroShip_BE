using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.SupportTicket;
using MetroShip.Service.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MetroShip.WebAPI.Controllers
{
    [Route("api/support-tickets")]
    [ApiController]
    public class SupportTicketController(IServiceProvider serviceProvider) : ControllerBase
    {
        private readonly ISupportTicketService _supportTicketService = serviceProvider.GetRequiredService<ISupportTicketService>();

        [HttpPost]
        public async Task<IActionResult> CreateTicketAsync([FromBody] SupportTicketRequest request, CancellationToken token = default)
        {
            await _supportTicketService.CreateTicketAsync(request, token);
            return Ok(BaseResponse.OkResponseDto("Support ticket created successfully.", null));
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
            return Ok(BaseResponse.OkResponseDto(tickets));
        }
    }
}
