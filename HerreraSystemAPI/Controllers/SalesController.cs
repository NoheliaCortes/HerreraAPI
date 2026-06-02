using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.SaleDtos;
using HerreraSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HerreraSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesController : ControllerBase
    {
        private readonly IRetailSaleService _retailSaleService;

        public SalesController(IRetailSaleService retailSaleService)
        {
            _retailSaleService = retailSaleService;
        }

        [HttpPost("retail")]
        public async Task<IActionResult> CreateRetailSale(CreateRetailSaleDto dto)
        {
            var result = await _retailSaleService.CreateRetailSaleAsync(dto);

            if (!result.Success)
                return BadRequest(ApiResponse<RetailSaleResponseDto>.Fail(result.ErrorMessage!));

            return CreatedAtAction(
                nameof(CreateRetailSale),
                ApiResponse<RetailSaleResponseDto>.Ok(result.Data!, "Venta registrada exitosamente"));
        }
    }
}
