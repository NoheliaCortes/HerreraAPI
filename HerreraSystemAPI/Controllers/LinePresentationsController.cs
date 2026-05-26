using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.LinePresentationDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HerreraSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LinePresentationsController : ControllerBase
    {
        private readonly ILinePresentationRepository _repository;

        public LinePresentationsController(ILinePresentationRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _repository.GetAllAsync();

            return Ok(ApiResponse<List<LinePresentationDto>>
                .Ok(data));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _repository.GetByIdAsync(id);

            if (data is null)
            {
                return NotFound(ApiResponse<LinePresentationDto>
                    .Fail($"Relación con Id {id} no encontrada"));
            }

            return Ok(ApiResponse<LinePresentationDto>
                .Ok(data));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateLinePresentationDto dto)
        {
            var created = await _repository.CreateAsync(dto);

            if (created is null)
            {
                return BadRequest(ApiResponse<object>
                    .Fail("La línea o presentación no existe, o la combinación ya está registrada"));
            }

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                ApiResponse<LinePresentationDto>
                    .Ok(created, "Relación creada exitosamente"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _repository.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(ApiResponse<object>
                    .Fail($"Relación con Id {id} no encontrada"));
            }

            return Ok(ApiResponse<object>
                .Ok(null!, "Relación eliminada exitosamente"));
        }




    }
}
