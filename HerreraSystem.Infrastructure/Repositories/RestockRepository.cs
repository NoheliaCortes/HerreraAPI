using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.RestockDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Application.Interfaces.Services;
using HerreraSystem.Domain.Entities;
using HerreraSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HerreraSystem.Infrastructure.Repositories
{
    public class RestockRepository : IRestockRepository
    {
        private readonly HerreraSystemContext _context;
        private readonly INicaraguaDateTimeService _dateTimeService;

        public RestockRepository(
            HerreraSystemContext context,
            INicaraguaDateTimeService dateTimeService)
        {
            _context = context;
            _dateTimeService = dateTimeService;
        }

        public async Task<Restock> CreateAsync(Restock restock)
        {
            _context.Restocks.Add(restock);
            await _context.SaveChangesAsync();
            return restock;
        }

        public async Task<int> CountByYearAsync(int year)
        {
            return await _context.Restocks
                .Where(r => r.RestockDate.HasValue && r.RestockDate.Value.Year == year)
                .CountAsync();
        }

        public async Task<PagedResponse<RestockListItemDto>> GetAllAsync(RestockQueryParams queryParams)
        {
            var (fromDate, toDateExclusive) = GetDateRange(queryParams.FromDate, queryParams.ToDate);

            var query = _context.Restocks
                .AsNoTracking()
                .Where(r =>
                    r.RestockDate.HasValue &&
                    r.RestockDate.Value >= fromDate &&
                    r.RestockDate.Value < toDateExclusive);

            if (!string.IsNullOrWhiteSpace(queryParams.Search))
            {
                var search = queryParams.Search.Trim();

                query = query.Where(r =>
                    r.RestockCode.Contains(search) ||
                    r.CreatedByNavigation.UserName.Contains(search));
            }

            return await query
                .OrderByDescending(r => r.RestockDate)
                .ThenByDescending(r => r.Id)
                .Select(r => new RestockListItemDto
                {
                    RestockId = r.Id,
                    RestockCode = r.RestockCode,
                    RestockDate = r.RestockDate,
                    UserName = r.CreatedByNavigation.UserName,
                    BatchCount = r.Batches.Count(),
                    TotalUnits = r.Batches.Sum(b => (int?)b.InitialQuantity) ?? 0,
                    TotalInvestment = r.Batches
                        .Sum(b => (decimal?)(b.InitialQuantity * b.UnitProductionCost)) ?? 0m
                })
                .ToPagedResponseAsync(queryParams);
        }

        public async Task<RestockDetailDto?> GetDetailAsync(int id)
        {
            return await _context.Restocks
                .AsNoTracking()
                .Where(r => r.Id == id)
                .Select(r => new RestockDetailDto
                {
                    RestockId = r.Id,
                    RestockCode = r.RestockCode,
                    RestockDate = r.RestockDate,
                    UserName = r.CreatedByNavigation.UserName,
                    BatchCount = r.Batches.Count(),
                    TotalUnits = r.Batches.Sum(b => (int?)b.InitialQuantity) ?? 0,
                    TotalInvestment = r.Batches
                        .Sum(b => (decimal?)(b.InitialQuantity * b.UnitProductionCost)) ?? 0m,
                    DifferentProductsCount = r.Batches
                        .Select(b => b.ProductId)
                        .Distinct()
                        .Count(),
                    Batches = r.Batches
                        .OrderBy(b => b.Id)
                        .Select(b => new RestockDetailBatchDto
                        {
                            BatchId = b.Id,
                            BatchCode = b.BatchCode,
                            ProductName = b.Product.ProductName,
                            BatchStatusName = b.BatchStatus.BatchStatusName,
                            InitialQuantity = b.InitialQuantity,
                            UnitProductionCost = b.UnitProductionCost,
                            TotalCost = b.InitialQuantity * b.UnitProductionCost,
                            ExpirationDate = b.ExpirationDate
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<RestockStatisticsDto> GetStatisticsAsync()
        {
            var now = _dateTimeService.Now;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var nextMonthStart = monthStart.AddMonths(1);

            var restocksQuery = _context.Restocks
                .AsNoTracking()
                .Where(r =>
                    r.RestockDate.HasValue &&
                    r.RestockDate.Value >= monthStart &&
                    r.RestockDate.Value < nextMonthStart);

            return new RestockStatisticsDto
            {
                RestocksThisMonth = await restocksQuery.CountAsync(),
                TotalInvestmentThisMonth = await restocksQuery
                    .SelectMany(r => r.Batches)
                    .SumAsync(b => (decimal?)(b.InitialQuantity * b.UnitProductionCost)) ?? 0m,
                BatchesCreatedThisMonth = await restocksQuery
                    .SelectMany(r => r.Batches)
                    .CountAsync()
            };
        }

        private (DateTime FromDate, DateTime ToDateExclusive) GetDateRange(
            DateTime? fromDate,
            DateTime? toDate)
        {
            if (!fromDate.HasValue && !toDate.HasValue)
            {
                var now = _dateTimeService.Now;
                var monthStart = new DateTime(now.Year, now.Month, 1);

                return (monthStart, monthStart.AddMonths(1));
            }

            var from = (fromDate == null || fromDate == DateTime.MinValue)
                ? new DateTime(1753, 1, 1)
                : fromDate.Value.Date;

            DateTime toExclusive;

            if (toDate == null || toDate == DateTime.MaxValue || toDate.Value.Date == DateTime.MaxValue.Date)
            {
                toExclusive = DateTime.MaxValue;
            }
            else
            {
                toExclusive = toDate.Value.Date.AddDays(1);
            }

            return (from, toExclusive);
        }
    }
}
