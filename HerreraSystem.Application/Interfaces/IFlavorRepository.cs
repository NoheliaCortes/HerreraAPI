using HerreraSystem.Application.DTOs.FlavorDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Interfaces
{
    public interface IFlavorRepository
    {
        Task<List<FlavorDto>> GetAllAsync();
        Task<FlavorDto?> GetByIdAsync(int id);
        Task<FlavorDto> CreateAsync(CreateFlavorDto dto);
        Task<bool> UpdateAsync(int id, UpdateFlavorDto dto);
        Task<bool> DeleteAsync(int id);

    }
}
