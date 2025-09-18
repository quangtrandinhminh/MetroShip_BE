using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Transaction;
using MetroShip.Service.ApiModels.VNPay;
using MetroShip.Service.Helpers;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Services;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MetroShip.WebAPI.Controllers
{
    [ApiController]
    [Authorize]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly TransactionEnumResponse _transactionEnumResponse = new();

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        /// <summary>
        /// Get all transactions (paginated & optional payment status filter)
        /// </summary>
        [HttpGet(WebApiEndpoint.TransactionEndpoint.GetTransactions)]
        public async Task<IActionResult> GetTransactions([FromQuery] PaginatedListRequest request, [FromQuery] PaymentStatusEnum? status, 
        [FromQuery] string? searchKeyword,[FromQuery] DateTimeOffset? createdFrom,[FromQuery] DateTimeOffset? createdTo,[FromQuery] OrderByRequest orderByRequest)
        {
            var result = await _transactionService.GetAllTransactionsAsync(
                request, status, searchKeyword, createdFrom, createdTo, orderByRequest
            );

            return Ok(BaseResponse.OkResponseDto(result, _transactionEnumResponse));
        }

        [HttpGet(WebApiEndpoint.TransactionEndpoint.GetTransactionType)]
        public ActionResult<IList<EnumResponse>> GetTransactionType()
        {
            var transactionTypes = EnumHelper.GetEnumList<TransactionTypeEnum>();
            return Ok(BaseResponse.OkResponseDto(transactionTypes));
        }

        // get banks from vietqr
        [HttpGet(WebApiEndpoint.TransactionEndpoint.GetBanksFromVietQr)]
        public async Task<IActionResult> GetBanksFromVietQr()
        {
            var result = await _transactionService.GetBanksFromVietQr();
            return Ok(BaseResponse.OkResponseDto(result));
        }

        // generate bank qr link
        [HttpGet(WebApiEndpoint.TransactionEndpoint.GenerateBankQrLink)]
        public async Task<IActionResult> GenerateBankQrLink([FromRoute] int bankId, [FromRoute] string accountNo, [FromQuery] decimal? amount)
        {
            var result = await _transactionService.GenerateBankQrLink(bankId, accountNo, amount);
            return Ok(BaseResponse.OkResponseDto(result));
        }

        [Authorize(Roles = $"{nameof(UserRoleEnum.Customer)}, {nameof(UserRoleEnum.Staff)}")]
        [HttpPost(WebApiEndpoint.ShipmentEndpoint.CreateTransactionVnPay)]
        public async Task<IActionResult> CreateVnPayUrl([FromBody] TransactionRequest request)
        {
            var paymentUrl = await _transactionService.CreateVnPayTransaction(request);
            return Ok(BaseResponse.OkResponseDto(paymentUrl));
        }

        [HttpGet(WebApiEndpoint.ShipmentEndpoint.VnpayExecute)]
        public async Task<IActionResult> VnPayExecute([FromQuery] VnPayCallbackModel model)
        {
            var result = await _transactionService.ExecuteVnPayPayment(model);
            // Redirect to the payment result page
            if (string.IsNullOrEmpty(result))
            {
                return Ok(BaseResponse.OkResponseDto("Update shipment success!", null));
            }

            return Redirect(result);
        }
    }
}