using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.CustomerDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Interfaces.Services
{
    public interface ICustomerService
    {
        Task<PagedResponse<CustomerDto>> GetAllAsync(
         PaginationParams paginationParams,
         string? search,
         int? departmentId,
         int? municipalityId);

        Task<ServiceResult<CustomerStatsDto>> GetStatsAsync();
        Task<ServiceResult<CustomerDto>> GetByIdAsync(int id);
        Task<ServiceResult<CustomerDto>> CreateAsync(CreateCustomerDto dto);
        Task<ServiceResult<bool>> UpdateAsync(int id, UpdateCustomerDto dto);
        Task<ServiceResult<bool>> DeleteAsync(int id);
    }
}
