using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.SaleDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Interfaces.Services
{
    public interface IRetailSaleService
    {
        Task<ServiceResult<RetailSaleResponseDto>> CreateRetailSaleAsync(CreateRetailSaleDto dto);
    }
}
