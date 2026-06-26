using System.Text;
using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.Auth;
using HerreraSystem.Application.DTOs.UserDto;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Application.Interfaces.Services;
using HerreraSystem.Domain.Entities;

namespace HerreraSystem.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepo;

    public UserService(IUserRepository userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<ServiceResult<IEnumerable<UserDto>>> GetAllUsersAsync()
    {
        var users = await _userRepo.GetAllWithRolesAsync();
        var usersDto = users.Select(u => new UserDto
        {
            Id = u.Id,
            UserName = u.UserName,
            Email = u.Email,
            IdNumber = u.IdNumber,
            FirstName = u.FirstName,
            LastName = u.LastName,
            IsActive = u.IsActive ?? false,
            Roles = u.UserRoles.Select(ur => ur.Role.RoleName).ToList()
        });

        return ServiceResult<IEnumerable<UserDto>>.Ok(usersDto);
    }

    public async Task<ServiceResult<UserDto>> GetByIdAsync(int id)
    {
        var user = await _userRepo.GetByIdWithRoleAsync(id);
        if (user == null)
            return ServiceResult<UserDto>.Fail("Usuario no encontrado.");

        var userDto = new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            IdNumber = user.IdNumber,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive ?? false,
            Roles = user.UserRoles.Select(ur => ur.Role?.RoleName ?? "Sin Rol").ToList()
        };

        return ServiceResult<UserDto>.Ok(userDto);
    }

    public async Task<ServiceResult<string>> CreateAsync(CreateUserDto dto)
    {
        var existingUser = await _userRepo.GetByUserNameAsync(dto.UserName);
        if (existingUser != null)
            return ServiceResult<string>.Fail("Ya existe un usuario con este nombre de usuario.");

        var existingEmail = await _userRepo.GetByEmailAsync(dto.Email);
        if (existingEmail != null)
            return ServiceResult<string>.Fail("Ya existe un usuario con este correo electrónico.");

        var existingId = await _userRepo.GetByIdNumberAsync(dto.IdNumber);
        if (existingId != null)
            return ServiceResult<string>.Fail("Ya existe un usuario con esta cédula.");

        // Validar que se haya enviado un rol
        if (string.IsNullOrWhiteSpace(dto.RoleName))
            return ServiceResult<string>.Fail("Debe seleccionar un rol.");

        // Validar que el rol exista
        var role = await _userRepo.GetRoleByNameAsync(dto.RoleName);
        if (role == null)
            return ServiceResult<string>.Fail("El rol seleccionado no existe.");

        var user = new User
        {
            UserName = dto.UserName,
            Email = dto.Email,
            IdNumber = dto.IdNumber,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PasswordHash = HashPassword(dto.Password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        user.UserRoles.Add(new UserRole
        {
            RoleId = role.Id,
            AssignedAt = DateTime.UtcNow
        });

        await _userRepo.AddAsync(user);

        return ServiceResult<string>.Ok("Usuario creado exitosamente.");
    }

    public async Task<ServiceResult<string>> UpdateAsync(UpdateUserDto dto)
    {
        var user = await _userRepo.GetByIdWithRoleAsync(dto.Id);
        if (user == null)
            return ServiceResult<string>.Fail("Usuario no encontrado.");

        var existingUserName = await _userRepo.GetByUserNameAsync(dto.UserName);
        if (existingUserName != null && existingUserName.Id != dto.Id)
            return ServiceResult<string>.Fail("Ya existe otro usuario con ese nombre.");

        user.UserName = dto.UserName;
        user.Email = dto.Email;
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.IdNumber = dto.IdNumber;
        user.IsActive = dto.IsActive;

        var role = await _userRepo.GetRoleByNameAsync(dto.RoleName);
        if (role != null)
        {
            user.UserRoles.Clear();
            user.UserRoles.Add(new UserRole
            {
                RoleId = role.Id,
                UserId = user.Id,
                AssignedAt = DateTime.UtcNow
            });
        }

        await _userRepo.UpdateAsync(user);
        return ServiceResult<string>.Ok("Usuario actualizado exitosamente.");
    }

    public async Task<ServiceResult<string>> ToggleStatusAsync(int targetUserId, int currentLoggedInUserId)
    {
        if (targetUserId == currentLoggedInUserId)
            return ServiceResult<string>.Fail("No puedes desactivar tu propia cuenta.");

        var user = await _userRepo.GetByIdWithRoleAsync(targetUserId);
        if (user == null)
            return ServiceResult<string>.Fail("Usuario no encontrado.");

        if (user.IsActive == true)
        {
            var roles = await _userRepo.GetUserRolesAsync(user.Id);
            if (roles.Contains("Administrador") || roles.Contains("Admin"))
            {
                var activeAdminsCount = await _userRepo.CountActiveAdminsAsync();
                if (activeAdminsCount <= 1)
                    return ServiceResult<string>.Fail("Debe quedar al menos un Administrador activo en el sistema.");
            }
        }
        if (user.IsActive == false || user.IsActive == null)
        {
            var hasInactiveRole = await _userRepo.HasInactiveRoleAsync(user.Id);
            if (hasInactiveRole)
                return ServiceResult<string>.Fail("No se puede activar el usuario porque su rol asignado está desactivado.");
        }

        user.IsActive = !(user.IsActive ?? false);
        await _userRepo.UpdateAsync(user);

        return ServiceResult<string>.Ok("Estado actualizado correctamente.");
    }

    public async Task<ServiceResult<string>> GeneratePasswordResetTokenAsync(ForgotPasswordRequestDto dto)
    {
        var user = await _userRepo.GetByUserNameAsync(dto.UserName);

        if (user == null)
            return ServiceResult<string>.Fail("El usuario no existe en el sistema");

        user.ResetToken = Guid.NewGuid().ToString();
        user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);

        await _userRepo.UpdateAsync(user);

        return ServiceResult<string>.Ok(user.ResetToken);
    }

    public async Task<ServiceResult<string>> ResetPasswordAsync(ResetPasswordRequestDto dto)
    {
        var user = await _userRepo.GetByResetTokenAsync(dto.Token);

        if (user == null || user.ResetTokenExpiry < DateTime.UtcNow)
            return ServiceResult<string>.Fail("Token invalido o expirado");

        user.PasswordHash = HashPassword(dto.NewPassword);
        user.ResetToken = null;
        user.ResetTokenExpiry = null;

        await _userRepo.UpdateAsync(user);
        return ServiceResult<string>.Ok("Contrasena actualizada exitosamente");
    }

    public async Task<ServiceResult<string>> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _userRepo.GetByIdWithRoleAsync(dto.UserId);
        if (user == null)
            return ServiceResult<string>.Fail("Usuario no encontrado.");

        user.PasswordHash = HashPassword(dto.NewPassword);

        await _userRepo.UpdateAsync(user);
        return ServiceResult<string>.Ok("Contraseña actualizada correctamente.");
    }

    private static byte[] HashPassword(string password)
    {
        var hashString = BCrypt.Net.BCrypt.HashPassword(password);
        return Encoding.UTF8.GetBytes(hashString);
    }
}