using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.PricesDtos;
using HerreraSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HerreraSystem.API.Controllers
{
    [Route("api/prices")]
    [ApiController]
    public class PricesController : ControllerBase
    {
        private readonly IGeneralPriceService _generalPriceService;

        public PricesController(IGeneralPriceService generalPriceService)
        {
            _generalPriceService = generalPriceService;
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var data = await _generalPriceService.GetStatisticsAsync();
            return Ok(ApiResponse<PriceStatisticsDto>.Ok(data));
        }
    }
}
