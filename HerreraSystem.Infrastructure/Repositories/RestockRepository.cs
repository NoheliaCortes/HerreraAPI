using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.RestockDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Domain.Entities;
using HerreraSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HerreraSystem.Infrastructure.Repositories
{
    public class RestockRepository : IRestockRepository
    {
        private readonly HerreraSystemContext _context;

        public RestockRepository(HerreraSystemContext context)
        {
            _context = context;
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
            var now = DateTime.UtcNow;
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

        private static (DateTime FromDate, DateTime ToDateExclusive) GetDateRange(
     DateTime? fromDate,
     DateTime? toDate)
        {
            // 1. Si ambas fechas son nulas, mantenemos tu lógica del mes actual
            if (!fromDate.HasValue && !toDate.HasValue)
            {
                var now = DateTime.UtcNow;
                var monthStart = new DateTime(now.Year, now.Month, 1);

                return (monthStart, monthStart.AddMonths(1));
            }

            // 2. Normalizar fecha de inicio (Si es nulo o viene por defecto MinValue de C#)
            var from = (fromDate == null || fromDate == DateTime.MinValue)
                ? new DateTime(1753, 1, 1) // Fecha mínima segura para la mayoría de bases de datos (SQL Server)
                : fromDate.Value.Date;

            // 3. Normalizar fecha de fin de manera segura sin desbordar el DateTime
            DateTime toExclusive;

            if (toDate == null || toDate == DateTime.MaxValue || toDate.Value.Date == DateTime.MaxValue.Date)
            {
                // Si no mandaron fecha "Hasta", ponemos el límite máximo representable en C# 
                // sin necesidad de sumarle días (para evitar el desborde)
                toExclusive = DateTime.MaxValue;
            }
            else
            {
                // Si mandaron una fecha válida y segura, le sumamos el día de manera normal 
                // para que el filtro "menor que (<)" en el Where funcione a la perfección
                toExclusive = toDate.Value.Date.AddDays(1);
            }

            return (from, toExclusive);
        }
    }
}
