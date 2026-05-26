using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.LineDtos;
using HerreraSystem.Application.DTOs.PresentationDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HerreraSystem.API.Controllers
{

        [ApiController]
        [Route("api/[controller]")]
        public class LinesController : ControllerBase
        {
        private readonly ILineRepository _lineRepository;
        private readonly ILinePresentationRepository _linePresentationRepository;

        public LinesController(ILineRepository lineRepository)
        {
            _lineRepository = lineRepository;
            _linePresentationRepository = new LinePresentationRepository(new HerreraSystem.Infrastructure.Data.HerreraSystemContext());

        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _lineRepository.GetAllAsync();
            return Ok(ApiResponse<List<LineDto>>.Ok(data));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var line = await _lineRepository.GetByIdAsync(id);
            if (line is null)
                return NotFound(ApiResponse<LineDto>.Fail($"Línea con Id {id} no encontrada"));

            return Ok(ApiResponse<LineDto>.Ok(line));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateLineDto dto)
        {
            var created = await _lineRepository.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById),
                new { id = created.Id },
                ApiResponse<LineDto>.Ok(created, "Línea creada exitosamente"));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateLineDto dto)
        {
            var updated = await _lineRepository.UpdateAsync(id, dto);
            if (!updated)
                return NotFound(ApiResponse<LineDto>.Fail($"Línea con Id {id} no encontrada"));

            return Ok(ApiResponse<object>.Ok(null!, "Línea actualizada exitosamente"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _lineRepository.DeleteAsync(id);
            if (!deleted)
                return NotFound(ApiResponse<LineDto>.Fail($"Línea con Id {id} no encontrada"));

            return Ok(ApiResponse<object>.Ok(null!, "Línea eliminada exitosamente"));
        }


        [HttpGet("{lineId}/presentations")]
        public async Task<IActionResult> GetPresentations(int lineId)
        {
            var data = await _linePresentationRepository
                .GetPresentationsByLineAsync(lineId);

            return Ok(ApiResponse<List<PresentationDto>>
                .Ok(data));
        }


    }


}
