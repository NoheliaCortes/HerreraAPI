using HerreraSystem.Application.DTOs.InventoryDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Interfaces.Services
{
    public interface IInventoryService
    {
        Task<List<InventoryProductDto>> GetInventoryProductsAsync(
           string? search,
           int? lineId,
           int? flavorId,
           int? presentationId);
    }
}
