using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.PresentationDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Application.Interfaces.Services;

namespace HerreraSystem.Application.Services
{
    public class PresentationService : IPresentationService
    {
        private readonly IPresentationRepository _presentationRepository;

        public PresentationService(IPresentationRepository presentationRepository)
        {
            _presentationRepository = presentationRepository;
        }

        public async Task<PagedResponse<PresentationDto>> GetAllAsync(PaginationParams paginationParams)
        {
            return await _presentationRepository.GetAllAsync(paginationParams);
        }

        public async Task<ServiceResult<PresentationDto>> GetByIdAsync(int id)
        {
            var presentation = await _presentationRepository.GetByIdAsync(id);
            if (presentation == null)
                return ServiceResult<PresentationDto>.Fail("Presentación no encontrada.");

            return ServiceResult<PresentationDto>.Ok(presentation);
        }

        public async Task<ServiceResult<PresentationDto>> CreateAsync(CreatePresentationDto dto)
        {
            var newPresentation = await _presentationRepository.CreateAsync(dto);
            return ServiceResult<PresentationDto>.Ok(newPresentation);
        }

        public async Task<ServiceResult<bool>> UpdateAsync(int id, UpdatePresentationDto dto)
        {
            var success = await _presentationRepository.UpdateAsync(id, dto);
            if (!success)
                return ServiceResult<bool>.Fail("Presentación no encontrada o no se pudo actualizar.");

            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            var success = await _presentationRepository.DeleteAsync(id);
            if (!success)
                return ServiceResult<bool>.Fail("Presentación no encontrada o no se pudo eliminar.");

            return ServiceResult<bool>.Ok(true);
        }
    }
}