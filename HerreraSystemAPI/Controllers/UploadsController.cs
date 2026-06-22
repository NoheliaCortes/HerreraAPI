using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.UploadDtos;
using HerreraSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HerreraSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadsController : ControllerBase
    {
        private readonly IProductImageService _productImageService;

        public UploadsController(IProductImageService productImageService)
        {
            _productImageService = productImageService;
        }

        [HttpPost("products")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadProductImage(IFormFile? file)
        {
            if (file is null)
                return BadRequest(ApiResponse<object>.Fail("El archivo es obligatorio"));

            await using var fileStream = file.OpenReadStream();
            var result = await _productImageService.UploadAsync(
                fileStream,
                file.FileName,
                file.Length);

            if (!result.Success)
                return BadRequest(ApiResponse<object>.Fail(result.ErrorMessage!));

            return Ok(
                ApiResponse<ProductImageUploadResponseDto>.Ok(
                    result.Data!,
                    "Imagen subida correctamente"));
        }
    }
}
