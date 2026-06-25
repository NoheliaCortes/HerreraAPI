using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.FlavorDtos;
using HerreraSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Interfaces.Repositories
{
    public interface IFlavorRepository
    {
        Task<PagedResponse<FlavorDto>> GetAllAsync(PaginationParams paginationParams);
        Task<FlavorDto?> GetByIdAsync(int id);
        Task<FlavorDto> CreateAsync(CreateFlavorDto dto, string? imageUrl);
        Task<bool> UpdateAsync(int id, UpdateFlavorDto dto, string? imageUrl);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(string flavorName, int? excludeId = null);
        Task<bool> HasProductsAsync(int flavorId);

    }
}
