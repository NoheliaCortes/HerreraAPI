using HerreraSystem.Application.DTOs.ReportDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Application.Interfaces.Services;

namespace HerreraSystem.Application.Services
{
    public class ReportsService : IReportsService
    {
        private const int TopCustomersLimit = 10;
        private readonly IReportsRepository _reportsRepository;

        public ReportsService(IReportsRepository reportsRepository) => _reportsRepository = reportsRepository;

        public Task<IReadOnlyList<SalesByLineFlavorDto>> GetSalesByLineFlavorAsync(ReportDateRangeParams range)
            => _reportsRepository.GetSalesByLineFlavorAsync(range.StartDate!.Value.Date, GetEndDateExclusive(range.EndDate!.Value));

        public Task<IReadOnlyList<MonthlySalesTrendDto>> GetMonthlySalesTrendAsync(ReportDateRangeParams range)
            => _reportsRepository.GetMonthlySalesTrendAsync(range.StartDate!.Value.Date, GetEndDateExclusive(range.EndDate!.Value));

        public Task<IReadOnlyList<SalesByGeographyDto>> GetSalesByGeographyAsync(ReportDateRangeParams range)
            => _reportsRepository.GetSalesByGeographyAsync(range.StartDate!.Value.Date, GetEndDateExclusive(range.EndDate!.Value));

        public Task<IReadOnlyList<TopCustomerDto>> GetTopCustomersAsync(ReportDateRangeParams range)
            => _reportsRepository.GetTopCustomersAsync(range.StartDate!.Value.Date, GetEndDateExclusive(range.EndDate!.Value), TopCustomersLimit);

        public Task<IReadOnlyList<SalesByPresentationDto>> GetSalesByPresentationAsync(ReportDateRangeParams range)
            => _reportsRepository.GetSalesByPresentationAsync(range.StartDate!.Value.Date, GetEndDateExclusive(range.EndDate!.Value));

        private static DateTime GetEndDateExclusive(DateTime endDate)
            => endDate.Date == DateTime.MaxValue.Date ? DateTime.MaxValue : endDate.Date.AddDays(1);
    }
}
