using HerreraSystem.Application.DTOs.ReportDtos;

namespace HerreraSystem.Application.Interfaces.Repositories
{
    public interface IReportsRepository
    {
        Task<IReadOnlyList<SalesByLineFlavorDto>> GetSalesByLineFlavorAsync(DateTime startDate, DateTime endDateExclusive);
        Task<IReadOnlyList<MonthlySalesTrendDto>> GetMonthlySalesTrendAsync(DateTime startDate, DateTime endDateExclusive);
        Task<IReadOnlyList<SalesByGeographyDto>> GetSalesByGeographyAsync(DateTime startDate, DateTime endDateExclusive);
        Task<IReadOnlyList<TopCustomerDto>> GetTopCustomersAsync(DateTime startDate, DateTime endDateExclusive, int limit);
        Task<IReadOnlyList<SalesByPresentationDto>> GetSalesByPresentationAsync(DateTime startDate, DateTime endDateExclusive);
    }
}
