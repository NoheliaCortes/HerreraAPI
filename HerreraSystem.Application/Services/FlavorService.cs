using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.FlavorDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Application.Interfaces.Services;
using HerreraSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Services
{
    public class FlavorService : IFlavorService
    {
        private readonly IFlavorRepository _flavorRepository;

        public FlavorService(IFlavorRepository flavorRepository)
        {
            _flavorRepository = flavorRepository;
        }

        public async Task<PagedResponse<FlavorDto>> GetAllAsync(
            PaginationParams paginationParams)
        {
            return await _flavorRepository
                .GetAllAsync(paginationParams);
        }

        public async Task<ServiceResult<FlavorDto>> GetByIdAsync(int id)
        {
            var flavor = await _flavorRepository.GetByIdAsync(id);

            if (flavor is null)
                return ServiceResult<FlavorDto>
                    .Fail($"Sabor con Id {id} no encontrado");

            return ServiceResult<FlavorDto>.Ok(flavor);
        }

        public async Task<ServiceResult<FlavorDto>> CreateAsync(CreateFlavorDto dto)
        {
            // 1. Crear la entidad (Flavor)
            var flavor = new Flavor
            {
                FlavorName = dto.FlavorName,
                FlavorColor = dto.FlavorColor,
                IsActive = dto.IsActive
            };

            // 2. CORRECCIÓN: Procesar la imagen ANTES de llegar al repositorio
            if (dto.ImageURL != null)
            {
                // Usas tu servicio de imágenes para guardar el archivo físicamente
                // y este te devuelve la RUTA (string)
                string imageUrl = await _productImageService.UploadImageAsync(dto.ImageURL);

                // Ahora asignas el string, no el archivo
                flavor.ImageUrl = imageUrl;
            }

            // 3. Guardar en el repositorio (Ahora flavor ya tiene un string en ImageUrl)
            await _flavorRepository.AddAsync(flavor);
            await _unitOfWork.SaveChangesAsync();

            // 4. Retornar el DTO
            return ServiceResult<FlavorDto>.Ok(new FlavorDto { /* ... mapeo ... */ });
        }

        public async Task<ServiceResult<bool>> UpdateAsync(
            int id,
            UpdateFlavorDto dto)
        {
            var flavor = await _flavorRepository
                .GetByIdAsync(id);

            if (flavor is null)
                return ServiceResult<bool>
                    .Fail($"Sabor con Id {id} no encontrado");

            var exists = await _flavorRepository
                .ExistsAsync(dto.FlavorName, id);

            if (exists)
                return ServiceResult<bool>
                    .Fail("Ya existe un sabor con ese nombre");

            var updated = await _flavorRepository
                .UpdateAsync(id, dto);

            return ServiceResult<bool>
                .Ok(updated);
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            var flavor = await _flavorRepository
                .GetByIdAsync(id);

            if (flavor is null)
                return ServiceResult<bool>
                    .Fail($"Sabor con Id {id} no encontrado");

            var hasProducts = await _flavorRepository
                .HasProductsAsync(id);

            if (hasProducts)
                return ServiceResult<bool>
                    .Fail("No se puede eliminar el sabor porque tiene productos asociados");

            var deleted = await _flavorRepository
                .DeleteAsync(id);

            return ServiceResult<bool>
                .Ok(deleted);
        }

    }
}
