using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.SaleDtos;
using HerreraSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HerreraSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesController : ControllerBase
    {
        private readonly IRetailSaleService _retailSaleService;
        private readonly ISaleService _saleService;

        public SalesController(
            IRetailSaleService retailSaleService,
            ISaleService saleService)
        {
            _retailSaleService = retailSaleService;
            _saleService = saleService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var data = await _saleService.GetStatsAsync();

            return Ok(ApiResponse<SalesStatsDto>.Ok(data));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] SaleQueryParams queryParams)
        {
            var data = await _saleService.GetAllAsync(queryParams);

            return Ok(ApiResponse<PagedResponse<SaleListItemDto>>.Ok(data));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _saleService.GetByIdAsync(id);

            if (data is null)
                return NotFound(
                    ApiResponse<SaleHeaderDetailDto>.Fail($"Venta con Id {id} no encontrada"));

            return Ok(ApiResponse<SaleHeaderDetailDto>.Ok(data));
        }

        [HttpGet("{id}/details")]
        public async Task<IActionResult> GetDetails(int id)
        {
            var sale = await _saleService.GetByIdAsync(id);

            if (sale is null)
                return NotFound(
                    ApiResponse<List<SaleDetailItemDto>>.Fail($"Venta con Id {id} no encontrada"));

            var data = await _saleService.GetDetailsAsync(id);

            return Ok(ApiResponse<IReadOnlyList<SaleDetailItemDto>>.Ok(data));
        }

        [HttpGet("{id}/payments")]
        public async Task<IActionResult> GetPayments(int id)
        {
            var sale = await _saleService.GetByIdAsync(id);

            if (sale is null)
                return NotFound(
                    ApiResponse<List<SalePaymentDto>>.Fail($"Venta con Id {id} no encontrada"));

            var data = await _saleService.GetPaymentsAsync(id);

            return Ok(ApiResponse<IReadOnlyList<SalePaymentDto>>.Ok(data));
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
