using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.PresentationDtos;
using HerreraSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HerreraSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PresentationsController : ControllerBase
    {
        private readonly IPresentationService _presentationService;

        public PresentationsController(IPresentationService presentationService)
        {
            _presentationService = presentationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] PaginationParams paginationParams)
        {
            var data = await _presentationService.GetAllAsync(paginationParams);

            return Ok(ApiResponse<PagedResponse<PresentationDto>>.Ok(data));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _presentationService.GetByIdAsync(id);

            if (!result.Success)
                return NotFound(ApiResponse<PresentationDto>.Fail(result.ErrorMessage!));

            return Ok(ApiResponse<PresentationDto>.Ok(result.Data!));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreatePresentationDto dto)
        {
            var result = await _presentationService.CreateAsync(dto);

            if (!result.Success)
                return BadRequest(ApiResponse<PresentationDto>.Fail(result.ErrorMessage!));

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Data!.Id },
                ApiResponse<PresentationDto>.Ok(result.Data, "Presentación creada exitosamente"));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdatePresentationDto dto)
        {
            var result = await _presentationService.UpdateAsync(id, dto);

            if (!result.Success)
            {
                if (result.ErrorMessage!.Contains("no encontrada"))
                    return NotFound(ApiResponse<object>.Fail(result.ErrorMessage));

                return BadRequest(ApiResponse<object>.Fail(result.ErrorMessage));
            }

            return Ok(ApiResponse<object>.Ok(null!, "Presentación actualizada exitosamente"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _presentationService.DeleteAsync(id);

            if (!result.Success)
            {
                if (result.ErrorMessage!.Contains("no encontrada"))
                    return NotFound(ApiResponse<object>.Fail(result.ErrorMessage));

                return BadRequest(ApiResponse<object>.Fail(result.ErrorMessage));
            }

            return Ok(ApiResponse<object>.Ok(null!, "Presentación eliminada exitosamente"));
        }
    }
}