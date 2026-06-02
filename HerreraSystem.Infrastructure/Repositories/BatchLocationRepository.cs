using HerreraSystem.Application.DTOs.BatchDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Domain.Entities;
using HerreraSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Infrastructure.Repositories
{
    public class BatchLocationRepository: IBatchLocationRepository
    {
        private readonly HerreraSystemContext _context;

        public BatchLocationRepository(HerreraSystemContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(BatchLocation batchLocation)
        {
            _context.BatchLocations.Add(batchLocation);
            await _context.SaveChangesAsync();
        }

        // Devuelve lotes con stock en Mostrador para un producto,
        // ordenados por RestockDate ASC — el más antiguo primero (FIFO)
        public async Task<List<BatchLocationFIFODto>> GetAvailableStockFifoAsync(
            int productId, int mostradorLocationId)
        {
            return await _context.BatchLocations
                .Where(bl =>
                    bl.LocationId == mostradorLocationId &&
                    bl.CurrentStock > 0 &&
                    bl.Batch.ProductId == productId &&
                    bl.Batch.BatchStatusId == 1) // Activo
                .OrderBy(bl => bl.Batch.Restock.RestockDate) // FIFO: el más antiguo primero
                .Select(bl => new BatchLocationFIFODto
                {
                    BatchLocationId = bl.Id,
                    BatchId = bl.BatchId,
                    CurrentStock = bl.CurrentStock,
                    RestockDate = bl.Batch.Restock.RestockDate!.Value
                })
                .ToListAsync();
        }

        public async Task<BatchLocation?> GetByBatchAndLocationAsync(int batchId, int locationId)
        {
            return await _context.BatchLocations
                .FirstOrDefaultAsync(bl =>
                    bl.BatchId == batchId && bl.LocationId == locationId);
        }

        public async Task UpdateStockAsync(BatchLocation batchLocation)
        {
            _context.BatchLocations.Update(batchLocation);
            await _context.SaveChangesAsync();
        }

    }
}
