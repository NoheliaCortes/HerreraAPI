using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.UploadDtos;
using HerreraSystem.Application.Interfaces.Services;

namespace HerreraSystem.API.Services
{
    public class ProductImageService : IProductImageService
    {
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        private readonly IWebHostEnvironment _environment;

        public ProductImageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<ServiceResult<ProductImageUploadResponseDto>> UploadAsync(
            Stream fileStream,
            string fileName,
            long fileLength)
        {
            if (fileStream is null)
                return ServiceResult<ProductImageUploadResponseDto>.Fail("El archivo es obligatorio");

            if (fileLength <= 0)
                return ServiceResult<ProductImageUploadResponseDto>.Fail("El archivo no puede estar vacío");

            var extension = Path.GetExtension(fileName);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
                return ServiceResult<ProductImageUploadResponseDto>.Fail("Extensión de imagen no permitida");

            var webRootPath = _environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRootPath))
                webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");

            var uploadsPath = Path.Combine(webRootPath, "uploads", "products");
            Directory.CreateDirectory(uploadsPath);

            var storedFileName = $"{Guid.NewGuid():D}{extension.ToLowerInvariant()}";
            var destinationPath = Path.Combine(uploadsPath, storedFileName);

            await using var outputStream = new FileStream(destinationPath, FileMode.CreateNew);
            await fileStream.CopyToAsync(outputStream);

            return ServiceResult<ProductImageUploadResponseDto>.Ok(
                new ProductImageUploadResponseDto
                {
                    ImageUrl = $"/uploads/products/{storedFileName}"
                });
        }
    }
}
