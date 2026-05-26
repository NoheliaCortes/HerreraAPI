using HerreraSystem.Application.DTOs.LineDtos;
using HerreraSystem.Domain.Entities;
using HerreraSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using HerreraSystem.Application.Interfaces.Repositories;

namespace HerreraSystem.Infrastructure.Repositories
{
    public class LineRepository : ILineRepository
    {
        private readonly HerreraSystemContext _context;

        public LineRepository(HerreraSystemContext context)
        {
            _context = context;
        }

        public async Task<List<LineDto>> GetAllAsync()
        {
            return await _context.Lines
                .Select(l => new LineDto
                {
                    Id = l.Id,
                    LineName = l.LineName,
                    IsActive = l.IsActive
                }).ToListAsync();
        }

        public async Task<LineDto?> GetByIdAsync(int id)
        {
            var line = await _context.Lines.FindAsync(id);
            if (line is null) return null;

            return new LineDto
            {
                Id = line.Id,
                LineName = line.LineName,
                IsActive = line.IsActive
            };
        }

        public async Task<LineDto> CreateAsync(CreateLineDto dto)
        {
            var line = new Line
            {
                LineName = dto.LineName,
                IsActive = true
            };

            _context.Lines.Add(line);
            await _context.SaveChangesAsync();

            return new LineDto
            {
                Id = line.Id,
                LineName = line.LineName,
                IsActive = line.IsActive
            };
        }

        public async Task<bool> UpdateAsync(int id, UpdateLineDto dto)
        {
            var line = await _context.Lines.FindAsync(id);
            if (line is null) return false;

            line.LineName = dto.LineName;
            line.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var line = await _context.Lines.FindAsync(id);
            if (line is null) return false;

            _context.Lines.Remove(line);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
