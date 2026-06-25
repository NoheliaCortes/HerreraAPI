using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Interfaces.Services
{
    public interface IFlavorImageService
    {
        Task<string?> SaveImageAsync(IFormFile? imageFile);
    }
}
