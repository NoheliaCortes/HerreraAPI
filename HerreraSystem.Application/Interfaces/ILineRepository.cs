using HerreraSystem.Application.DTOs.LineDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Interfaces
{
    public interface ILineRepository
    {
        Task<List<LineDto>> GetAllAsync();
        Task<LineDto?> GetByIdAsync(int id);
        Task<LineDto> CreateAsync(CreateLineDto dto);
        Task<bool> UpdateAsync(int id, UpdateLineDto dto);
        Task<bool> DeleteAsync(int id);

    }
}
