using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.PricesDtos;

namespace HerreraSystem.Application.Interfaces.Services
{
    public interface IGeneralPriceService
    {
        Task<List<GeneralPriceDto>> GetGeneralPricesAsync(int? lineId);

        Task<ServiceResult<GeneralPriceDetailDto>> CreateGeneralPriceAsync(CreateGeneralPriceDto dto);

        Task<ServiceResult<GeneralPriceDetailDto>> ChangeGeneralPriceAsync(int linePresentationId, ChangeGeneralPriceDto dto);

        Task<List<GeneralPriceDetailDto>> GetCurrentGeneralPricesAsync(int? lineId, int? priceTypeId);

        Task<PagedResponse<GeneralPriceDetailDto>> GetGeneralPriceHistoryAsync(
            int? linePresentationId,
            int? priceTypeId,
            PaginationParams paginationParams);

        Task<PriceStatisticsDto> GetStatisticsAsync();
    }
}
