using HerreraSystem.Application.DTOs.PresentationDtos;
using HerreraSystem.Domain.Entities;
using HerreraSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;
using HerreraSystem.Application.Interfaces;
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

        public async Task<List<PresentationDto>> GetAllAsync()
        {
            return await _context.Presentations
                .Select(p => new PresentationDto
                {
                    Id = p.Id,
                    PresentationName = p.PresentationName,
                    IsActive = p.IsActive
                }).ToListAsync();
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

        public async Task<PresentationDto> CreateAsync(CreatePresentationDto dto)
        {
            var presentation = new Presentation
            {
                PresentationName = dto.PresentationName,
                IsActive = true
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

            presentation.PresentationName = dto.PresentationName;
            presentation.IsActive = dto.IsActive;

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
