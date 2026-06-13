using System;
using System.Collections.Generic;
using System.Text;
using HerreraSystem.Application.DTOs.MunicipalityDtos;

namespace HerreraSystem.Application.Interfaces.Repositories
{
    public interface IMunicipalityRepository
    {
        Task<IReadOnlyList<MunicipalityDto>> GetAllAsync();
        Task<IReadOnlyList<MunicipalityDto>> GetByDepartmentAsync(int departmentId);
        Task<MunicipalityDto?> GetByIdAsync(int id);
    }
}
