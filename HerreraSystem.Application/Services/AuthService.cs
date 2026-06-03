using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.Auth;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Application.Interfaces.Services;
using HerreraSystem.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace HerreraSystem.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IConfiguration _config;

    public AuthService(IUserRepository userRepo, IConfiguration config)
    {
        _userRepo = userRepo;
        _config = config;
    }

    public async Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginRequestDto dto)
    {
        var user = await _userRepo.GetByUserNameAsync(dto.Username);

        if (user == null || user.IsActive == false)
            return ServiceResult<AuthResponseDto>.Fail("Credenciales invalidas");

        if (!VerifyPassword(dto.Password, user.PasswordHash))
            return ServiceResult<AuthResponseDto>.Fail("Credenciales invalidas");

        var roles = await _userRepo.GetUserRolesAsync(user.Id);
        var token = GenerateJwtToken(user, roles);
        var expiresAt = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiresInMinutes"]!));

        var response = new AuthResponseDto
        {
            Token = token,
            UserName = user.UserName,
            Roles = roles,
            ExpiresAt = expiresAt
        };

        return ServiceResult<AuthResponseDto>.Ok(response);
    }

    private string GenerateJwtToken(User user, List<string> roles)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new("username", user.UserName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (!string.IsNullOrEmpty(user.Email))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
        }

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var expires = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiresInMinutes"]!));

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static bool VerifyPassword(string password, byte[] storedHash)
    {
        try
        {
            var hashString = Encoding.UTF8.GetString(storedHash);
            return BCrypt.Net.BCrypt.Verify(password, hashString);
        }
        catch
        {
            return false;
        }
    }
}