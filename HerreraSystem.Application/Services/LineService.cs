using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.LineDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Application.Interfaces.Services;

namespace HerreraSystem.Application.Services
{
    public class LineService : ILineService
    {
        private readonly ILineRepository _lineRepository;

        public LineService(ILineRepository lineRepository)
        {
            _lineRepository = lineRepository;
        }

        public async Task<PagedResponse<LineDto>> GetAllAsync(PaginationParams paginationParams)
        {
            return await _lineRepository.GetAllAsync(paginationParams);
        }

        public async Task<ServiceResult<LineDto>> GetByIdAsync(int id)
        {
            var line = await _lineRepository.GetByIdAsync(id);
            if (line == null)
                return ServiceResult<LineDto>.Fail("Línea no encontrada.");

            return ServiceResult<LineDto>.Ok(line);
        }

        public async Task<ServiceResult<LineDto>> CreateAsync(CreateLineDto dto)
        {
            var newLine = await _lineRepository.CreateAsync(dto);
            return ServiceResult<LineDto>.Ok(newLine);
        }

        public async Task<ServiceResult<bool>> UpdateAsync(int id, UpdateLineDto dto)
        {
            var success = await _lineRepository.UpdateAsync(id, dto);
            if (!success)
                return ServiceResult<bool>.Fail("Línea no encontrada o no se pudo actualizar.");

            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            var success = await _lineRepository.DeleteAsync(id);
            if (!success)
                return ServiceResult<bool>.Fail("Línea no encontrada o no se pudo eliminar.");

            return ServiceResult<bool>.Ok(true);
        }
    }
}