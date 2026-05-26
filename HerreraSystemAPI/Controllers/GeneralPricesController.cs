using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.PricesDtos;
using HerreraSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
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
        /// Retorna los precios generales agrupados por LinePresentation para la vista de gestión.
        /// </summary>
        /// <param name="lineId">Opcional. Filtra por línea (para los tabs del frontend).</param>
        [HttpGet("general")]
        public async Task<IActionResult> GetGeneralPrices([FromQuery] int? lineId)
        {
            var data = await _generalPriceService.GetGeneralPricesAsync(lineId);
            return Ok(ApiResponse<List<GeneralPriceDto>>.Ok(data));
        }

    }
}
