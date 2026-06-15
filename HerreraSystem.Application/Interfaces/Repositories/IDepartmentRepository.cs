using HerreraSystem.Application.DTOs.DepartmentDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Interfaces.Repositories
{
    public interface IDepartmentRepository
    {
        Task<IReadOnlyList<DepartmentDto>> GetAllAsync();
        Task<DepartmentDto?> GetByIdAsync(int id);
    }
}
