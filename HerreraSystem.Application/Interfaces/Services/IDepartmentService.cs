using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.DepartmentDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Interfaces.Services
{
    public interface IDepartmentService
    {
        Task<IReadOnlyList<DepartmentDto>> GetAllAsync();
        Task<ServiceResult<DepartmentDto>> GetByIdAsync(int id);

    }
}
