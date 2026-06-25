using HerreraSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace HerreraSystem.API.Services
{
    public class FlavorImageService : IFlavorImageService
    {
        private readonly IWebHostEnvironment _environment;

        public FlavorImageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string?> SaveImageAsync(IFormFile? imageFile)
        {
            if (imageFile is null || imageFile.Length == 0)
                return null;

            var webRootPath = _environment.WebRootPath
                              ?? Path.Combine(_environment.ContentRootPath, "wwwroot");

            var uploadsFolder = Path.Combine(webRootPath, "uploads", "flavors");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var extension = Path.GetExtension(imageFile.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await imageFile.CopyToAsync(stream);

            return $"/uploads/flavors/{fileName}";
        }
    }
}