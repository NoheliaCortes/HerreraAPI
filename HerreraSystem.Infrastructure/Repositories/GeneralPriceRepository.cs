using HerreraSystem.Application.DTOs.PricesDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using static HerreraSystem.Application.Common.Constants;

namespace HerreraSystem.Infrastructure.Repositories
{
    public class GeneralPriceRepository : IGeneralPriceRepository
    {
        private readonly HerreraSystemContext _context;

        public GeneralPriceRepository(HerreraSystemContext context)
        {
            _context = context;
        }

        public async Task<List<GeneralPriceDto>> GetGeneralPricesAsync(int? lineId)
        {
            var now = DateTime.UtcNow;

            var query = _context.LinePresentations.AsQueryable();

            if (lineId.HasValue)
                query = query.Where(lp => lp.LineId == lineId.Value);

            return await query
                .Select(lp => new GeneralPriceDto
                {
                    LinePresentationId = lp.Id,
                    LineName = lp.Line.LineName,
                    PresentationName = lp.Presentation.PresentationName,

                            RetailPrice = lp.ProductPrices
                .Where(pp => pp.IsActive == true
                          && pp.ProductId == null
                          && pp.PriceTypeId == PriceTypeConstants.Retail
                          && (pp.ValidTo == null || pp.ValidTo >= now))
                .OrderByDescending(pp => pp.ValidFrom)
                .Select(pp => (decimal?)pp.Price)
                .FirstOrDefault(),

                            WholesalePrice = lp.ProductPrices
                .Where(pp => pp.IsActive == true
                          && pp.ProductId == null
                          && pp.PriceTypeId == PriceTypeConstants.Wholesale
                          && (pp.ValidTo == null || pp.ValidTo >= now))
                .OrderByDescending(pp => pp.ValidFrom)
                .Select(pp => (decimal?)pp.Price)
                .FirstOrDefault(),

                            ProductsCount = lp.Products.Count()
                        })
                 .ToListAsync();
        }


    }
}
