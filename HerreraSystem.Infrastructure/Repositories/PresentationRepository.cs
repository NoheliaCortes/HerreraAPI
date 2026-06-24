using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.PresentationDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Domain.Entities;
using HerreraSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HerreraSystem.Infrastructure.Repositories
{
    public class PresentationRepository : IPresentationRepository
    {
        private readonly HerreraSystemContext _context;

        public PresentationRepository(HerreraSystemContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<PresentationDto>> GetAllAsync(PaginationParams paginationParams)
        {
            var query = _context.Presentations
                .AsNoTracking()
                .OrderBy(p => p.Id)
                .Select(p => new PresentationDto
                {
                    Id = p.Id,
                    PresentationName = p.PresentationName,
                    IsActive = p.IsActive
                });

            var totalRecords = await query.CountAsync();

            var items = await query
                .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            return new PagedResponse<PresentationDto>
            {
                Data = items,
                CurrentPage = paginationParams.Page,
                PageSize = paginationParams.PageSize,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)paginationParams.PageSize)
            };
        }

        public async Task<PresentationDto?> GetByIdAsync(int id)
        {
            var presentation = await _context.Presentations.FindAsync(id);
            if (presentation is null) return null;

            return new PresentationDto
            {
                Id = presentation.Id,
                PresentationName = presentation.PresentationName,
                IsActive = presentation.IsActive
            };
        }

        public async Task<bool> ExistsByNameAsync(string presentationName)
        {
            var normalized = presentationName.Trim().ToLower();
            return await _context.Presentations.AnyAsync(p => p.PresentationName.ToLower() == normalized);
        }

        public async Task<bool> ExistsByNameAsync(string presentationName, int excludeId)
        {
            var normalized = presentationName.Trim().ToLower();
            return await _context.Presentations.AnyAsync(p => p.Id != excludeId && p.PresentationName.ToLower() == normalized);
        }

        public async Task<PresentationDto> CreateAsync(CreatePresentationDto dto)
        {
            var presentation = new Presentation
            {
                PresentationName = dto.PresentationName.Trim(),
                IsActive = dto.IsActive ?? true
            };

            _context.Presentations.Add(presentation);
            await _context.SaveChangesAsync();

            return new PresentationDto
            {
                Id = presentation.Id,
                PresentationName = presentation.PresentationName,
                IsActive = presentation.IsActive
            };
        }

        public async Task<bool> UpdateAsync(int id, UpdatePresentationDto dto)
        {
            var presentation = await _context.Presentations.FindAsync(id);
            if (presentation is null) return false;

            presentation.PresentationName = dto.PresentationName.Trim();
            presentation.IsActive = dto.IsActive ?? presentation.IsActive;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var presentation = await _context.Presentations.FindAsync(id);
            if (presentation is null) return false;

            _context.Presentations.Remove(presentation);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}