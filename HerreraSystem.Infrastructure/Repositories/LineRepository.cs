using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.LineDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Domain.Entities;
using HerreraSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HerreraSystem.Infrastructure.Repositories
{
    public class LineRepository : ILineRepository
    {
        private readonly HerreraSystemContext _context;

        public LineRepository(HerreraSystemContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<LineDto>> GetAllAsync(PaginationParams paginationParams)
        {
            var query = _context.Lines
                .AsNoTracking()
                .OrderBy(l => l.Id)
                .Select(l => new LineDto
                {
                    Id = l.Id,
                    LineName = l.LineName,
                    IsActive = l.IsActive
                });

            var totalRecords = await query.CountAsync();

            var items = await query
                .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            return new PagedResponse<LineDto>
            {
                Data = items,
                CurrentPage = paginationParams.Page,
                PageSize = paginationParams.PageSize,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)paginationParams.PageSize)
            };
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

        public async Task<bool> ExistsByNameAsync(string lineName)
        {
            var normalized = lineName.Trim().ToLower();
            return await _context.Lines.AnyAsync(l => l.LineName.ToLower() == normalized);
        }

        public async Task<bool> ExistsByNameAsync(string lineName, int excludeId)
        {
            var normalized = lineName.Trim().ToLower();
            return await _context.Lines.AnyAsync(l => l.Id != excludeId && l.LineName.ToLower() == normalized);
        }

        public async Task<LineDto> CreateAsync(CreateLineDto dto)
        {
            var line = new Line
            {
                LineName = dto.LineName.Trim(),
                IsActive = dto.IsActive ?? true
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

            line.LineName = dto.LineName.Trim();
            line.IsActive = dto.IsActive ?? line.IsActive;

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