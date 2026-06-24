using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.SaleDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Application.Interfaces.Services;

namespace HerreraSystem.Application.Services
{
    public class SaleService : ISaleService
    {
        private readonly ISaleRepository _saleRepository;

        public SaleService(ISaleRepository saleRepository)
        {
            _saleRepository = saleRepository;
        }

        public async Task<SalesStatsDto> GetStatsAsync()
            => await _saleRepository.GetStatsAsync();

        public async Task<PagedResponse<SaleListItemDto>> GetAllAsync(SaleQueryParams queryParams)
            => await _saleRepository.GetAllAsync(queryParams);

        public async Task<SaleHeaderDetailDto?> GetByIdAsync(int id)
            => await _saleRepository.GetByIdAsync(id);

        public async Task<IReadOnlyList<SaleDetailItemDto>> GetDetailsAsync(int id)
            => await _saleRepository.GetDetailsAsync(id);

        public async Task<IReadOnlyList<SalePaymentDto>> GetPaymentsAsync(int id)
            => await _saleRepository.GetPaymentsAsync(id);
    }
}
