using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.SaleDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Domain.Entities;
using HerreraSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HerreraSystem.Infrastructure.Repositories
{
    public class SaleRepository : ISaleRepository
    {
        private readonly HerreraSystemContext _context;

        public SaleRepository(HerreraSystemContext context)
        {
            _context = context;
        }

        public async Task<Sale> CreateAsync(Sale sale)
        {
            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();
            return sale;
        }

        public async Task<int> CountByYearAsync(int year)
        {
            return await _context.Sales
                .Where(s => s.SaleDate.HasValue && s.SaleDate.Value.Year == year)
                .CountAsync();
        }

        public async Task<SalesStatsDto> GetStatsAsync()
        {
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var nextMonthStart = monthStart.AddMonths(1);

            var salesQuery = _context.Sales
                .AsNoTracking()
                .Where(s =>
                    s.SaleDate.HasValue &&
                    s.SaleDate.Value >= monthStart &&
                    s.SaleDate.Value < nextMonthStart);

            return new SalesStatsDto
            {
                SalesThisMonth = await salesQuery.CountAsync(),
                TotalIncomeThisMonth = await salesQuery
                    .SumAsync(s => (decimal?)s.TotalSale) ?? 0m,
                ProductsSoldThisMonth = await salesQuery
                    .SelectMany(s => s.SaleDetails)
                    .SumAsync(d => (int?)d.Quantity) ?? 0
            };
        }

        public async Task<PagedResponse<SaleListItemDto>> GetAllAsync(SaleQueryParams queryParams)
        {
            var (startDate, endDateExclusive) = GetDateRange(
                queryParams.StartDate,
                queryParams.EndDate);

            return await _context.Sales
                .AsNoTracking()
                .Where(s =>
                    s.SaleDate.HasValue &&
                    s.SaleDate.Value >= startDate &&
                    s.SaleDate.Value < endDateExclusive)
                .OrderByDescending(s => s.SaleDate)
                .ThenByDescending(s => s.Id)
                .Select(s => new SaleListItemDto
                {
                    Id = s.Id,
                    SaleCode = s.SaleCode,
                    SaleDate = s.SaleDate,
                    CustomerName = ((s.Customer.FirstName + " " + s.Customer.LastName).Trim() == string.Empty)
                        ? "Cliente generico"
                        : (s.Customer.FirstName + " " + s.Customer.LastName).Trim(),
                    SaleTypeId = s.SaleTypeId,
                    SaleTypeName = s.SaleType.SaleTypeName,
                    PaymentTypeId = s.PaymentTypeId,
                    PaymentTypeName = s.PaymentType.PaymentTypeName,
                    Total = s.TotalSale
                })
                .ToPagedResponseAsync(queryParams);
        }

        public async Task<SaleHeaderDetailDto?> GetByIdAsync(int id)
        {
            return await _context.Sales
                .AsNoTracking()
                .Where(s => s.Id == id)
                .Select(s => new SaleHeaderDetailDto
                {
                    Id = s.Id,
                    SaleCode = s.SaleCode,
                    SaleDate = s.SaleDate,
                    OrderCode = s.Order == null ? null : s.Order.OrderCode,
                    Customer = new SaleCustomerInfoDto
                    {
                        Id = s.Customer.Id,
                        FullName = ((s.Customer.FirstName + " " + s.Customer.LastName).Trim() == string.Empty)
                            ? "Cliente generico"
                            : (s.Customer.FirstName + " " + s.Customer.LastName).Trim(),
                        DepartmentName = s.Customer.Municipality.Department.DepartmentName,
                        MunicipalityName = s.Customer.Municipality.MunicipalityName,
                        PointOfSale = s.Customer.PointOfSale
                    },
                    Total = s.TotalSale,
                    PaymentStatusName = s.PaymentStatus,
                    PendingBalance = s.PendingBalance,
                    CreatedByUserName = s.CreatedByNavigation.UserName,
                    PaymentTypeId = s.PaymentTypeId,
                    PaymentTypeName = s.PaymentType.PaymentTypeName,
                    SaleTypeId = s.SaleTypeId,
                    SaleTypeName = s.SaleType.SaleTypeName
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<SaleDetailItemDto>> GetDetailsAsync(int id)
        {
            return await _context.SaleDetails
                .AsNoTracking()
                .Where(d => d.SaleId == id)
                .OrderBy(d => d.Id)
                .Select(d => new SaleDetailItemDto
                {
                    Id = d.Id,
                    ProductId = d.ProductId,
                    ProductName = d.Product.ProductName,
                    BatchCode = d.Batch.BatchCode ?? string.Empty,
                    Quantity = d.Quantity,
                    UnitPrice = d.AppliedPrice,
                    LineSubtotal = d.LineSubtotal ?? d.Quantity * d.AppliedPrice
                })
                .ToListAsync();
        }

        public async Task<IReadOnlyList<SalePaymentDto>> GetPaymentsAsync(int id)
        {
            return await _context.Payments
                .AsNoTracking()
                .Where(p => p.SaleId == id)
                .OrderByDescending(p => p.PaymentDate)
                .ThenByDescending(p => p.Id)
                .Select(p => new SalePaymentDto
                {
                    Id = p.Id,
                    Amount = p.AmountReceived,
                    PaymentMethodName = p.PaymentMethod.PaymentMethodName,
                    PaymentDate = p.PaymentDate,
                    TransactionReference = p.TransactionReference,
                    RegisteredByUserName = p.RegisteredByNavigation.UserName
                })
                .ToListAsync();
        }

        private static (DateTime StartDate, DateTime EndDateExclusive) GetDateRange(
            DateTime? startDate,
            DateTime? endDate)
        {
            if (!startDate.HasValue && !endDate.HasValue)
            {
                var now = DateTime.UtcNow;
                var monthStart = new DateTime(now.Year, now.Month, 1);

                return (monthStart, monthStart.AddMonths(1));
            }

            var from = (startDate == null || startDate == DateTime.MinValue)
                ? new DateTime(1753, 1, 1)
                : startDate.Value.Date;

            DateTime toExclusive;

            if (endDate == null || endDate == DateTime.MaxValue || endDate.Value.Date == DateTime.MaxValue.Date)
            {
                toExclusive = DateTime.MaxValue;
            }
            else
            {
                toExclusive = endDate.Value.Date.AddDays(1);
            }

            return (from, toExclusive);
        }
    }
}
