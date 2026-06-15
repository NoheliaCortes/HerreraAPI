using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.MunicipalityDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using HerreraSystem.Application.Interfaces.Services;

namespace HerreraSystem.Application.Services
{
    public class MunicipalityService: IMunicipalityService
    {
        private readonly IMunicipalityRepository _municipalityRepository;

        public MunicipalityService(IMunicipalityRepository municipalityRepository)
        {
            _municipalityRepository = municipalityRepository;
        }

        public async Task<IReadOnlyList<MunicipalityDto>> GetAllAsync()
        {
            return await _municipalityRepository.GetAllAsync();
        }

        public async Task<ServiceResult<IReadOnlyList<MunicipalityDto>>> GetByDepartmentAsync(
            int departmentId)
        {
            var municipalities = await _municipalityRepository
                .GetByDepartmentAsync(departmentId);

            // Retorna lista vacía como Ok — es válido que un departamento
            // no tenga municipios activos aún, no es un error
            return ServiceResult<IReadOnlyList<MunicipalityDto>>
                .Ok(municipalities);
        }

        public async Task<ServiceResult<MunicipalityDto>> GetByIdAsync(int id)
        {
            var municipality = await _municipalityRepository.GetByIdAsync(id);

            if (municipality is null)
                return ServiceResult<MunicipalityDto>
                    .Fail($"Municipio con Id {id} no encontrado");

            return ServiceResult<MunicipalityDto>.Ok(municipality);
        }
    }
}
