using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.PricesDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Application.Interfaces.Services;
using static HerreraSystem.Application.Common.Constants;

namespace HerreraSystem.Application.Services
{
    public class GeneralPriceService : IGeneralPriceService
    {
        private readonly IGeneralPriceRepository _generalPriceRepository;

        public GeneralPriceService(IGeneralPriceRepository generalPriceRepository)
        {
            _generalPriceRepository = generalPriceRepository;
        }

        public async Task<List<GeneralPriceDto>> GetGeneralPricesAsync(int? lineId)
            => await _generalPriceRepository.GetGeneralPricesAsync(lineId);

        public async Task<ServiceResult<GeneralPriceDetailDto>> CreateGeneralPriceAsync(CreateGeneralPriceDto dto)
        {
            dto.PriceTypeId = PriceTypeConstants.Retail;

            var validationError = await ValidateGeneralPriceAsync(
                dto.LinePresentationId,
                dto.PriceTypeId,
                dto.Price,
                dto.ValidFrom,
                dto.ValidTo);

            if (validationError is not null)
                return ServiceResult<GeneralPriceDetailDto>.Fail(validationError);

            var hasOverlap = await _generalPriceRepository.HasOverlappingGeneralPriceAsync(
                dto.LinePresentationId,
                dto.PriceTypeId,
                dto.ValidFrom,
                dto.ValidTo);

            if (hasOverlap)
                return ServiceResult<GeneralPriceDetailDto>.Fail(
                    "Ya existe un precio general activo para esa línea, presentación, tipo de precio y rango de fechas");

            var created = await _generalPriceRepository.CreateGeneralPriceAsync(dto);
            return ServiceResult<GeneralPriceDetailDto>.Ok(created);
        }

        public async Task<ServiceResult<GeneralPriceDetailDto>> ChangeGeneralPriceAsync(
            int linePresentationId,
            ChangeGeneralPriceDto dto)
        {
            var validationError = await ValidateGeneralPriceAsync(
                linePresentationId,
                dto.PriceTypeId,
                dto.Price,
                dto.ValidFrom,
                dto.ValidTo);

            if (validationError is not null)
                return ServiceResult<GeneralPriceDetailDto>.Fail(validationError);

            var changed = await _generalPriceRepository.ChangeGeneralPriceAsync(linePresentationId, dto);
            if (changed is null)
                return ServiceResult<GeneralPriceDetailDto>.Fail("No se pudo crear el nuevo precio general");

            return ServiceResult<GeneralPriceDetailDto>.Ok(changed);
        }

        public async Task<List<GeneralPriceDetailDto>> GetCurrentGeneralPricesAsync(int? lineId, int? priceTypeId)
            => await _generalPriceRepository.GetCurrentGeneralPricesAsync(lineId, priceTypeId);

        public async Task<PagedResponse<GeneralPriceDetailDto>> GetGeneralPriceHistoryAsync(
            int? linePresentationId,
            int? priceTypeId,
            PaginationParams paginationParams)
        {
            return await _generalPriceRepository.GetGeneralPriceHistoryAsync(
                linePresentationId,
                priceTypeId,
                paginationParams);
        }

        public async Task<PriceStatisticsDto> GetStatisticsAsync()
            => await _generalPriceRepository.GetStatisticsAsync();

        private async Task<string?> ValidateGeneralPriceAsync(
            int linePresentationId,
            int priceTypeId,
            decimal price,
            DateTime validFrom,
            DateTime? validTo)
        {
            if (linePresentationId <= 0)
                return "La presentación de línea es requerida";

            if (priceTypeId <= 0)
                return "El tipo de precio es requerido";

            if (price <= 0)
                return "El precio debe ser mayor que cero";

            if (validFrom == default)
                return "La fecha inicial de vigencia es requerida";

            if (validTo.HasValue && validTo.Value < validFrom)
                return "La fecha final de vigencia no puede ser menor que la fecha inicial";

            var linePresentationExists = await _generalPriceRepository.LinePresentationExistsAsync(linePresentationId);
            if (!linePresentationExists)
                return $"La combinación de línea y presentación con Id {linePresentationId} no existe";

            var priceTypeExists = await _generalPriceRepository.PriceTypeExistsAsync(priceTypeId);
            if (!priceTypeExists)
                return $"El tipo de precio con Id {priceTypeId} no existe o no está activo";

            return null;
        }
    }
}
