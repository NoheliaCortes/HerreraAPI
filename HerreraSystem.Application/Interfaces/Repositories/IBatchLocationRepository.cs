using HerreraSystem.Application.DTOs.BatchDtos;
using HerreraSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Interfaces.Repositories
{
    public interface IBatchLocationRepository
    {
        Task CreateAsync(BatchLocation batchLocation);

        // Nuevos para la venta al detalle
        Task<List<BatchLocationFIFODto>> GetAvailableStockFifoAsync(
            int productId, int mostradorLocationId);

        Task<BatchLocation?> GetByBatchAndLocationAsync(
            int batchId, int locationId);

        Task UpdateStockAsync(BatchLocation batchLocation);

     
    }
}
