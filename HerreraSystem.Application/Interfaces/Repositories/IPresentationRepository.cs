using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.PresentationDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Interfaces.Repositories
{
    public interface IPresentationRepository
    {
        Task<PagedResponse<PresentationDto>> GetAllAsync(PaginationParams paginationParams);
        Task<PresentationDto?> GetByIdAsync(int id);
        Task<PresentationDto> CreateAsync(CreatePresentationDto dto);
        Task<bool> UpdateAsync(int id, UpdatePresentationDto dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsByNameAsync(string presentationName);
        Task<bool> ExistsByNameAsync(string presentationName, int excludeId);

    }
}
