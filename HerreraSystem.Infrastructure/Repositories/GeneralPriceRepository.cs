using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.PricesDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Domain.Entities;
using HerreraSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
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

            var query = _context.LinePresentations.AsNoTracking().AsQueryable();

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
                            && pp.ValidFrom <= now
                            && (pp.ValidTo == null || pp.ValidTo >= now))
                        .OrderByDescending(pp => pp.ValidFrom)
                        .Select(pp => (decimal?)pp.Price)
                        .FirstOrDefault(),
                    WholesalePrice = lp.ProductPrices
                        .Where(pp => pp.IsActive == true
                            && pp.ProductId == null
                            && pp.PriceTypeId == PriceTypeConstants.Wholesale
                            && pp.ValidFrom <= now
                            && (pp.ValidTo == null || pp.ValidTo >= now))
                        .OrderByDescending(pp => pp.ValidFrom)
                        .Select(pp => (decimal?)pp.Price)
                        .FirstOrDefault(),
                    ProductsCount = lp.Products.Count()
                })
                .ToListAsync();
        }

        public async Task<bool> LinePresentationExistsAsync(int linePresentationId)
        {
            return await _context.LinePresentations
                .AnyAsync(lp => lp.Id == linePresentationId);
        }

        public async Task<bool> PriceTypeExistsAsync(int priceTypeId)
        {
            return await _context.PriceTypes
                .AnyAsync(pt => pt.Id == priceTypeId && pt.IsActive == true);
        }

        public async Task<bool> HasOverlappingGeneralPriceAsync(
            int linePresentationId,
            int priceTypeId,
            DateTime validFrom,
            DateTime? validTo,
            int? excludeId = null)
        {
            var end = validTo ?? DateTime.MaxValue;

            return await _context.ProductPrices.AnyAsync(pp =>
                pp.ProductId == null
                && pp.LinePresentationId == linePresentationId
                && pp.PriceTypeId == priceTypeId
                && pp.IsActive == true
                && (!excludeId.HasValue || pp.Id != excludeId.Value)
                && pp.ValidFrom <= end
                && (pp.ValidTo ?? DateTime.MaxValue) >= validFrom);
        }

        public async Task<GeneralPriceDetailDto> CreateGeneralPriceAsync(CreateGeneralPriceDto dto)
        {
            var productPrice = new ProductPrice
            {
                LinePresentationId = dto.LinePresentationId,
                ProductId = null,
                PriceTypeId = dto.PriceTypeId,
                Price = dto.Price,
                ValidFrom = dto.ValidFrom,
                ValidTo = dto.ValidTo,
                IsActive = true,
                CreatedBy = dto.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };

            _context.ProductPrices.Add(productPrice);
            await _context.SaveChangesAsync();

            return (await GetGeneralPriceDetailQuery()
                .FirstAsync(pp => pp.Id == productPrice.Id));
        }

        public async Task<GeneralPriceDetailDto?> ChangeGeneralPriceAsync(int linePresentationId, ChangeGeneralPriceDto dto)
        {
            var now = DateTime.UtcNow;
            var current = await _context.ProductPrices
                .Where(pp => pp.ProductId == null
                    && pp.LinePresentationId == linePresentationId
                    && pp.PriceTypeId == dto.PriceTypeId
                    && pp.IsActive == true
                    && pp.ValidFrom <= now
                    && (pp.ValidTo == null || pp.ValidTo >= now))
                .OrderByDescending(pp => pp.ValidFrom)
                .FirstOrDefaultAsync();

            var end = dto.ValidTo ?? DateTime.MaxValue;
            var hasAnotherOverlap = await _context.ProductPrices.AnyAsync(pp =>
                pp.ProductId == null
                && pp.LinePresentationId == linePresentationId
                && pp.PriceTypeId == dto.PriceTypeId
                && pp.IsActive == true
                && (current == null || pp.Id != current.Id)
                && pp.ValidFrom <= end
                && (pp.ValidTo ?? DateTime.MaxValue) >= dto.ValidFrom);

            if (hasAnotherOverlap)
                return null;

            if (current is not null)
            {
                if (dto.ValidFrom <= current.ValidFrom)
                    return null;

                current.IsActive = false;
                current.ValidTo = dto.ValidFrom.AddMilliseconds(-1);
            }

            var productPrice = new ProductPrice
            {
                LinePresentationId = linePresentationId,
                ProductId = null,
                PriceTypeId = dto.PriceTypeId,
                Price = dto.Price,
                ValidFrom = dto.ValidFrom,
                ValidTo = dto.ValidTo,
                IsActive = true,
                CreatedBy = dto.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };

            _context.ProductPrices.Add(productPrice);
            await _context.SaveChangesAsync();

            return await GetGeneralPriceDetailQuery()
                .FirstOrDefaultAsync(pp => pp.Id == productPrice.Id);
        }

        public async Task<List<GeneralPriceDetailDto>> GetCurrentGeneralPricesAsync(int? lineId, int? priceTypeId)
        {
            var now = DateTime.UtcNow;

            var query = GetGeneralPriceBaseQuery()
                .Where(pp => pp.IsActive == true
                    && pp.ValidFrom <= now
                    && (pp.ValidTo == null || pp.ValidTo >= now));

            if (lineId.HasValue)
                query = query.Where(pp => pp.LinePresentation!.LineId == lineId.Value);

            if (priceTypeId.HasValue)
                query = query.Where(pp => pp.PriceTypeId == priceTypeId.Value);

            return await ProjectGeneralPrice(query)
                .OrderBy(dto => dto.LineName)
                .ThenBy(dto => dto.PresentationName)
                .ThenBy(dto => dto.PriceTypeName)
                .ToListAsync();
        }

        public async Task<PagedResponse<GeneralPriceDetailDto>> GetGeneralPriceHistoryAsync(
            int? linePresentationId,
            int? priceTypeId,
            PaginationParams paginationParams)
        {
            var query = GetGeneralPriceBaseQuery();

            if (linePresentationId.HasValue)
                query = query.Where(pp => pp.LinePresentationId == linePresentationId.Value);

            if (priceTypeId.HasValue)
                query = query.Where(pp => pp.PriceTypeId == priceTypeId.Value);

            return await ProjectGeneralPrice(query)
                .OrderByDescending(dto => dto.ValidFrom)
                .ThenByDescending(dto => dto.Id)
                .ToPagedResponseAsync(paginationParams);
        }

        public async Task<PriceStatisticsDto> GetStatisticsAsync()
        {
            var now = DateTime.UtcNow;
            var nextSevenDays = now.AddDays(7);

            var productsWithPrice = await _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive == true)
                .CountAsync(p =>
                    _context.ProductPrices.Any(pp =>
                        pp.ProductId == p.Id
                        && pp.IsActive == true
                        && pp.ValidFrom <= now
                        && (pp.ValidTo == null || pp.ValidTo >= now))
                    || _context.ProductPrices.Any(pp =>
                        pp.ProductId == null
                        && pp.LinePresentationId == p.LinePresentationId
                        && pp.IsActive == true
                        && pp.ValidFrom <= now
                        && (pp.ValidTo == null || pp.ValidTo >= now)));

            var activeSpecialPrices = await _context.ProductPrices
                .AsNoTracking()
                .CountAsync(pp => pp.ProductId != null
                    && pp.IsActive == true
                    && pp.ValidFrom <= now
                    && (pp.ValidTo == null || pp.ValidTo >= now));

            var promotionsExpiringSoon = await _context.ProductPrices
                .AsNoTracking()
                .CountAsync(pp => pp.ProductId != null
                    && pp.IsActive == true
                    && pp.ValidFrom <= now
                    && pp.ValidTo != null
                    && pp.ValidTo >= now
                    && pp.ValidTo <= nextSevenDays);

            var lastUpdate = await _context.ProductPrices
                .AsNoTracking()
                .Select(pp => pp.CreatedAt ?? pp.ValidFrom)
                .OrderByDescending(date => date)
                .FirstOrDefaultAsync();

            return new PriceStatisticsDto
            {
                ProductsWithPrice = productsWithPrice,
                ActiveSpecialPrices = activeSpecialPrices,
                PromotionsExpiringSoon = promotionsExpiringSoon,
                LastUpdate = lastUpdate == default ? null : lastUpdate
            };
        }

        private IQueryable<ProductPrice> GetGeneralPriceBaseQuery()
        {
            return _context.ProductPrices
                .AsNoTracking()
                .Where(pp => pp.ProductId == null && pp.LinePresentationId != null);
        }

        private IQueryable<GeneralPriceDetailDto> GetGeneralPriceDetailQuery()
        {
            return ProjectGeneralPrice(GetGeneralPriceBaseQuery());
        }

        private static IQueryable<GeneralPriceDetailDto> ProjectGeneralPrice(IQueryable<ProductPrice> query)
        {
            return query.Select(pp => new GeneralPriceDetailDto
            {
                Id = pp.Id,
                LinePresentationId = pp.LinePresentationId!.Value,
                LineName = pp.LinePresentation!.Line.LineName,
                PresentationName = pp.LinePresentation.Presentation.PresentationName,
                PriceTypeId = pp.PriceTypeId,
                PriceTypeName = pp.PriceType.PriceName,
                Price = pp.Price,
                ValidFrom = pp.ValidFrom,
                ValidTo = pp.ValidTo,
                IsActive = pp.IsActive == true,
                CreatedBy = pp.CreatedBy,
                CreatedAt = pp.CreatedAt
            });
        }
    }
}
