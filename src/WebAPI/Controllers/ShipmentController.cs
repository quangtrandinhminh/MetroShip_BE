using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.Graph;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Shipment;
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
    public class ShipmentController(IShipmentService shipmentService) : ControllerBase
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
            await shipmentService.BookShipment(request);
            return Created(nameof(Create), BaseResponse.OkResponseDto(ResponseMessageShipment.SHIPMENT_CREATE_SUCCESS));
        }

        [Authorize(Roles = nameof(UserRoleEnum.Customer))]
        [HttpPost(WebApiEndpoint.ShipmentEndpoint.GetShipmentItinerary)]
        public async Task<ActionResult<List<ItineraryResponse>>> GetPath(
            [FromBody]  BestPathRequest request)
        {
            var result = await shipmentService.FindPathAsync(request);
            return Ok(BaseResponse.OkResponseDto(result));
        }

    }
}
