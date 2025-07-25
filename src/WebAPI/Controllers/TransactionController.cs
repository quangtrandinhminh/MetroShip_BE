﻿using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Transaction;
using MetroShip.Service.Helpers;
using MetroShip.Service.Interfaces;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MetroShip.WebAPI.Controllers
{
    [ApiController]
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
        public async Task<ActionResult<PaginatedListResponse<TransactionResponse>>> GetAllAsync(
            [FromQuery] PaymentStatusEnum? status,[FromQuery] PaginatedListRequest request)
        {
            var result = await _transactionService.GetAllAsync(status, request);
            return Ok(BaseResponse.OkResponseDto(result, _transactionEnumResponse));
        }
    }
}