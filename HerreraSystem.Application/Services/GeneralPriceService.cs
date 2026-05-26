using HerreraSystem.Application.DTOs.PricesDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using HerreraSystem.Application.Interfaces.Services;

namespace HerreraSystem.Application.Services
{
    public class GeneralPriceService : IGeneralPriceService
    {
        private readonly IGeneralPriceRepository _generalPriceRepository;

        public GeneralPriceService(IGeneralPriceRepository generalPriceRepository)
        {
            _generalPriceRepository = generalPriceRepository;
        }

        public async Task<List<GeneralPriceDto>> GetGeneralPricesAsync(int? lineId)
            => await _generalPriceRepository.GetGeneralPricesAsync(lineId);
    }
}
