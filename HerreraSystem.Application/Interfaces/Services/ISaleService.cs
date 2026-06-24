using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.SaleDtos;

namespace HerreraSystem.Application.Interfaces.Services
{
    public interface ISaleService
    {
        Task<SalesStatsDto> GetStatsAsync();

        Task<PagedResponse<SaleListItemDto>> GetAllAsync(SaleQueryParams queryParams);

        Task<SaleHeaderDetailDto?> GetByIdAsync(int id);

        Task<IReadOnlyList<SaleDetailItemDto>> GetDetailsAsync(int id);

        Task<IReadOnlyList<SalePaymentDto>> GetPaymentsAsync(int id);
    }
}
