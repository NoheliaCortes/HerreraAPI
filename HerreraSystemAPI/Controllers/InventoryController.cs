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
    }
}
