using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Parcel;
using MetroShip.Service.BusinessModels;
using MetroShip.Service.Helpers;
using MetroShip.Service.Interfaces;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MetroShip.WebAPI.Controllers
{
    [ApiController]
    [Route("api/parcels")]
    public class ParcelController : ControllerBase
    {
        private readonly IParcelService _parcelService;
        private readonly IList<EnumResponse> _enumResponses = EnumHelper.GetEnumList<ParcelStatusEnum>();

        public ParcelController(IParcelService parcelService)
        {
            _parcelService = parcelService;
        }
        /*/// <summary>
        /// Tính toán thông tin kiện hàng
        /// </summary>
        [HttpPost("calculate-parcel")]
        public ActionResult<CreateParcelResponse> CalculateParcelInfo([FromBody] ParcelRequest request)
        {
            var result = _parcelService.CalculateParcelInfo(request);
            return Ok(result);
        }

        /// <summary>
        /// Tính toán chi phí vận chuyển
        /// </summary>
        [HttpPost("price")]
        public ActionResult<decimal> CalculateShippingCost([FromBody] ParcelRequest request, [FromQuery] double distanceKm, [FromQuery] decimal pricePerKm)
        {
            var cost = _parcelService.CalculateShippingCost(request, distanceKm, pricePerKm);
            return Ok(cost);
        }*/

        [HttpGet(WebApiEndpoint.ParcelEndpoint.GetParcels)]
        public async Task<ActionResult> GetAll([FromQuery] PaginatedListRequest request)
        {
            var result = await _parcelService.GetAllParcels(request);
            return Ok(BaseResponse.OkResponseDto(result, _enumResponses));
        }

        [HttpGet("qrcode/{parcelTrackingCode}")]
        public async Task<IActionResult> GetParcelQRCode([FromRoute] string parcelTrackingCode)
        {
            var qrCode = TrackingCodeGenerator.GenerateQRCode(parcelTrackingCode);
            return Ok(qrCode);
        }

        /*[HttpPost(WebApiEndpoint.ParcelEndpoint.ConfirmParcel)]
        [Authorize(Roles = nameof(UserRoleEnum.Staff))]
        public async Task<IActionResult> ConfirmParcelAsync([FromRoute] Guid parcelId)
        {
            await _parcelService.ConfirmParcelAsync(parcelId);
            return Ok(BaseResponse.OkResponseDto("Parcel confirmation processed successfully.", null));
        }

        [HttpPost(WebApiEndpoint.ParcelEndpoint.RejectParcel)]
        [Authorize(Roles = nameof(UserRoleEnum.Staff))]
        public async Task<IActionResult> RejectParcelAsync([FromBody] ParcelRejectRequest request)
        {
            await _parcelService.RejectParcelAsync(request);
            return Ok(BaseResponse.OkResponseDto("Parcel rejection processed successfully.", null));
        }*/
    }
}
