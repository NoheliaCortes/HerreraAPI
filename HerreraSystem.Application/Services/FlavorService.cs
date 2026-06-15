using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.FlavorDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Application.Interfaces.Services;
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

        public async Task<ServiceResult<FlavorDto>> CreateAsync(
            CreateFlavorDto dto)
        {
            var exists = await _flavorRepository
                .ExistsAsync(dto.FlavorName);

            if (exists)
                return ServiceResult<FlavorDto>
                    .Fail("Ya existe un sabor con ese nombre");

            var created = await _flavorRepository
                .CreateAsync(dto);

            return ServiceResult<FlavorDto>
                .Ok(created);
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
