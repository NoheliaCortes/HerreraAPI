using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.InventoryDtos;
using HerreraSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HerreraSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        /// <summary>
        /// Vista general de inventario por producto con stocks por ubicación y precios vigentes.
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats([FromQuery] string period = "week")
        {
            var data = await _inventoryService.GetStatsAsync(period);

            return Ok(
                ApiResponse<InventoryStatsDto>.Ok(
                    data,
                    "Estadísticas de inventario obtenidas exitosamente"));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] int? lineId,
            [FromQuery] int? flavorId,
            [FromQuery] int? presentationId,
            [FromQuery] PaginationParams paginationParams)
        {
            var data = await _inventoryService.GetAllAsync(
                search, lineId, flavorId, presentationId, paginationParams);

            return Ok(
                ApiResponse<PagedResponse<InventoryProductDto>>.Ok(data));
        }

        [HttpGet("products")]
        public async Task<IActionResult> GetInventoryProducts(
            [FromQuery] string? search,
            [FromQuery] int? lineId,
            [FromQuery] int? flavorId,
            [FromQuery] int? presentationId)
        {
            var data = await _inventoryService.GetInventoryProductsAsync(
                search, lineId, flavorId, presentationId);

            return Ok(ApiResponse<List<InventoryProductDto>>.Ok(data));
        }

        [HttpGet("products/{productId}/batches")]
        public async Task<IActionResult> GetProductBatches(int productId)
        {
            var data = await _inventoryService.GetProductBatchesAsync(productId);

            if (data is null)
                return NotFound(
                    ApiResponse<InventoryProductBatchesDto>
                        .Fail($"Producto con Id {productId} no encontrado"));

            return Ok(ApiResponse<InventoryProductBatchesDto>.Ok(data));
        }

        [HttpGet("batches/{batchId}/detail")]
        public async Task<IActionResult> GetBatchDetail(int batchId)
        {
            var data = await _inventoryService.GetBatchDetailAsync(batchId);

            if (data is null)
                return NotFound(
                    ApiResponse<InventoryBatchDetailDto>
                        .Fail($"Lote con Id {batchId} no encontrado"));

            return Ok(ApiResponse<InventoryBatchDetailDto>.Ok(data));
        }
    }
}
