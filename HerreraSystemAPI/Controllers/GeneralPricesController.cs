using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.PricesDtos;
using HerreraSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HerreraSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeneralPricesController : ControllerBase
    {
        private readonly IGeneralPriceService _generalPriceService;

        public GeneralPricesController(IGeneralPriceService generalPriceService)
        {
            _generalPriceService = generalPriceService;
        }

        /// <summary>
        /// Retorna los precios generales agrupados por LinePresentation para la vista de gestion.
        /// </summary>
        /// <param name="lineId">Opcional. Filtra por linea.</param>
        [HttpGet("general")]
        public async Task<IActionResult> GetGeneralPrices([FromQuery] int? lineId)
        {
            var data = await _generalPriceService.GetGeneralPricesAsync(lineId);
            return Ok(ApiResponse<List<GeneralPriceDto>>.Ok(data));
        }

        /// <summary>
        /// Crea un precio general asignado a una combinacion LinePresentation.
        /// </summary>
        [HttpPost("general")]
        public async Task<IActionResult> CreateGeneralPrice(CreateGeneralPriceDto dto)
        {
            var result = await _generalPriceService.CreateGeneralPriceAsync(dto);
            if (!result.Success)
                return BadRequest(ApiResponse<GeneralPriceDetailDto>.Fail(result.ErrorMessage!));

            return CreatedAtAction(
                nameof(GetGeneralPriceHistory),
                new { linePresentationId = result.Data!.LinePresentationId },
                ApiResponse<GeneralPriceDetailDto>.Ok(result.Data, "Precio general creado exitosamente"));
        }

        /// <summary>
        /// Cambia un precio general conservando historial.
        /// </summary>
        [HttpPut("general/{linePresentationId:int}")]
        public async Task<IActionResult> ChangeGeneralPrice(int linePresentationId, ChangeGeneralPriceDto dto)
        {
            var result = await _generalPriceService.ChangeGeneralPriceAsync(linePresentationId, dto);
            if (!result.Success)
                return BadRequest(ApiResponse<GeneralPriceDetailDto>.Fail(result.ErrorMessage!));

            return Ok(ApiResponse<GeneralPriceDetailDto>.Ok(result.Data!, "Precio general actualizado exitosamente"));
        }

        /// <summary>
        /// Retorna los precios generales vigentes.
        /// </summary>
        [HttpGet("general/current")]
        public async Task<IActionResult> GetCurrentGeneralPrices(
            [FromQuery] int? lineId,
            [FromQuery] int? priceTypeId)
        {
            var data = await _generalPriceService.GetCurrentGeneralPricesAsync(lineId, priceTypeId);
            return Ok(ApiResponse<List<GeneralPriceDetailDto>>.Ok(data));
        }

        /// <summary>
        /// Retorna el historial paginado de precios generales.
        /// </summary>
        [HttpGet("general/history")]
        public async Task<IActionResult> GetGeneralPriceHistory(
            [FromQuery] int? linePresentationId,
            [FromQuery] int? priceTypeId,
            [FromQuery] PaginationParams paginationParams)
        {
            var data = await _generalPriceService.GetGeneralPriceHistoryAsync(
                linePresentationId,
                priceTypeId,
                paginationParams);

            return Ok(ApiResponse<PagedResponse<GeneralPriceDetailDto>>.Ok(data));
        }
    }
}
