using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.MunicipalityDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Interfaces.Services
{
    public interface IMunicipalityService
    {
        Task<IReadOnlyList<MunicipalityDto>> GetAllAsync();
        Task<ServiceResult<IReadOnlyList<MunicipalityDto>>> GetByDepartmentAsync(int departmentId);
        Task<ServiceResult<MunicipalityDto>> GetByIdAsync(int id);
    }
}
