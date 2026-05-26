using System;
using System.Collections.Generic;
using System.Text;
using HerreraSystem.Application.DTOs.InventoryDtos;

namespace HerreraSystem.Application.Interfaces.Repositories
{
    public interface IInventoryRepository
    {
        Task<List<InventoryProductDto>> GetInventoryProductsAsync(
            string? search,
            int? lineId,
            int? flavorId,
            int? presentationId);

    }
}
