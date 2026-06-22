using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.ProductDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Domain.Entities;
using HerreraSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using static HerreraSystem.Application.Common.Constants;


namespace HerreraSystem.Infrastructure.Repositories
{
    public class ProductRepository:IProductRepository
    {
        private readonly HerreraSystemContext _context;

        public ProductRepository(HerreraSystemContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<ProductDto>> GetAllAsync(
        PaginationParams paginationParams)
        {
            var query = _context.Products
                .AsNoTracking()
                .OrderBy(p => p.ProductName)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    LinePresentationId = p.LinePresentationId,
                    FlavorId = p.FlavorId,
                    ProductName = p.ProductName,
                    IsActive = p.IsActive,
                    CreatedBy = p.CreatedBy,
                    CreatedAt = p.CreatedAt,
                    ImageUrl = p.ImageUrl,
                    MinimumStock = p.MinimumStock
                });

            return await query.ToPagedResponseAsync(paginationParams);
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product is null) return null;

            return new ProductDto
            {
                Id = product.Id,
                LinePresentationId = product.LinePresentationId,
                FlavorId = product.FlavorId,
                ProductName = product.ProductName,
                IsActive = product.IsActive,
                CreatedBy = product.CreatedBy,
                CreatedAt = product.CreatedAt,
                ImageUrl = product.ImageUrl,
                MinimumStock = product.MinimumStock
            };
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto dto)
        {
            var product = new Product
            {
                LinePresentationId = dto.LinePresentationId,
                FlavorId = dto.FlavorId,
                ProductName = dto.ProductName,
                CreatedBy = dto.CreatedBy,
                ImageUrl = dto.ImageUrl,
                MinimumStock = dto.MinimumStock,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return new ProductDto
            {
                Id = product.Id,
                LinePresentationId = product.LinePresentationId,
                FlavorId = product.FlavorId,
                ProductName = product.ProductName,
                IsActive = product.IsActive,
                CreatedBy = product.CreatedBy,
                CreatedAt = product.CreatedAt,
                ImageUrl = product.ImageUrl,
                MinimumStock = product.MinimumStock
            };
        }

        public async Task<bool> UpdateAsync(int id, UpdateProductDto dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product is null) return false;

            product.LinePresentationId = dto.LinePresentationId;
            product.FlavorId = dto.FlavorId;
            product.ProductName = dto.ProductName;
            product.IsActive = dto.IsActive;
            product.ImageUrl = dto.ImageUrl;
            product.MinimumStock = dto.MinimumStock;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PatchAsync(int id, PatchProductDto dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product is null) return false;

            if (dto.LinePresentationId.HasValue)
                product.LinePresentationId = dto.LinePresentationId.Value;
            if (dto.FlavorId.HasValue)
                product.FlavorId = dto.FlavorId.Value;
            if (dto.ProductName is not null)
                product.ProductName = dto.ProductName;
            if (dto.IsActive.HasValue)
                product.IsActive = dto.IsActive.Value;
            if (dto.ImageUrl is not null)
                product.ImageUrl = dto.ImageUrl;
            if (dto.MinimumStock.HasValue)
                product.MinimumStock = dto.MinimumStock.Value;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product is null) return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PagedResponse<ProductCatalogDto>> GetCatalogAsync(
        int? lineId,
        int? flavorId,
        string? search,
        bool? active,
        PaginationParams paginationParams)
        {
            var query = _context.Products
                .AsNoTracking()
                .AsQueryable();

            if (lineId.HasValue)
                query = query.Where(p =>
                    p.LinePresentation.LineId == lineId.Value);

            if (flavorId.HasValue)
                query = query.Where(p =>
                    p.FlavorId == flavorId.Value);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p =>
                    p.ProductName.Contains(search));

            if (active.HasValue)
                query = query.Where(p =>
                    p.IsActive == active.Value);

            var catalogQuery = query
                .OrderBy(p => p.ProductName)
                .Select(p => new ProductCatalogDto
                {
                    Id = p.Id,
                    ProductName = p.ProductName,
                    ImageUrl = p.ImageUrl,
                    IsActive = p.IsActive,

                    LineName = p.LinePresentation.Line.LineName,

                    FlavorName = p.Flavor.FlavorName,

                    PresentationName = p.LinePresentation
                        .Presentation.PresentationName,

                    WholesalePrice = _context.ProductPrices
                        .Where(pp =>
                            pp.IsActive == true &&
                            pp.LinePresentationId == p.LinePresentationId &&
                            pp.PriceTypeId == PriceTypeConstants.Wholesale &&
                            (pp.ValidTo == null ||
                             pp.ValidTo >= DateTime.UtcNow))
                        .OrderByDescending(pp => pp.ValidFrom)
                        .Select(pp => (decimal?)pp.Price)
                        .FirstOrDefault(),

                    RetailPrice = _context.ProductPrices
                        .Where(pp =>
                            pp.IsActive == true &&
                            pp.LinePresentationId == p.LinePresentationId &&
                            pp.PriceTypeId == PriceTypeConstants.Retail &&
                            (pp.ValidTo == null ||
                             pp.ValidTo >= DateTime.UtcNow))
                        .OrderByDescending(pp => pp.ValidFrom)
                        .Select(pp => (decimal?)pp.Price)
                        .FirstOrDefault()
                });

            return await catalogQuery
                .ToPagedResponseAsync(paginationParams);
        }

        public async Task<bool> ExistsAsync(
            string productName, int linePresentationId, int flavorId, int? excludeId = null)
        {
            return await _context.Products
                .Where(p => p.ProductName == productName
                         && p.LinePresentationId == linePresentationId
                         && p.FlavorId == flavorId
                         && (excludeId == null || p.Id != excludeId))
                .AnyAsync();
        }

        public async Task<bool> HasBatchesAsync(int productId)
            => await _context.Batches.AnyAsync(b => b.ProductId == productId);

        public async Task<bool> HasActivePricesAsync(int productId)
            => await _context.ProductPrices
                .AnyAsync(pp => pp.ProductId == productId && pp.IsActive == true);
    }

}

