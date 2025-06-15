using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Transaction;
using MetroShip.Service.Helpers;
using MetroShip.Service.Interfaces;
using MetroShip.Utility.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MetroShip.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly IList<EnumResponse> _enumResponses = EnumHelper.GetEnumList<PaymentStatusEnum>();

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        /// <summary>
        /// Get all transactions (paginated & optional payment status filter)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PaginatedListResponse<TransactionResponse>>> GetAllAsync(
            [FromQuery] PaymentStatusEnum? status,[FromQuery] PaginatedListRequest request)
        {
            var result = await _transactionService.GetAllAsync(status, request);
            return Ok(BaseResponse.OkResponseDto(result, _enumResponses));
        }
    }
}