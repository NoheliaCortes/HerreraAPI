using HerreraSystem.Application.DTOs.MunicipalityDtos;
using HerreraSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;
using HerreraSystem.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HerreraSystem.Infrastructure.Repositories
{
    public class MunicipalityRepository: IMunicipalityRepository
    {
        private readonly HerreraSystemContext _context;

        public MunicipalityRepository(HerreraSystemContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<MunicipalityDto>> GetAllAsync()
        {
            return await _context.Municipalities
                .AsNoTracking()
                .Include(m => m.Department)
                .Where(m => m.IsActive == true)
                .OrderBy(m => m.Department.DepartmentName)
                    .ThenBy(m => m.MunicipalityName)
                .Select(m => new MunicipalityDto
                {
                    Id = m.Id,
                    DepartmentId = m.DepartmentId,
                    DepartmentName = m.Department.DepartmentName,
                    MunicipalityName = m.MunicipalityName,
                    IsActive = m.IsActive
                })
                .ToListAsync();
        }

        public async Task<IReadOnlyList<MunicipalityDto>> GetByDepartmentAsync(int departmentId)
        {
            return await _context.Municipalities
                .AsNoTracking()
                .Include(m => m.Department)
                .Where(m => m.DepartmentId == departmentId && m.IsActive == true)
                .OrderBy(m => m.MunicipalityName)
                .Select(m => new MunicipalityDto
                {
                    Id = m.Id,
                    DepartmentId = m.DepartmentId,
                    DepartmentName = m.Department.DepartmentName,
                    MunicipalityName = m.MunicipalityName,
                    IsActive = m.IsActive
                })
                .ToListAsync();
        }

        public async Task<MunicipalityDto?> GetByIdAsync(int id)
        {
            return await _context.Municipalities
                .AsNoTracking()
                .Include(m => m.Department)
                .Where(m => m.Id == id)
                .Select(m => new MunicipalityDto
                {
                    Id = m.Id,
                    DepartmentId = m.DepartmentId,
                    DepartmentName = m.Department.DepartmentName,
                    MunicipalityName = m.MunicipalityName,
                    IsActive = m.IsActive
                })
                .FirstOrDefaultAsync();
        }
    }
}
