﻿using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.Graph;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Shipment;
using MetroShip.Service.ApiModels.Transaction;
using MetroShip.Service.ApiModels.VNPay;
using MetroShip.Service.BusinessModels;
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
    [Route("api/shipments")]
    public class ShipmentController(
        IShipmentService shipmentService,
        ITransactionService transactionService
        ) : ControllerBase
    {
        private readonly IList<EnumResponse> _enumResponses = EnumHelper.GetEnumList<ShipmentStatusEnum>();


        [HttpGet(WebApiEndpoint.ShipmentEndpoint.GetShipments)]
        public async Task<IActionResult> Get([FromQuery] PaginatedListRequest request)
        {
            var response = await shipmentService.GetAllShipments(request);
            return Ok(BaseResponse.OkResponseDto(response, _enumResponses));
        }

        [HttpGet(WebApiEndpoint.ShipmentEndpoint.GetShipmentByTrackingCode)]
        public async Task<IActionResult> Get([FromRoute] string shipmentTrackingCode)
        {
            var response = await shipmentService.GetShipmentByTrackingCode(shipmentTrackingCode);
            return Ok(BaseResponse.OkResponseDto(response, _enumResponses));
        }

        [Authorize(Roles = nameof(UserRoleEnum.Customer))]
        [HttpGet(WebApiEndpoint.ShipmentEndpoint.GetShipmentsHistory)]
        public async Task<IActionResult> GetHistory([FromQuery] PaginatedListRequest request, ShipmentStatusEnum? status)
        {
            var response = await shipmentService.GetShipmentsHistory(request, status);
            return Ok(BaseResponse.OkResponseDto(response, _enumResponses));
        }

        [Authorize(Roles = nameof(UserRoleEnum.Customer))]
        [HttpPost(WebApiEndpoint.ShipmentEndpoint.CreateShipment)]
        public async Task<IActionResult> Create([FromBody] ShipmentRequest request)
        {
            var shipmentId = await shipmentService.BookShipment(request);
            return Created(nameof(Create), 
                BaseResponse.OkResponseDto(shipmentId));
        }

        [Authorize(Roles = nameof(UserRoleEnum.Customer))]
        [HttpPost(WebApiEndpoint.ShipmentEndpoint.GetShipmentItinerary)]
        public async Task<ActionResult<List<ItineraryResponse>>> GetPath(
            [FromBody]  BestPathRequest request)
        {
            var result = await shipmentService.FindPathAsync(request);
            return Ok(BaseResponse.OkResponseDto(result));
        }

        [HttpPost(WebApiEndpoint.ShipmentEndpoint.CreateTransactionVnPay)]
        public async Task<IActionResult> CreateVnPayUrl([FromBody] TransactionRequest request)
        {
            var paymentUrl = await transactionService.CreateVnPayTransaction(request);
            return Ok(BaseResponse.OkResponseDto(paymentUrl));
        }

        [AllowAnonymous]
        [HttpGet(WebApiEndpoint.ShipmentEndpoint.VnpayExecute)]
        public async Task<IActionResult> VnPayExecute([FromQuery] VnPayCallbackModel model)
        {
            var result = await transactionService.ExecuteVnPayPayment(model);
            // Redirect to the payment result page
            if (string.IsNullOrEmpty(result))
            {
                return Ok(BaseResponse.OkResponseDto("Update shipment success!", null));
            }

            return Redirect(result);
        }

        [AllowAnonymous]
        [HttpPost(WebApiEndpoint.ShipmentEndpoint.GetTotalPrice)]
        public async Task<IActionResult> GetTotalPrice([FromBody] TotalPriceCalcRequest request)
        {
            var result = await shipmentService.GetItineraryAndTotalPrice(request);
            return Ok(BaseResponse.OkResponseDto(result));
        }
    }
}
