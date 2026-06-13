using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.DepartmentDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Services
{
    public class DepartmentService:IDepartmentService
    {
        private readonly IDepartmentRepository _departmentRepository;

        public DepartmentService(IDepartmentRepository departmentRepository)
        {
            _departmentRepository = departmentRepository;
        }

        public async Task<IReadOnlyList<DepartmentDto>> GetAllAsync()
        {
            return await _departmentRepository.GetAllAsync();
        }

        public async Task<ServiceResult<DepartmentDto>> GetByIdAsync(int id)
        {
            var department = await _departmentRepository.GetByIdAsync(id);

            if (department is null)
                return ServiceResult<DepartmentDto>
                    .Fail($"Departamento con Id {id} no encontrado");

            return ServiceResult<DepartmentDto>.Ok(department);
        }
    }
}
