using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.PresentationDtos;

namespace HerreraSystem.Application.Interfaces.Services
{
    public interface IPresentationService
    {
        Task<PagedResponse<PresentationDto>> GetAllAsync(PaginationParams paginationParams);
        Task<ServiceResult<PresentationDto>> GetByIdAsync(int id);
        Task<ServiceResult<PresentationDto>> CreateAsync(CreatePresentationDto dto);
        Task<ServiceResult<bool>> UpdateAsync(int id, UpdatePresentationDto dto);
        Task<ServiceResult<bool>> DeleteAsync(int id);
    }
}