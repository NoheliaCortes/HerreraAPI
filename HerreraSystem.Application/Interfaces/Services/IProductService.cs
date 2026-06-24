using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.ProductDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Interfaces.Services
{
    public interface IProductService
    {
        Task<PagedResponse<ProductDto>> GetAllAsync(
        PaginationParams paginationParams);

        Task<ServiceResult<ProductDto>> GetByIdAsync(int id);
        Task<PagedResponse<ProductCatalogDto>> GetCatalogAsync(
        int? lineId,
        int? presentationId,
        int? flavorId,
        string? search,
        bool? active,
        PaginationParams paginationParams);
        Task<ServiceResult<List<ProductSelectionDto>>> GetByLinePresentationAsync(
            int linePresentationId);
        Task<ProductStatsDto> GetStatsAsync();
        Task<ServiceResult<ProductDto>> CreateAsync(CreateProductDto dto);
        Task<ServiceResult<bool>> UpdateAsync(int id, UpdateProductDto dto);
        Task<ServiceResult<bool>> PatchAsync(int id, PatchProductDto dto);
        Task<ServiceResult<bool>> DeleteAsync(int id);

    }
}
