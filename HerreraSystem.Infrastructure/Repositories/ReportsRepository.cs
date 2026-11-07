using HerreraSystem.Application.DTOs.ReportDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HerreraSystem.Infrastructure.Repositories
{
    public class ReportsRepository : IReportsRepository
    {
        private const int GenericCustomerId = 1;
        private readonly HerreraSystemContext _context;

        public ReportsRepository(HerreraSystemContext context) => _context = context;

        public async Task<IReadOnlyList<SalesByLineFlavorDto>> GetSalesByLineFlavorAsync(DateTime startDate, DateTime endDateExclusive)
            => await GetSaleDetails(startDate, endDateExclusive)
                .GroupBy(d => new { d.Product.LinePresentation.Line.LineName, d.Product.Flavor.FlavorName })
                .Select(g => new SalesByLineFlavorDto
                {
                    LineName = g.Key.LineName,
                    FlavorName = g.Key.FlavorName,
                    UnitsSold = g.Sum(d => d.Quantity),
                    TotalAmount = g.Sum(d => d.LineSubtotal ?? d.Quantity * d.AppliedPrice)
                })
                .OrderByDescending(x => x.UnitsSold).ThenBy(x => x.LineName).ThenBy(x => x.FlavorName)
                .ToListAsync();

        public async Task<IReadOnlyList<MonthlySalesTrendDto>> GetMonthlySalesTrendAsync(DateTime startDate, DateTime endDateExclusive)
            => await GetSaleDetails(startDate, endDateExclusive)
                .GroupBy(d => new { d.Sale.SaleDate!.Value.Year, d.Sale.SaleDate.Value.Month, d.Product.LinePresentation.Line.LineName })
                .Select(g => new MonthlySalesTrendDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    LineName = g.Key.LineName,
                    UnitsSold = g.Sum(d => d.Quantity)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month).ThenBy(x => x.LineName)
                .ToListAsync();

        public async Task<IReadOnlyList<SalesByGeographyDto>> GetSalesByGeographyAsync(DateTime startDate, DateTime endDateExclusive)
            => await GetSaleDetails(startDate, endDateExclusive)
                .Where(d => d.Sale.CustomerId != GenericCustomerId)
                .GroupBy(d => new { d.Sale.Customer.Municipality.Department.DepartmentName, d.Sale.Customer.Municipality.MunicipalityName })
                .Select(g => new SalesByGeographyDto
                {
                    DepartmentName = g.Key.DepartmentName,
                    MunicipalityName = g.Key.MunicipalityName,
                    UnitsSold = g.Sum(d => d.Quantity),
                    TotalAmount = g.Sum(d => d.LineSubtotal ?? d.Quantity * d.AppliedPrice)
                })
                .OrderByDescending(x => x.TotalAmount).ThenBy(x => x.DepartmentName).ThenBy(x => x.MunicipalityName)
                .ToListAsync();

        public async Task<IReadOnlyList<TopCustomerDto>> GetTopCustomersAsync(DateTime startDate, DateTime endDateExclusive, int limit)
            => await GetSaleDetails(startDate, endDateExclusive)
                .Where(d => d.Sale.CustomerId != GenericCustomerId)
                .GroupBy(d => new
                {
                    d.Sale.CustomerId,
                    d.Sale.Customer.FirstName,
                    d.Sale.Customer.LastName,
                    d.Sale.Customer.Municipality.MunicipalityName
                })
                .Select(g => new TopCustomerDto
                {
                    CustomerId = g.Key.CustomerId,
                    CustomerName = (g.Key.FirstName + " " + g.Key.LastName).Trim(),
                    MunicipalityName = g.Key.MunicipalityName,
                    UnitsPurchased = g.Sum(d => d.Quantity)
                })
                .OrderByDescending(x => x.UnitsPurchased).ThenBy(x => x.CustomerName).Take(limit)
                .ToListAsync();

        public async Task<IReadOnlyList<SalesByPresentationDto>> GetSalesByPresentationAsync(DateTime startDate, DateTime endDateExclusive)
        {
            var grouped = await GetSaleDetails(startDate, endDateExclusive)
                .GroupBy(d => new
                {
                    d.Product.LinePresentation.Presentation.PresentationName,
                    d.Product.LinePresentation.Line.LineName
                })
                .Select(g => new
                {
                    g.Key.PresentationName,
                    g.Key.LineName,
                    UnitsSold = g.Sum(d => d.Quantity)
                })
                .ToListAsync();

            var totalUnits = grouped.Sum(x => x.UnitsSold);
            return grouped
                .Select(x => new SalesByPresentationDto
                {
                    PresentationName = x.PresentationName,
                    LineName = x.LineName,
                    UnitsSold = x.UnitsSold,
                    ParticipationPercentage = totalUnits == 0 ? 0m : Math.Round(x.UnitsSold * 100m / totalUnits, 2)
                })
                .OrderByDescending(x => x.UnitsSold).ThenBy(x => x.PresentationName).ThenBy(x => x.LineName)
                .ToList();
        }

        private IQueryable<HerreraSystem.Domain.Entities.SaleDetail> GetSaleDetails(DateTime startDate, DateTime endDateExclusive)
            => _context.SaleDetails
                .AsNoTracking()
                .Where(d => d.Sale.CustomerId != GenericCustomerId &&
                            d.Sale.SaleDate.HasValue &&
                            d.Sale.SaleDate.Value >= startDate &&
                            d.Sale.SaleDate.Value < endDateExclusive);
    }
}
