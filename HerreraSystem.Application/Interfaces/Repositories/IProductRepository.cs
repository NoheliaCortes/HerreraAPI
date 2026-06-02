using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.ProductDtos;
using System;
using System.Collections.Generic;
using System.Text;


namespace HerreraSystem.Application.Interfaces.Repositories
{
    public interface IProductRepository
    {
        Task<PagedResponse<ProductDto>> GetAllAsync(
        PaginationParams paginationParams);
        Task<ProductDto?> GetByIdAsync(int id);
        Task<ProductDto> CreateAsync(CreateProductDto dto);
        Task<bool> PatchAsync(int id, PatchProductDto dto);
        Task<bool> DeleteAsync(int id);
        Task<PagedResponse<ProductCatalogDto>> GetCatalogAsync(
        int? lineId,
        int? flavorId,
        string? search,
        bool? active,
        PaginationParams paginationParams);
        // Métodos de consulta para validaciones en el Service
        Task<bool> ExistsAsync(string productName, int linePresentationId, int flavorId, int? excludeId = null);
        Task<bool> HasBatchesAsync(int productId);
        Task<bool> HasActivePricesAsync(int productId);



    }
}
