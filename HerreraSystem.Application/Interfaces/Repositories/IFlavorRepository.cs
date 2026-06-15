using HerreraSystem.Application.DTOs.FlavorDtos;
using System;
using System.Collections.Generic;
using System.Text;
using HerreraSystem.Application.Common;

namespace HerreraSystem.Application.Interfaces.Repositories
{
    public interface IFlavorRepository
    {
        Task<PagedResponse<FlavorDto>> GetAllAsync(PaginationParams paginationParams);
        Task<FlavorDto?> GetByIdAsync(int id);
        Task<FlavorDto> CreateAsync(CreateFlavorDto dto);
        Task<bool> UpdateAsync(int id, UpdateFlavorDto dto);
        Task<bool> DeleteAsync(int id);

        Task<bool> ExistsAsync(string flavorName, int? excludeId = null);
        Task<bool> HasProductsAsync(int flavorId);

    }
}
