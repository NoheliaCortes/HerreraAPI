using HerreraSystem.Application.DTOs.PricesDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Interfaces.Repositories
{
    public interface IGeneralPriceRepository
    {
        Task<List<GeneralPriceDto>> GetGeneralPricesAsync(int? lineId);
    }
}
