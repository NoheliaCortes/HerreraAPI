using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.ProductDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Services
{
    public class ProductService:IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IFlavorRepository _flavorRepository;
        private readonly ILinePresentationRepository _linePresentationRepository;

        public ProductService(
            IProductRepository productRepository,
            IFlavorRepository flavorRepository,
            ILinePresentationRepository linePresentationRepository)
        {
            _productRepository = productRepository;
            _flavorRepository = flavorRepository;
            _linePresentationRepository = linePresentationRepository;
        }

        // ── Consultas ────────────────────────────────────────────────────────

        public async Task<PagedResponse<ProductDto>> GetAllAsync(
        PaginationParams paginationParams)
        {
            return await _productRepository
                .GetAllAsync(paginationParams);
        }
        public async Task<ServiceResult<ProductDto>> GetByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product is null)
                return ServiceResult<ProductDto>.Fail($"Producto con Id {id} no encontrado");

            return ServiceResult<ProductDto>.Ok(product);
        }

        public async Task<PagedResponse<ProductCatalogDto>> GetCatalogAsync(
        int? lineId,
        int? flavorId,
        string? search,
        bool? active,
        PaginationParams paginationParams)
        {
            return await _productRepository.GetCatalogAsync(
                lineId,
                flavorId,
                search,
                active,
                paginationParams);
        }

        // ── Operaciones con lógica de negocio ────────────────────────────────

        public async Task<ServiceResult<ProductDto>> CreateAsync(CreateProductDto dto)
        {
            var flavor = await _flavorRepository.GetByIdAsync(dto.FlavorId);
            if (flavor is null)
                return ServiceResult<ProductDto>.Fail(
                    $"El sabor con Id {dto.FlavorId} no existe");

            var linePresentation = await _linePresentationRepository
                .GetByIdAsync(dto.LinePresentationId);
            if (linePresentation is null)
                return ServiceResult<ProductDto>.Fail(
                    $"La presentación de línea con Id {dto.LinePresentationId} no existe");

            var isDuplicate = await _productRepository.ExistsAsync(
                dto.ProductName, dto.LinePresentationId, dto.FlavorId);
            if (isDuplicate)
                return ServiceResult<ProductDto>.Fail(
                    $"Ya existe un producto '{dto.ProductName}' con esa línea y sabor");

            var created = await _productRepository.CreateAsync(dto);
            return ServiceResult<ProductDto>.Ok(created);
        }

        public async Task<ServiceResult<bool>> PatchAsync(int id, PatchProductDto dto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product is null)
                return ServiceResult<bool>.Fail($"Producto con Id {id} no encontrado");

            if (dto.FlavorId.HasValue)
            {
                var flavor = await _flavorRepository.GetByIdAsync(dto.FlavorId.Value);
                if (flavor is null)
                    return ServiceResult<bool>.Fail(
                        $"El sabor con Id {dto.FlavorId} no existe");
            }

            if (dto.LinePresentationId.HasValue)
            {
                var lp = await _linePresentationRepository
                    .GetByIdAsync(dto.LinePresentationId.Value);
                if (lp is null)
                    return ServiceResult<bool>.Fail(
                        $"La presentación de línea con Id {dto.LinePresentationId} no existe");
            }

            if (dto.ProductName is not null)
            {
                var targetLinePresentationId = dto.LinePresentationId ?? product.LinePresentationId;
                var targetFlavorId = dto.FlavorId ?? product.FlavorId;

                var isDuplicate = await _productRepository.ExistsAsync(
                    dto.ProductName, targetLinePresentationId, targetFlavorId, excludeId: id);
                if (isDuplicate)
                    return ServiceResult<bool>.Fail(
                        $"Ya existe un producto '{dto.ProductName}' con esa línea y sabor");
            }

            await _productRepository.PatchAsync(id, dto);
            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product is null)
                return ServiceResult<bool>.Fail($"Producto con Id {id} no encontrado");

            var hasBatches = await _productRepository.HasBatchesAsync(id);
            if (hasBatches)
                return ServiceResult<bool>.Fail(
                    $"No se puede eliminar '{product.ProductName}' porque tiene lotes registrados");

            var hasActivePrices = await _productRepository.HasActivePricesAsync(id);
            if (hasActivePrices)
                return ServiceResult<bool>.Fail(
                    $"No se puede eliminar '{product.ProductName}' porque tiene precios activos");

            await _productRepository.DeleteAsync(id);
            return ServiceResult<bool>.Ok(true);
        }


    }
}
