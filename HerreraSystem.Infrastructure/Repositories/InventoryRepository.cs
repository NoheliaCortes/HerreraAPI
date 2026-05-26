using HerreraSystem.Application.DTOs.InventoryDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Infrastructure.Repositories
{
    public class InventoryRepository:IInventoryRepository
    {
        private readonly HerreraSystemContext _context;

        public InventoryRepository(HerreraSystemContext context)
        {
            _context = context;
        }

        public async Task<List<InventoryProductDto>> GetInventoryProductsAsync(
            string? search,
            int? lineId,
            int? flavorId,
            int? presentationId)
        {
            var now = DateTime.UtcNow;

            var query = _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive == true)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.ProductName.Contains(search));

            if (lineId.HasValue)
                query = query.Where(p => p.LinePresentation.LineId == lineId.Value);

            if (flavorId.HasValue)
                query = query.Where(p => p.FlavorId == flavorId.Value);

            if (presentationId.HasValue)
                query = query.Where(p => p.LinePresentation.PresentationId == presentationId.Value);

            return await query
                .Select(p => new InventoryProductDto
                {
                    ProductId = p.Id,
                    ProductName = p.ProductName,
                    LineName = p.LinePresentation.Line.LineName,
                    PresentationName = p.LinePresentation.Presentation.PresentationName,
                    FlavorName = p.Flavor.FlavorName,

                    DisplayStock = p.Batches
                        .SelectMany(b => b.BatchLocations)
                        .Where(bl => bl.LocationId == 1)
                        .Sum(bl => (int?)bl.CurrentStock) ?? 0,

                    WarehouseStock = p.Batches
                        .SelectMany(b => b.BatchLocations)
                        .Where(bl => bl.LocationId == 2)
                        .Sum(bl => (int?)bl.CurrentStock) ?? 0,

                    ReservedStock = p.Batches
                        .SelectMany(b => b.BatchLocations)
                        .Where(bl => bl.LocationId == 3)
                        .Sum(bl => (int?)bl.CurrentStock) ?? 0,

                    TotalStock = p.Batches
                        .SelectMany(b => b.BatchLocations)
                        .Sum(bl => (int?)bl.CurrentStock) ?? 0,

                    RetailPrice = p.LinePresentation.ProductPrices
                    .Where(pp => pp.IsActive == true
                              && pp.PriceTypeId == 1
                              && pp.ProductId == null          // precio general de LinePresentation
                              && (pp.ValidTo == null || pp.ValidTo >= now))
                    .Select(pp => (decimal?)pp.Price)
                    .FirstOrDefault(),

                                    WholesalePrice = p.LinePresentation.ProductPrices
                    .Where(pp => pp.IsActive == true
                              && pp.PriceTypeId == 2
                              && pp.ProductId == null          // precio general de LinePresentation
                              && (pp.ValidTo == null || pp.ValidTo >= now))
                    .Select(pp => (decimal?)pp.Price)
                    .FirstOrDefault()
                })
                .ToListAsync();
        }

    }
}
