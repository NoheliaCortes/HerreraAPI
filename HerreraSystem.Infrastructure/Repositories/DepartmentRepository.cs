using HerreraSystem.Application.DTOs.DepartmentDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace HerreraSystem.Infrastructure.Repositories
{
    public class DepartmentRepository:IDepartmentRepository
    {
        private readonly HerreraSystemContext _context;

        public DepartmentRepository(HerreraSystemContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<DepartmentDto>> GetAllAsync()
        {
            return await _context.Departments
                .AsNoTracking()
                .Where(d => d.IsActive == true)
                .OrderBy(d => d.DepartmentName)
                .Select(d => new DepartmentDto
                {
                    Id = d.Id,
                    DepartmentName = d.DepartmentName,
                    IsActive = d.IsActive
                })
                .ToListAsync();
        }

        public async Task<DepartmentDto?> GetByIdAsync(int id)
        {
            return await _context.Departments
                .AsNoTracking()
                .Where(d => d.Id == id)
                .Select(d => new DepartmentDto
                {
                    Id = d.Id,
                    DepartmentName = d.DepartmentName,
                    IsActive = d.IsActive
                })
                .FirstOrDefaultAsync();
        }
    }
}
