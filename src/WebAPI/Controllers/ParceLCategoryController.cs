using MetroShip.Service.ApiModels.ParcelCategory;
using MetroShip.Service.ApiModels;
using MetroShip.Service.Interfaces;
using MetroShip.Utility.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Utility.Enums;

namespace MetroShip.WebAPI.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class ParcelCategoryController : ControllerBase
    {
        private readonly IParcelCategoryService _parcelCategoryService;

        public ParcelCategoryController(IParcelCategoryService parcelCategoryService)
        {
            _parcelCategoryService = parcelCategoryService;
        }

        [HttpGet(WebApiEndpoint.ParcelCategory.GetCategories)]
        public async Task<IActionResult> GetAll(
            [FromQuery] bool? isActive,
            [FromQuery] PaginatedListRequest request,
            [FromQuery] bool isIncludeAllCategoryInsurances = false)
        {
            var result = await _parcelCategoryService.GetAllAsync(
                isActive, request, isIncludeAllCategoryInsurances);
            return Ok(BaseResponse.OkResponseDto(result));
        }

        [HttpPost(WebApiEndpoint.ParcelCategory.CreateCategory)]
        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        public async Task<IActionResult> Create([FromBody] ParcelCategoryCreateRequest request)
        {
            var result = await _parcelCategoryService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, BaseResponse.OkResponseDto(result));
        }

        [HttpPut(WebApiEndpoint.ParcelCategory.UpdateCategory)]
        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        public async Task<IActionResult> Update([FromBody] ParcelCategoryUpdateRequest request)
        {
            await _parcelCategoryService.UpdateAsync(request);
            return Ok(BaseResponse.OkResponseDto(ResponseMessageConstantsParcelCategory.UPDATE_SUCCESS));
        }

        [HttpDelete(WebApiEndpoint.ParcelCategory.DeleteCategory)]
        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _parcelCategoryService.DeleteAsync(id);
            return Ok(BaseResponse.OkResponseDto(ResponseMessageConstantsParcelCategory.DELETE_SUCCESS));
        }

        [HttpGet(WebApiEndpoint.ParcelCategory.GetCategory)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _parcelCategoryService.GetByIdAsync(id);
            return Ok(BaseResponse.OkResponseDto(result));
        }
    }
}