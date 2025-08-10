using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.Graph;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Shipment;
using MetroShip.Service.ApiModels.Transaction;
using MetroShip.Service.ApiModels.VNPay;
using MetroShip.Service.BusinessModels;
using MetroShip.Service.Helpers;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Services;
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
        public async Task<IActionResult> GetShipments([FromQuery] PaginatedListRequest request, [FromQuery] ShipmentFilterRequest? filterRequest,[FromQuery] string? searchKeyword, 
            [FromQuery] DateTimeOffset? createdFrom, [FromQuery] DateTimeOffset? createdTo, [FromQuery] OrderByRequest? orderByRequest)
        {
            var result = await shipmentService.GetAllShipmentsAsync(
                request, filterRequest, searchKeyword, createdFrom, createdTo, orderByRequest
            );
            return Ok(BaseResponse.OkResponseDto(result));
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
            var (shipmentId,trackingCode) = await shipmentService.BookShipment(request);
            return Created(nameof(Create), 
                BaseResponse.OkResponseDto(new { ShipmentId = shipmentId, TrackingCode = trackingCode }));
        }

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
        // feedback
        [Authorize(Roles = nameof(UserRoleEnum.Customer))]
        [HttpPost(WebApiEndpoint.ShipmentEndpoint.FeedbackShipment)]
        public async Task<IActionResult> FeedbackShipment([FromBody] ShipmentFeedbackRequest request)
        {
            await shipmentService.FeedbackShipment(request);
            return Ok(BaseResponse.OkResponseDto(ResponseMessageShipment.FEEDBACK_SUCCESS, null));
        }

        [Authorize(Roles = nameof(UserRoleEnum.Staff))]
        [HttpPost(WebApiEndpoint.ShipmentEndpoint.CompleteShipment)]
        public async Task<IActionResult> CompleteShipment([FromBody] ShipmentPickUpRequest request)
        {
            await shipmentService.CompleteShipment(request);
            return Ok(BaseResponse.OkResponseDto(ResponseMessageShipment.COMPLETED_SUCCESS, null));
        }

        //[Authorize(Roles = $"{nameof(UserRoleEnum.Staff)},{nameof(UserRoleEnum.Customer)}")]
        [HttpGet(WebApiEndpoint.ShipmentEndpoint.GetLocation)]
        public async Task<IActionResult> GetShipmentLocation(string trackingCode)
        {
            var data = await shipmentService.GetShipmentLocationAsync(trackingCode);
            return Ok(data);
        }

        [Authorize(Roles = nameof(UserRoleEnum.Staff))]
        [HttpPut(WebApiEndpoint.ShipmentEndpoint.UnloadingAtStation)]
        public async Task<IActionResult> UpdateStatusUnload([FromBody] UpdateShipmentStatusRequest request)
        {
            var staffId = User?.Identity?.Name ?? "unknown";
            var result = await shipmentService.UpdateShipmentStatusAsync(request, ShipmentStatusEnum.UnloadingAtStation, staffId);
            return Ok(result);
        }

        [Authorize(Roles = nameof(UserRoleEnum.Staff))]
        [HttpPut(WebApiEndpoint.ShipmentEndpoint.StorageInWarehouse)]
        public async Task<IActionResult> UpdateStatusStorage([FromBody] UpdateShipmentStatusRequest request)
        {
            var staffId = User?.Identity?.Name ?? "unknown";
            var result = await shipmentService.UpdateShipmentStatusAsync(request, ShipmentStatusEnum.StorageInWarehouse, staffId);
            return Ok(result);
        }

        //[Authorize(Roles = nameof(UserRoleEnum.Staff))]
        [HttpPost(WebApiEndpoint.ShipmentEndpoint.AssignTrainToShipment)]
        public async Task<IActionResult> AssignTrainToShipment(string trackingCode, string trainId)
        {
            var result = await shipmentService.AssignTrainToShipmentAsync(trackingCode, trainId);
            return Ok(BaseResponse.OkResponseDto(result));
        }

        [Authorize(Roles = nameof(UserRoleEnum.Customer))]
        [HttpPost(WebApiEndpoint.ShipmentEndpoint.CancelShipment)]
        public async Task<IActionResult> CancelShipment([FromBody] ShipmentRejectRequest request)
        {
            await shipmentService.CancelShipment(request);
            return Ok(BaseResponse.OkResponseDto(ResponseMessageShipment.CANCELLED_SUCCESS, null));
        }
    }
}
