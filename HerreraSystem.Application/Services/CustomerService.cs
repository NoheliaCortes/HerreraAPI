using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.CustomerDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using HerreraSystem.Application.Interfaces.Services;

namespace HerreraSystem.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<PagedResponse<CustomerDto>> GetAllAsync(
            PaginationParams paginationParams)
        {
            return await _customerRepository
                .GetAllAsync(paginationParams);
        }

        public async Task<ServiceResult<CustomerDto>> GetByIdAsync(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);

            if (customer is null)
                return ServiceResult<CustomerDto>
                    .Fail($"Cliente con Id {id} no encontrado");

            return ServiceResult<CustomerDto>.Ok(customer);
        }

        public async Task<ServiceResult<CustomerDto>> CreateAsync(
            CreateCustomerDto dto)
        {
            var exists = await _customerRepository
                .ExistsAsync(dto.FirstName, dto.LastName, dto.MunicipalityId);

            if (exists)
                return ServiceResult<CustomerDto>
                    .Fail("Ya existe un cliente con ese nombre en el mismo municipio");

            var created = await _customerRepository
                .CreateAsync(dto);

            return ServiceResult<CustomerDto>
                .Ok(created);
        }

        public async Task<ServiceResult<bool>> UpdateAsync(
            int id,
            UpdateCustomerDto dto)
        {
            var customer = await _customerRepository
                .GetByIdAsync(id);

            if (customer is null)
                return ServiceResult<bool>
                    .Fail($"Cliente con Id {id} no encontrado");

            var exists = await _customerRepository
                .ExistsAsync(dto.FirstName, dto.LastName, dto.MunicipalityId, id);

            if (exists)
                return ServiceResult<bool>
                    .Fail("Ya existe un cliente con ese nombre en el mismo municipio");

            var updated = await _customerRepository
                .UpdateAsync(id, dto);

            return ServiceResult<bool>
                .Ok(updated);
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            var customer = await _customerRepository
                .GetByIdAsync(id);

            if (customer is null)
                return ServiceResult<bool>
                    .Fail($"Cliente con Id {id} no encontrado");

            var hasActivity = await _customerRepository
                .HasOrdersOrSalesAsync(id);

            if (hasActivity)
                return ServiceResult<bool>
                    .Fail("No se puede eliminar el cliente porque tiene órdenes o ventas asociadas");

            var deleted = await _customerRepository
                .DeleteAsync(id);

            return ServiceResult<bool>
                .Ok(deleted);
        }
    }
}
