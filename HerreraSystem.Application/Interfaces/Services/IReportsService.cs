using HerreraSystem.Application.DTOs.ReportDtos;

namespace HerreraSystem.Application.Interfaces.Services
{
    public interface IReportsService
    {
        Task<IReadOnlyList<SalesByLineFlavorDto>> GetSalesByLineFlavorAsync(ReportDateRangeParams range);
        Task<IReadOnlyList<MonthlySalesTrendDto>> GetMonthlySalesTrendAsync(ReportDateRangeParams range);
        Task<IReadOnlyList<SalesByGeographyDto>> GetSalesByGeographyAsync(ReportDateRangeParams range);
        Task<IReadOnlyList<TopCustomerDto>> GetTopCustomersAsync(ReportDateRangeParams range);
        Task<IReadOnlyList<SalesByPresentationDto>> GetSalesByPresentationAsync(ReportDateRangeParams range);
    }
}
