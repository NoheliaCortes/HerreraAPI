using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.PresentationDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HerreraSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PresentationsController : ControllerBase
    {
        private readonly IPresentationRepository _presentationRepository;

        public PresentationsController(IPresentationRepository presentationRepository)
        {
            _presentationRepository = presentationRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _presentationRepository.GetAllAsync();
            return Ok(ApiResponse<List<PresentationDto>>.Ok(data));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var presentation = await _presentationRepository.GetByIdAsync(id);
            if (presentation is null)
                return NotFound(ApiResponse<PresentationDto>.Fail($"Presentación con Id {id} no encontrada"));

            return Ok(ApiResponse<PresentationDto>.Ok(presentation));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreatePresentationDto dto)
        {
            var created = await _presentationRepository.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById),
                new { id = created.Id },
                ApiResponse<PresentationDto>.Ok(created, "Presentación creada exitosamente"));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdatePresentationDto dto)
        {
            var updated = await _presentationRepository.UpdateAsync(id, dto);
            if (!updated)
                return NotFound(ApiResponse<PresentationDto>.Fail($"Presentación con Id {id} no encontrada"));

            return Ok(ApiResponse<object>.Ok(null!, "Presentación actualizada exitosamente"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _presentationRepository.DeleteAsync(id);
            if (!deleted)
                return NotFound(ApiResponse<PresentationDto>.Fail($"Presentación con Id {id} no encontrada"));

            return Ok(ApiResponse<object>.Ok(null!, "Presentación eliminada exitosamente"));
        }
    }
}
