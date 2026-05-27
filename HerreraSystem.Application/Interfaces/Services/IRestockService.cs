using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.RestockDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Interfaces.Services
{
    public interface IRestockService
    {
        Task<ServiceResult<RestockResponseDto>> CreateRestockAsync(CreateRestockDto dto);
    }
}
