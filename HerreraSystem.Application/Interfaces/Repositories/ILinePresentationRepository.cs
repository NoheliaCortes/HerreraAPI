using HerreraSystem.Application.DTOs.LinePresentationDtos;
using HerreraSystem.Application.DTOs.PresentationDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Interfaces.Repositories
{
    public interface ILinePresentationRepository
    {
        Task<List<LinePresentationDto>> GetAllAsync();

        Task<LinePresentationDto?> GetByIdAsync(int id);

        Task<LinePresentationDto?> CreateAsync(CreateLinePresentationDto dto);

        Task<bool> DeleteAsync(int id);

        Task<List<PresentationDto>> GetPresentationsByLineAsync(int lineId);
    }
}
