using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.RoleDtos;

namespace HerreraSystem.Application.Interfaces.Services;

public interface IRoleService
{
    Task<ServiceResult<IEnumerable<RoleDto>>> GetAllRolesAsync();
    Task<ServiceResult<RoleDto>> GetByIdAsync(int id);
    Task<ServiceResult<string>> CreateAsync(CreateRoleDto dto);
    Task<ServiceResult<string>> UpdateAsync(UpdateRoleDto dto);
    Task<ServiceResult<string>> ToggleStatusAsync(int id);
}