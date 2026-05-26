using HerreraSystem.Application.DTOs.PresentationDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Interfaces.Repositories
{
    public interface IPresentationRepository
    {
        Task<List<PresentationDto>> GetAllAsync();
        Task<PresentationDto?> GetByIdAsync(int id);
        Task<PresentationDto> CreateAsync(CreatePresentationDto dto);
        Task<bool> UpdateAsync(int id, UpdatePresentationDto dto);
        Task<bool> DeleteAsync(int id);

    }
}
