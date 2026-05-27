using HerreraSystem.Domain.Entities;
using HerreraSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using HerreraSystem.Application.Interfaces.Repositories;

namespace HerreraSystem.Infrastructure.Repositories
{
    public class BatchRepository:IBatchRepository
    {
        private readonly HerreraSystemContext _context;

        public BatchRepository(HerreraSystemContext context)
        {
            _context = context;
        }

        public async Task<Batch> CreateAsync(Batch batch)
        {
            _context.Batches.Add(batch);
            await _context.SaveChangesAsync();
            return batch;
        }

        public async Task<int> CountByYearAsync(int year)
        {
            // Cuenta lotes creados en ese año para el correlativo
            return await _context.Batches
                .Include(b => b.Restock)
                .Where(b => b.Restock.RestockDate.HasValue
                         && b.Restock.RestockDate.Value.Year == year)
                .CountAsync();
        }

        public async Task<string> BuildBatchCodeAsync(int productId, int year, int correlative)
        {
            var detail = await _context.Products
                .Where(p => p.Id == productId)
                .Select(p => new
                {
                    Line = p.LinePresentation.Line.LineName,
                    Flavor = p.Flavor.FlavorName,
                    Presentation = p.LinePresentation.Presentation.PresentationName
                })
                .FirstAsync();

            string line = Truncate(detail.Line, 3).ToUpper();
            string flavor = Truncate(detail.Flavor, 3).ToUpper();
            string presentation = Truncate(
              detail.Presentation.Replace(" ", ""),3).ToUpper();

            return $"{line}-{flavor}-{presentation}-{year}-{correlative:D4}";
        }

        private static string Truncate(string value, int maxLength) =>
            value.Length <= maxLength ? value : value[..maxLength];
    }
}
