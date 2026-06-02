using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace HerreraSystem.Infrastructure.Repositories
{
    public class ProductPriceRepository : IProductPriceRepository
    {
        private readonly HerreraSystemContext _context;

        public ProductPriceRepository(HerreraSystemContext context)
        {
            _context = context;
        }

        public async Task<decimal?> GetActivePriceAsync(int productId, string priceTypeName)
        {
            var now = DateTime.UtcNow;

            // Intento 1 — precio asignado directamente al producto
            var priceByProduct = await _context.ProductPrices
                .Where(pp =>
                    pp.ProductId == productId &&
                    pp.IsActive == true &&
                    pp.PriceType.PriceName == priceTypeName &&
                    pp.ValidFrom <= now &&
                    (pp.ValidTo == null || pp.ValidTo >= now))
                .Select(pp => (decimal?)pp.Price)
                .FirstOrDefaultAsync();

            if (priceByProduct is not null)
                return priceByProduct;

            // Intento 2 — precio asignado por LinePresentationId del producto
            // Primero obtenemos el LinePresentationId del producto
            var linePresentationId = await _context.Products
                .Where(p => p.Id == productId)
                .Select(p => (int?)p.LinePresentationId)
                .FirstOrDefaultAsync();

            if (linePresentationId is null)
                return null;

            return await _context.ProductPrices
                .Where(pp =>
                    pp.LinePresentationId == linePresentationId &&
                    pp.ProductId == null &&           // precio genérico de línea, no de producto
                    pp.IsActive == true &&
                    pp.PriceType.PriceName == priceTypeName &&
                    pp.ValidFrom <= now &&
                    (pp.ValidTo == null || pp.ValidTo >= now))
                .Select(pp => (decimal?)pp.Price)
                .FirstOrDefaultAsync();
        }
    }
}
