using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.PricesDtos;

namespace HerreraSystem.Application.Interfaces.Repositories
{
    public interface IGeneralPriceRepository
    {
        Task<List<GeneralPriceDto>> GetGeneralPricesAsync(int? lineId);

        Task<bool> LinePresentationExistsAsync(int linePresentationId);

        Task<bool> PriceTypeExistsAsync(int priceTypeId);

        Task<bool> HasOverlappingGeneralPriceAsync(
            int linePresentationId,
            int priceTypeId,
            DateTime validFrom,
            DateTime? validTo,
            int? excludeId = null);

        Task<GeneralPriceDetailDto> CreateGeneralPriceAsync(CreateGeneralPriceDto dto);

        Task<GeneralPriceDetailDto?> ChangeGeneralPriceAsync(int linePresentationId, ChangeGeneralPriceDto dto);

        Task<List<GeneralPriceDetailDto>> GetCurrentGeneralPricesAsync(int? lineId, int? priceTypeId);

        Task<PagedResponse<GeneralPriceDetailDto>> GetGeneralPriceHistoryAsync(
            int? linePresentationId,
            int? priceTypeId,
            PaginationParams paginationParams);

        Task<PriceStatisticsDto> GetStatisticsAsync();
    }
}
