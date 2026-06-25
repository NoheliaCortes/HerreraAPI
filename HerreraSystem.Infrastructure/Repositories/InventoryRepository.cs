using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.InventoryDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Application.Interfaces.Services;
using HerreraSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Infrastructure.Repositories
{
    public class InventoryRepository:IInventoryRepository
    {
        private const int WarehouseLocationId = 1;
        private const int DisplayLocationId = 2;
        private const int ReservedLocationId = 3;
        private const int ActiveBatchStatusId = 1;
        private const int DetailSaleTypeId = 1;
        private const int WholesaleSaleTypeId = 2;

        private readonly HerreraSystemContext _context;
        private readonly INicaraguaDateTimeService _dateTimeService;

        public InventoryRepository(
            HerreraSystemContext context,
            INicaraguaDateTimeService dateTimeService)
        {
            _context = context;
            _dateTimeService = dateTimeService;
        }

        public async Task<List<InventoryProductDto>> GetInventoryProductsAsync(
            string? search,
            int? lineId,
            int? flavorId,
            int? presentationId)
        {
            return await BuildInventoryProductsQuery(
                    search, lineId, flavorId, presentationId)
                .ToListAsync();
        }

        public async Task<PagedResponse<InventoryProductDto>> GetAllAsync(
            string? search,
            int? lineId,
            int? flavorId,
            int? presentationId,
            PaginationParams paginationParams)
        {
            var query = BuildInventoryProductsQuery(
                search, lineId, flavorId, presentationId);

            return await query.ToPagedResponseAsync(paginationParams);
        }

        public async Task<InventoryProductBatchesDto?> GetProductBatchesAsync(int productId)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Where(p => p.Id == productId)
                .Select(p => new
                {
                    p.Id,
                    p.ProductName
                })
                .FirstOrDefaultAsync();

            if (product is null)
                return null;

            var batches = await _context.Batches
                .AsNoTracking()
                .Where(b =>
                    b.ProductId == productId &&
                    b.BatchStatusId == ActiveBatchStatusId)
                .Select(b => new InventoryProductBatchDto
                {
                    BatchId = b.Id,
                    BatchCode = b.BatchCode,
                    BatchStatusName = b.BatchStatus.BatchStatusName,
                    EntryDate = b.Restock.RestockDate,
                    ExpirationDate = b.ExpirationDate,
                    StockDisplay = b.BatchLocations
                        .Where(bl => bl.LocationId == DisplayLocationId)
                        .Sum(bl => (int?)bl.CurrentStock) ?? 0,
                    StockWarehouse = b.BatchLocations
                        .Where(bl => bl.LocationId == WarehouseLocationId)
                        .Sum(bl => (int?)bl.CurrentStock) ?? 0,
                    StockReserved = b.BatchLocations
                        .Where(bl => bl.LocationId == ReservedLocationId)
                        .Sum(bl => (int?)bl.CurrentStock) ?? 0,
                    TotalCurrentStock = b.BatchLocations
                        .Where(bl =>
                            bl.LocationId == DisplayLocationId ||
                            bl.LocationId == WarehouseLocationId ||
                            bl.LocationId == ReservedLocationId)
                        .Sum(bl => (int?)bl.CurrentStock) ?? 0,
                    AvailableForSale = b.BatchLocations
                        .Where(bl =>
                            bl.LocationId == DisplayLocationId ||
                            bl.LocationId == WarehouseLocationId)
                        .Sum(bl => (int?)bl.CurrentStock) ?? 0
                })
                .Where(b => b.TotalCurrentStock > 0)
                .OrderBy(b => b.EntryDate)
                .ThenBy(b => b.BatchId)
                .ToListAsync();

            return new InventoryProductBatchesDto
            {
                ProductId = product.Id,
                ProductName = product.ProductName,
                ActiveBatchCount = batches.Count,
                Batches = batches
            };
        }

        public async Task<InventoryBatchDetailDto?> GetBatchDetailAsync(int batchId)
        {
            return await _context.Batches
                .AsNoTracking()
                .Where(b => b.Id == batchId)
                .Select(b => new InventoryBatchDetailDto
                {
                    BatchId = b.Id,
                    BatchCode = b.BatchCode,
                    ProductId = b.ProductId,
                    RestockId = b.RestockId,
                    BatchStatusName = b.BatchStatus.BatchStatusName,
                    EntryDate = b.Restock.RestockDate,
                    ExpirationDate = b.ExpirationDate,
                    InitialQuantity = b.InitialQuantity,
                    UnitProductionCost = b.UnitProductionCost,
                    EstimatedTotalCost = b.InitialQuantity * b.UnitProductionCost,
                    StockDisplay = b.BatchLocations
                        .Where(bl => bl.LocationId == DisplayLocationId)
                        .Sum(bl => (int?)bl.CurrentStock) ?? 0,
                    StockWarehouse = b.BatchLocations
                        .Where(bl => bl.LocationId == WarehouseLocationId)
                        .Sum(bl => (int?)bl.CurrentStock) ?? 0,
                    StockReserved = b.BatchLocations
                        .Where(bl => bl.LocationId == ReservedLocationId)
                        .Sum(bl => (int?)bl.CurrentStock) ?? 0,
                    TotalCurrentStock = b.BatchLocations
                        .Where(bl =>
                            bl.LocationId == DisplayLocationId ||
                            bl.LocationId == WarehouseLocationId ||
                            bl.LocationId == ReservedLocationId)
                        .Sum(bl => (int?)bl.CurrentStock) ?? 0,
                    AvailableForSale = b.BatchLocations
                        .Where(bl =>
                            bl.LocationId == DisplayLocationId ||
                            bl.LocationId == WarehouseLocationId)
                        .Sum(bl => (int?)bl.CurrentStock) ?? 0,
                    SoldDetail = b.MovementDetails
                        .Where(md =>
                            md.Movement.SaleId != null &&
                            md.DestinationLocationId == null &&
                            md.Movement.Sale!.SaleTypeId == DetailSaleTypeId)
                        .Sum(md => (int?)md.Quantity) ?? 0,
                    SoldWholesale = b.MovementDetails
                        .Where(md =>
                            md.Movement.SaleId != null &&
                            md.DestinationLocationId == null &&
                            md.Movement.Sale!.SaleTypeId == WholesaleSaleTypeId)
                        .Sum(md => (int?)md.Quantity) ?? 0,
                    TotalSold = b.InitialQuantity - (
                        b.BatchLocations
                            .Where(bl =>
                                bl.LocationId == DisplayLocationId ||
                                bl.LocationId == WarehouseLocationId ||
                                bl.LocationId == ReservedLocationId)
                            .Sum(bl => (int?)bl.CurrentStock) ?? 0)
                })
                .FirstOrDefaultAsync();
        }

        public async Task<InventoryStatsDto> GetStatsAsync(string period)
        {
            var normalizedPeriod = NormalizePeriod(period);
            var periodStart = GetPeriodStart(normalizedPeriod);

            var totalProducts = await _context.Products
                .AsNoTracking()
                .CountAsync(p => p.IsActive == true);

            var lowStockProducts = await _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive == true)
                .Select(p => new
                {
                    p.MinimumStock,
                    AvailableStock = p.Batches
                        .Where(b => b.BatchStatusId == ActiveBatchStatusId)
                        .SelectMany(b => b.BatchLocations)
                        .Where(bl =>
                            bl.LocationId == DisplayLocationId ||
                            bl.LocationId == WarehouseLocationId)
                        .Sum(bl => (int?)bl.CurrentStock) ?? 0
                })
                .CountAsync(p => p.AvailableStock <= p.MinimumStock);

            var bestSellingFlavorQuery = _context.MovementDetails
                .AsNoTracking()
                .Where(md =>
                    md.Movement.SaleId != null &&
                    md.DestinationLocationId == null);

            if (periodStart.HasValue)
            {
                bestSellingFlavorQuery = bestSellingFlavorQuery
                    .Where(md =>
                        md.Movement.Sale!.SaleDate.HasValue &&
                        md.Movement.Sale.SaleDate.Value >= periodStart.Value);
            }

            var bestSellingFlavor = await bestSellingFlavorQuery
                .GroupBy(md => new
                {
                    md.Batch.Product.FlavorId,
                    md.Batch.Product.Flavor.FlavorName
                })
                .Select(g => new BestSellingFlavorDto
                {
                    FlavorId = g.Key.FlavorId,
                    FlavorName = g.Key.FlavorName,
                    QuantitySold = g.Sum(md => md.Quantity),
                    Period = normalizedPeriod
                })
                .OrderByDescending(f => f.QuantitySold)
                .ThenBy(f => f.FlavorName)
                .FirstOrDefaultAsync();

            var inventoryValue = await _context.Batches
                .AsNoTracking()
                .Where(b => b.BatchStatusId == ActiveBatchStatusId)
                .Select(b => new
                {
                    b.UnitProductionCost,
                    TotalCurrentStock = b.BatchLocations
                        .Where(bl =>
                            bl.LocationId == DisplayLocationId ||
                            bl.LocationId == WarehouseLocationId ||
                            bl.LocationId == ReservedLocationId)
                        .Sum(bl => (int?)bl.CurrentStock) ?? 0
                })
                .SumAsync(b => b.TotalCurrentStock * b.UnitProductionCost);

            return new InventoryStatsDto
            {
                TotalProducts = totalProducts,
                LowStockProducts = lowStockProducts,
                BestSellingFlavor = bestSellingFlavor,
                InventoryValue = inventoryValue
            };
        }

        private static string NormalizePeriod(string? period)
        {
            var normalized = period?.Trim().ToLowerInvariant();

            return normalized switch
            {
                "day" => "day",
                "week" => "week",
                "month" => "month",
                "year" => "year",
                "all" => "all",
                _ => "week"
            };
        }

        private DateTime? GetPeriodStart(string period)
        {
            var now = _dateTimeService.Now;

            return period switch
            {
                "day" => now.Date,
                "week" => now.AddDays(-7),
                "month" => now.AddMonths(-1),
                "year" => now.AddYears(-1),
                "all" => null,
                _ => now.AddDays(-7)
            };
        }

        private IQueryable<InventoryProductDto> BuildInventoryProductsQuery(
            string? search,
            int? lineId,
            int? flavorId,
            int? presentationId)
        {
            var now = _dateTimeService.Now;

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

            return query
                .OrderBy(p => p.ProductName)
                .Select(p => new InventoryProductDto
                {
                    ProductId = p.Id,
                    ProductName = p.ProductName,
                    ImageUrl = p.ImageUrl,
                    LineName = p.LinePresentation.Line.LineName,
                    PresentationName = p.LinePresentation.Presentation.PresentationName,
                    FlavorName = p.Flavor.FlavorName,

                    DisplayStock = p.Batches
                        .SelectMany(b => b.BatchLocations)
                        .Where(bl => bl.LocationId == DisplayLocationId)
                        .Sum(bl => (int?)bl.CurrentStock) ?? 0,

                    WarehouseStock = p.Batches
                        .SelectMany(b => b.BatchLocations)
                        .Where(bl => bl.LocationId == WarehouseLocationId)
                        .Sum(bl => (int?)bl.CurrentStock) ?? 0,

                    ReservedStock = p.Batches
                        .SelectMany(b => b.BatchLocations)
                        .Where(bl => bl.LocationId == ReservedLocationId)
                        .Sum(bl => (int?)bl.CurrentStock) ?? 0,

                    TotalStock = p.Batches
                        .SelectMany(b => b.BatchLocations)
                        .Sum(bl => (int?)bl.CurrentStock) ?? 0,

                    RetailPrice = p.LinePresentation.ProductPrices
                        .Where(pp => pp.IsActive == true
                                  && pp.PriceTypeId == 1
                                  && pp.ProductId == null
                                  && (pp.ValidTo == null || pp.ValidTo >= now))
                        .Select(pp => (decimal?)pp.Price)
                        .FirstOrDefault(),

                    WholesalePrice = p.LinePresentation.ProductPrices
                        .Where(pp => pp.IsActive == true
                                  && pp.PriceTypeId == 2
                                  && pp.ProductId == null
                                  && (pp.ValidTo == null || pp.ValidTo >= now))
                        .Select(pp => (decimal?)pp.Price)
                        .FirstOrDefault()
                });
        }

    }
}
