using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.UploadDtos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HerreraSystem.Application.Interfaces.Services
{
    public interface IProductImageService
    {
        Task<ServiceResult<ProductImageUploadResponseDto>> UploadAsync(
            Stream fileStream,
            string fileName,
            long fileLength);
    }
}
