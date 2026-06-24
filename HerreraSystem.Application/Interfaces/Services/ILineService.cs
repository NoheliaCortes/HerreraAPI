using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.LineDtos;

namespace HerreraSystem.Application.Interfaces.Services
{
    public interface ILineService
    {
        Task<PagedResponse<LineDto>> GetAllAsync(PaginationParams paginationParams);
        Task<ServiceResult<LineDto>> GetByIdAsync(int id);
        Task<ServiceResult<LineDto>> CreateAsync(CreateLineDto dto);
        Task<ServiceResult<bool>> UpdateAsync(int id, UpdateLineDto dto);
        Task<ServiceResult<bool>> DeleteAsync(int id);
    }
}