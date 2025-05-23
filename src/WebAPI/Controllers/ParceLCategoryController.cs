using MetroShip.Service.ApiModels.ParcelCategory;
using MetroShip.Service.ApiModels;
using MetroShip.Service.Interfaces;
using MetroShip.Utility.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MetroShip.Service.ApiModels.PaginatedList;

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

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll([FromQuery] bool? isActive, [FromQuery] PaginatedListRequest request)
        {
            var result = await _parcelCategoryService.GetAllAsync(isActive,request);
            return Ok(BaseResponse.OkResponseDto(result));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create([FromBody] ParcelCategoryCreateRequest request)
        {
            var result = await _parcelCategoryService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, BaseResponse.OkResponseDto(result));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ParcelCategoryUpdateRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest(BaseResponse.NotFoundResponseDto("Invalid ID in request body."));
            }

            await _parcelCategoryService.UpdateAsync(request);
            return Ok(BaseResponse.OkResponseDto(ResponseMessageConstantsParcelCategory.UPDATE_SUCCESS));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _parcelCategoryService.DeleteAsync(id);
            return Ok(BaseResponse.OkResponseDto(ResponseMessageConstantsParcelCategory.DELETE_SUCCESS));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _parcelCategoryService.GetByIdAsync(id);
            return Ok(BaseResponse.OkResponseDto(result));
        }
    }
}