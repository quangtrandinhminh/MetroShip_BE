using MetroShip.Service.ApiModels;
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
    [ApiController]
    [Route("api/shipments")]
    public class ShipmentController(
        IShipmentService shipmentService,
        ITransactionService transactionService
        ) : ControllerBase
    {
        private readonly IList<EnumResponse> _enumResponses = EnumHelper.GetEnumList<ShipmentStatusEnum>();

        [Authorize]
        [HttpGet(WebApiEndpoint.ShipmentEndpoint.GetShipments)]
        public async Task<IActionResult> Get(
            [FromQuery] PaginatedListRequest request, 
            [FromQuery] ShipmentFilterRequest filterRequest,
            [FromQuery] OrderByRequest orderByRequest
            )
        {
            var response = await shipmentService.GetAllShipments(request, filterRequest, orderByRequest
                );
            return Ok(BaseResponse.OkResponseDto(response, _enumResponses));
        }

        [Authorize]
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

        /*[Authorize(Roles = nameof(UserRoleEnum.Customer))]
        [HttpPost(WebApiEndpoint.ShipmentEndpoint.GetShipmentItinerary)]
        public async Task<ActionResult<List<ItineraryResponse>>> GetPath(
            [FromBody]  BestPathRequest request)
        {
            var result = await shipmentService.FindPathAsync(request);
            return Ok(BaseResponse.OkResponseDto(result));
        }*/

        [Authorize(Roles = nameof(UserRoleEnum.Customer))]
        [HttpPost(WebApiEndpoint.ShipmentEndpoint.CreateTransactionVnPay)]
        public async Task<IActionResult> CreateVnPayUrl([FromBody] TransactionRequest request)
        {
            var paymentUrl = await transactionService.CreateVnPayTransaction(request);
            return Ok(BaseResponse.OkResponseDto(paymentUrl));
        }

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

        [Authorize(Roles = nameof(UserRoleEnum.Customer))]
        [HttpPost(WebApiEndpoint.ShipmentEndpoint.GetTotalPrice)]
        public async Task<IActionResult> GetTotalPrice([FromBody] TotalPriceCalcRequest request)
        {
            var result = await shipmentService.GetItineraryAndTotalPrice(request);
            return Ok(BaseResponse.OkResponseDto(result));
        }

        /*[HttpGet(WebApiEndpoint.ShipmentEndpoint.GetShipmentsByLineAndDate)]
        public async Task<IActionResult> GetShipmentByLineAndDate(
            [FromRoute] string lineCode,
            [FromRoute] DateTimeOffset date,
                [FromQuery] PaginatedListRequest request,
                string? regionCode,
                ShiftEnum? shift
            )
        {
            var response = await shipmentService.GetShipmentByLineAndDate(request, lineCode, date, regionCode, shift);
            return Ok(BaseResponse.OkResponseDto(response, _enumResponses));
        }*/

        /*[HttpGet(WebApiEndpoint.ShipmentEndpoint.GetAvailableTimeSlots)]
        public async Task<IActionResult> GetAvailableTimeSlots([FromQuery] ShipmentAvailableTimeSlotsRequest request)
        {
            var response = await shipmentService.CheckAvailableTimeSlotsAsync(
                request.ShipmentId, request.MaxAttempts.Value);
            return Ok(BaseResponse.OkResponseDto(response));
        }*/

        [Authorize(Roles = nameof(UserRoleEnum.Staff))]
        [HttpPost(WebApiEndpoint.ShipmentEndpoint.PickUpShipment)]
        public async Task<IActionResult> PickUpShipment([FromBody] ShipmentPickUpRequest request)
        {
            await shipmentService.PickUpShipment(request);
            return Ok(BaseResponse.OkResponseDto(ResponseMessageShipment.PICKED_UP_SUCCESS, null));
        }

        [Authorize(Roles = nameof(UserRoleEnum.Staff))]
        [HttpPost(WebApiEndpoint.ShipmentEndpoint.RejectShipment)]
        public async Task<IActionResult> RejectShipment([FromBody] ShipmentRejectRequest request)
        {
            await shipmentService.RejectShipment(request);
            return Ok(BaseResponse.OkResponseDto(ResponseMessageShipment.REJECTED_SUCCESS, null));
        }

        [Authorize(Roles = $"{nameof(UserRoleEnum.Staff)},{nameof(UserRoleEnum.Customer)}")]
        [HttpGet(WebApiEndpoint.ShipmentEndpoint.GetLocation)]
        public async Task<IActionResult> GetShipmentLocation(string trackingCode)
        {
            var data = await shipmentService.GetShipmentLocationAsync(trackingCode);
            return Ok(data);
        }

        [Authorize(Roles = nameof(UserRoleEnum.Staff))]
        [HttpPut(WebApiEndpoint.ShipmentEndpoint.UpdateStatusAtStation)]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateShipmentStatusRequest request)
        {
            var staffId = User?.Identity?.Name ?? "unknown";
            await shipmentService.UpdateShipmentStatusByStationAsync(request, staffId);
            return Ok();
        }
    }
}
