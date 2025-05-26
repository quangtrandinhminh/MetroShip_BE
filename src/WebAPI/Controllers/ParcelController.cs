using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Parcel;
using MetroShip.Service.Interfaces;
using MetroShip.Utility.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MetroShip.WebAPI.Controllers
{
    [ApiController]
    public class ParcelController : ControllerBase
    {
        private readonly IParcelService _parcelService;

        public ParcelController(IParcelService parcelService)
        {
            _parcelService = parcelService;
        }
        /// <summary>
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
        }

        [HttpGet(WebApiEndpoint.ParcelEndpoint.GetParcels)]
        public async Task<ActionResult> GetAll([FromQuery] PaginatedListRequest request)
        {
            var result = await _parcelService.GetAllParcels(request);
            return Ok(BaseResponse.OkResponseDto(result));
        }
    }
}
