using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.Auth;
using HerreraSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HerreraSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
        => _authService = authService;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var result = await _authService.LoginAsync(dto);

        if (!result.Success)
            return Unauthorized(ApiResponse<string>.Fail(result.ErrorMessage!));

        return Ok(ApiResponse<AuthResponseDto>.Ok(result.Data!));
    }
}