using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.FlavorDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Interfaces.Services
{
    public interface IFlavorService
    {
        Task<PagedResponse<FlavorDto>> GetAllAsync(PaginationParams paginationParams);
        Task<ServiceResult<FlavorDto>> GetByIdAsync(int id);
        Task<ServiceResult<FlavorDto>> CreateAsync(CreateFlavorDto dto);
        Task<ServiceResult<bool>> UpdateAsync(int id, UpdateFlavorDto dto);
        Task<ServiceResult<bool>> DeleteAsync(int id);
    }
}
