using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.LineDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Interfaces.Repositories
{
    public interface ILineRepository
    {
        Task<PagedResponse<LineDto>> GetAllAsync(PaginationParams paginationParams);
        Task<LineDto?> GetByIdAsync(int id);
        Task<LineDto> CreateAsync(CreateLineDto dto);
        Task<bool> UpdateAsync(int id, UpdateLineDto dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsByNameAsync(string lineName);
        Task<bool> ExistsByNameAsync(string lineName, int excludeId);

    }
}
