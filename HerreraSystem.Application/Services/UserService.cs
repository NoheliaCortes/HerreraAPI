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
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive ?? false,
            Roles = user.UserRoles.Select(ur => ur.Role.RoleName).ToList()
        };

        return ServiceResult<UserDto>.Ok(userDto);
    }

    public async Task<ServiceResult<string>> CreateAsync(CreateUserDto dto)
    {
        var existingUser = await _userRepo.GetByUserNameAsync(dto.UserName);
        if (existingUser != null)
            return ServiceResult<string>.Fail("El usuario ya existe.");

        var user = new User
        {
            UserName = dto.UserName,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PasswordHash = HashPassword(dto.Password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var role = await _userRepo.GetRoleByNameAsync(dto.RoleName);
        if (role != null)
        {
            user.UserRoles.Add(new UserRole
            {
                RoleId = role.Id,
                AssignedAt = DateTime.UtcNow
            });
        }

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

    public async Task<ServiceResult<string>> ToggleStatusAsync(int id)
    {
        var user = await _userRepo.GetByIdWithRoleAsync(id);
        if (user == null)
            return ServiceResult<string>.Fail("Usuario no encontrado.");

        user.IsActive = !(user.IsActive ?? false);
        await _userRepo.UpdateAsync(user);

        var status = user.IsActive == true ? "activado" : "desactivado";
        return ServiceResult<string>.Ok($"Usuario {status} exitosamente.");
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

    private static byte[] HashPassword(string password)
    {
        var hashString = BCrypt.Net.BCrypt.HashPassword(password);
        return Encoding.UTF8.GetBytes(hashString);
    }
}