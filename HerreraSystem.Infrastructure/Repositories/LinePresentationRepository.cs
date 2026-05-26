using HerreraSystem.Application.DTOs.LinePresentationDtos;
using HerreraSystem.Application.DTOs.PresentationDtos;
using HerreraSystem.Domain.Entities;
using HerreraSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using HerreraSystem.Application.Interfaces.Repositories;

namespace HerreraSystem.Infrastructure.Repositories
{
    public class LinePresentationRepository : ILinePresentationRepository
    {
        private readonly HerreraSystemContext _context;

        public LinePresentationRepository(HerreraSystemContext context)
        {
            _context = context;
        }

        public async Task<List<LinePresentationDto>> GetAllAsync()
        {
            return await _context.LinePresentations
                .Include(lp => lp.Line)
                .Include(lp => lp.Presentation)
                .Select(lp => new LinePresentationDto
                {
                    Id = lp.Id,

                    Line = new LineReferenceDto
                    {
                        Id = lp.Line.Id,
                        Name = lp.Line.LineName
                    },

                    Presentation = new PresentationReferenceDto
                    {
                        Id = lp.Presentation.Id,
                        Name = lp.Presentation.PresentationName
                    }
                })
                .ToListAsync();
        }

        public async Task<LinePresentationDto?> GetByIdAsync(int id)
        {
            var linePresentation = await _context.LinePresentations
                .Include(lp => lp.Line)
                .Include(lp => lp.Presentation)
                .FirstOrDefaultAsync(lp => lp.Id == id);

            if (linePresentation is null)
                return null;

            return new LinePresentationDto
            {
                Id = linePresentation.Id,

                Line = new LineReferenceDto
                {
                    Id = linePresentation.Line.Id,
                    Name = linePresentation.Line.LineName
                },

                Presentation = new PresentationReferenceDto
                {
                    Id = linePresentation.Presentation.Id,
                    Name = linePresentation.Presentation.PresentationName
                }
            };
        }

        public async Task<LinePresentationDto?> CreateAsync(CreateLinePresentationDto dto)
        {
            var lineExists = await _context.Lines
                .AnyAsync(l => l.Id == dto.LineId);

            if (!lineExists)
                return null;

            var presentationExists = await _context.Presentations
                .AnyAsync(p => p.Id == dto.PresentationId);

            if (!presentationExists)
                return null;

            var combinationExists = await _context.LinePresentations
                .AnyAsync(lp =>
                    lp.LineId == dto.LineId &&
                    lp.PresentationId == dto.PresentationId);

            if (combinationExists)
                return null;

            var entity = new LinePresentation
            {
                LineId = dto.LineId,
                PresentationId = dto.PresentationId
            };

            _context.LinePresentations.Add(entity);

            await _context.SaveChangesAsync();

            var created = await _context.LinePresentations
                .Include(lp => lp.Line)
                .Include(lp => lp.Presentation)
                .FirstAsync(lp => lp.Id == entity.Id);

            return new LinePresentationDto
            {
                Id = created.Id,

                Line = new LineReferenceDto
                {
                    Id = created.Line.Id,
                    Name = created.Line.LineName
                },

                Presentation = new PresentationReferenceDto
                {
                    Id = created.Presentation.Id,
                    Name = created.Presentation.PresentationName
                }
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var linePresentation = await _context.LinePresentations
                .FindAsync(id);

            if (linePresentation is null)
                return false;

            _context.LinePresentations.Remove(linePresentation);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<PresentationDto>> GetPresentationsByLineAsync(int lineId)
        {
            return await _context.LinePresentations
                .Where(lp => lp.LineId == lineId)
                .Include(lp => lp.Presentation)
                .Select(lp => new PresentationDto
                {
                    Id = lp.Presentation.Id,
                    PresentationName = lp.Presentation.PresentationName,
                    IsActive = lp.Presentation.IsActive
                })
                .ToListAsync();
        }
    }



}
