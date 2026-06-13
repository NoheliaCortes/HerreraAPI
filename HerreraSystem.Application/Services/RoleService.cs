using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.RoleDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Application.Interfaces.Services;
using HerreraSystem.Domain.Entities;

namespace HerreraSystem.Application.Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepo;

    public RoleService(IRoleRepository roleRepo)
    {
        _roleRepo = roleRepo;
    }

    public async Task<ServiceResult<IEnumerable<RoleDto>>> GetAllRolesAsync()
    {
        var roles = await _roleRepo.GetAllAsync();
        var rolesDto = roles.Select(r => new RoleDto
        {
            Id = r.Id,
            RoleName = r.RoleName,
            RoleDescription = r.RoleDescription,
            IsActive = r.IsActive
        });

        return ServiceResult<IEnumerable<RoleDto>>.Ok(rolesDto);
    }

    public async Task<ServiceResult<RoleDto>> GetByIdAsync(int id)
    {
        var role = await _roleRepo.GetByIdAsync(id);
        if (role == null)
            return ServiceResult<RoleDto>.Fail("Rol no encontrado.");

        var roleDto = new RoleDto
        {
            Id = role.Id,
            RoleName = role.RoleName,
            RoleDescription = role.RoleDescription,
            IsActive = role.IsActive
        };

        return ServiceResult<RoleDto>.Ok(roleDto);
    }

    public async Task<ServiceResult<string>> CreateAsync(CreateRoleDto dto)
    {
        var existingRole = await _roleRepo.GetByNameAsync(dto.RoleName);
        if (existingRole != null)
            return ServiceResult<string>.Fail("Ya existe un rol con ese nombre.");

        var role = new Role
        {
            RoleName = dto.RoleName,
            RoleDescription = dto.RoleDescription
        };

        await _roleRepo.AddAsync(role);
        return ServiceResult<string>.Ok("Rol creado exitosamente.");
    }

    public async Task<ServiceResult<string>> UpdateAsync(UpdateRoleDto dto)
    {
        var role = await _roleRepo.GetByIdAsync(dto.Id);
        if (role == null)
            return ServiceResult<string>.Fail("Rol no encontrado.");

        var existingRoleName = await _roleRepo.GetByNameAsync(dto.RoleName);
        if (existingRoleName != null && existingRoleName.Id != dto.Id)
            return ServiceResult<string>.Fail("Ya existe otro rol con ese nombre.");

        role.RoleName = dto.RoleName;
        role.RoleDescription = dto.RoleDescription;

        await _roleRepo.UpdateAsync(role);
        return ServiceResult<string>.Ok("Rol actualizado exitosamente.");
    }

    public async Task<ServiceResult<string>> ToggleStatusAsync(int id)
    {
        var role = await _roleRepo.GetByIdAsync(id);
        if (role == null)
            return ServiceResult<string>.Fail("Rol no encontrado.");

        if (role.IsActive == true)
        {
            if (role.RoleName == "Administrador" || role.RoleName == "Admin")
                return ServiceResult<string>.Fail("No se puede desactivar el rol del sistema.");

            var hasActiveUsers = await _roleRepo.HasActiveUsersAsync(role.Id);
            if (hasActiveUsers)
                return ServiceResult<string>.Fail("No se puede desactivar este rol porque tiene usuarios activos asignados.");
        }

        role.IsActive = !role.IsActive;

        await _roleRepo.UpdateAsync(role);
        var status = role.IsActive == true ? "activado" : "desactivado";
        return ServiceResult<string>.Ok($"Rol {status} exitosamente.");
    }
}