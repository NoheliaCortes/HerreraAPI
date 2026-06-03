using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.Auth;

namespace HerreraSystem.Application.Interfaces.Services;

public interface IAuthService
{
    Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginRequestDto dto);
}