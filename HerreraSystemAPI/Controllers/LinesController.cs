using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.LineDtos;
using HerreraSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HerreraSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LinesController : ControllerBase
    {
        private readonly ILineService _lineService;

        public LinesController(ILineService lineService)
        {
            _lineService = lineService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] PaginationParams paginationParams)
        {
            var data = await _lineService.GetAllAsync(paginationParams);

            return Ok(ApiResponse<PagedResponse<LineDto>>.Ok(data));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _lineService.GetByIdAsync(id);

            if (!result.Success)
                return NotFound(ApiResponse<LineDto>.Fail(result.ErrorMessage!));

            return Ok(ApiResponse<LineDto>.Ok(result.Data!));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateLineDto dto)
        {
            var result = await _lineService.CreateAsync(dto);

            if (!result.Success)
                return BadRequest(ApiResponse<LineDto>.Fail(result.ErrorMessage!));

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Data!.Id },
                ApiResponse<LineDto>.Ok(result.Data, "Línea creada exitosamente"));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateLineDto dto)
        {
            var result = await _lineService.UpdateAsync(id, dto);

            if (!result.Success)
            {
                if (result.ErrorMessage!.Contains("no encontrada"))
                    return NotFound(ApiResponse<object>.Fail(result.ErrorMessage));

                return BadRequest(ApiResponse<object>.Fail(result.ErrorMessage));
            }

            return Ok(ApiResponse<object>.Ok(null!, "Línea actualizada exitosamente"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _lineService.DeleteAsync(id);
            
            if (!result.Success)
            {
                if (result.ErrorMessage!.Contains("no encontrada"))
                    return NotFound(ApiResponse<object>.Fail(result.ErrorMessage));

                return BadRequest(ApiResponse<object>.Fail(result.ErrorMessage));
            }

            return Ok(ApiResponse<object>.Ok(null!, "Línea eliminada exitosamente"));
        }
    }
}