using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.InventoryDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using HerreraSystem.Application.Interfaces.Services;

namespace HerreraSystem.Application.Services
{
    public class InventoryService: IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;

        public InventoryService(IInventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public async Task<List<InventoryProductDto>> GetInventoryProductsAsync(
            string? search,
            int? lineId,
            int? flavorId,
            int? presentationId)
            => await _inventoryRepository.GetInventoryProductsAsync(
                search, lineId, flavorId, presentationId);

        public async Task<PagedResponse<InventoryProductDto>> GetAllAsync(
            string? search,
            int? lineId,
            int? flavorId,
            int? presentationId,
            PaginationParams paginationParams)
            => await _inventoryRepository.GetAllAsync(
                search, lineId, flavorId, presentationId, paginationParams);

        public async Task<InventoryProductBatchesDto?> GetProductBatchesAsync(int productId)
            => await _inventoryRepository.GetProductBatchesAsync(productId);

        public async Task<InventoryBatchDetailDto?> GetBatchDetailAsync(int batchId)
            => await _inventoryRepository.GetBatchDetailAsync(batchId);

        public async Task<InventoryStatsDto> GetStatsAsync(string period)
            => await _inventoryRepository.GetStatsAsync(period);

    }
}
