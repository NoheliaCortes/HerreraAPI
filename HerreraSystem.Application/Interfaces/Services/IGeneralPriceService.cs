using HerreraSystem.Application.DTOs.PricesDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Interfaces.Services
{
    public interface IGeneralPriceService
    {
        Task<List<GeneralPriceDto>> GetGeneralPricesAsync(int? lineId);
    }
}
