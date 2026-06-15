using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.CustomerDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Interfaces.Repositories
{
    public interface ICustomerRepository
    {
        // Modificado para aceptar filtros opcionales
        Task<PagedResponse<CustomerDto>> GetAllAsync(
            PaginationParams paginationParams,
            string? search,
            int? departmentId,
            int? municipalityId);

        // Nuevo método para estadísticas
        Task<CustomerStatsDto> GetStatsAsync();
        Task<CustomerDto?> GetByIdAsync(int id);
        Task<CustomerDto> CreateAsync(CreateCustomerDto dto);
        Task<bool> UpdateAsync(int id, UpdateCustomerDto dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(string firstName, string lastName, int municipalityId, int? excludeId = null);
        Task<bool> HasOrdersOrSalesAsync(int customerId);
    }
}
