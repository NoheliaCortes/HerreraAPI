using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.ProductDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HerreraSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
        [FromQuery] PaginationParams paginationParams)
        {
            var data = await _productService
                .GetAllAsync(paginationParams);

            return Ok(
                ApiResponse<PagedResponse<ProductDto>>.Ok(data));
        }

        [HttpGet("catalog")]
        public async Task<IActionResult> GetCatalog(
    [FromQuery] int? lineId,
    [FromQuery] int? flavorId,
    [FromQuery] string? search,
    [FromQuery] bool? active,
    [FromQuery] PaginationParams paginationParams)
        {
            var data = await _productService.GetCatalogAsync(
                lineId,
                flavorId,
                search,
                active,
                paginationParams);

            return Ok(
                ApiResponse<PagedResponse<ProductCatalogDto>>
                    .Ok(data));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _productService.GetByIdAsync(id);
            if (!result.Success)
                return NotFound(ApiResponse<ProductDto>.Fail(result.ErrorMessage!));

            return Ok(ApiResponse<ProductDto>.Ok(result.Data!));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductDto dto)
        {
            var result = await _productService.CreateAsync(dto);
            if (!result.Success)
                return BadRequest(ApiResponse<ProductDto>.Fail(result.ErrorMessage!));

            return CreatedAtAction(nameof(GetById),
                new { id = result.Data!.Id },
                ApiResponse<ProductDto>.Ok(result.Data, "Producto creado exitosamente"));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateProductDto dto)
        {
            var result = await _productService.UpdateAsync(id, dto);
            if (!result.Success)
            {
                if (result.ErrorMessage!.Contains("no encontrado"))
                    return NotFound(ApiResponse<object>.Fail(result.ErrorMessage));

                return BadRequest(ApiResponse<object>.Fail(result.ErrorMessage));
            }

            return Ok(ApiResponse<object>.Ok(null!, "Producto actualizado exitosamente"));
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(int id, PatchProductDto dto)
        {
            var result = await _productService.PatchAsync(id, dto);
            if (!result.Success)
            {
                if (result.ErrorMessage!.Contains("no encontrado"))
                    return NotFound(ApiResponse<object>.Fail(result.ErrorMessage));

                return BadRequest(ApiResponse<object>.Fail(result.ErrorMessage));
            }

            return Ok(ApiResponse<object>.Ok(null!, "Producto actualizado exitosamente"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.DeleteAsync(id);
            if (!result.Success)
            {
                if (result.ErrorMessage!.Contains("no encontrado"))
                    return NotFound(ApiResponse<object>.Fail(result.ErrorMessage));

                return BadRequest(ApiResponse<object>.Fail(result.ErrorMessage));
            }

            return Ok(ApiResponse<object>.Ok(null!, "Producto eliminado exitosamente"));

        }
    }
}
