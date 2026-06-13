using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.Auth;
using HerreraSystem.Application.DTOs.UserDto;
using HerreraSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace HerreraSystem.API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _userService.GetAllUsersAsync();
        return Ok(ApiResponse<IEnumerable<UserDto>>.Ok(result.Data!));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _userService.GetByIdAsync(id);
        if (!result.Success)
            return NotFound(ApiResponse<string>.Fail(result.ErrorMessage!));

        return Ok(ApiResponse<UserDto>.Ok(result.Data!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var result = await _userService.CreateAsync(dto);
        if (!result.Success)
            return BadRequest(ApiResponse<string>.Fail(result.ErrorMessage!));

        return Ok(ApiResponse<string>.Ok(result.Data!));
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateUserDto dto)
    {
        var result = await _userService.UpdateAsync(dto);
        if (!result.Success)
            return BadRequest(ApiResponse<string>.Fail(result.ErrorMessage!));

        return Ok(ApiResponse<string>.Ok(result.Data!));
    }

    [HttpPatch("{id}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var currentUserIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
                              ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

        if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim.Value, out int currentUserId))
            return Unauthorized(new { success = false, message = "Sesión inválida." });

        var result = await _userService.ToggleStatusAsync(id, currentUserId);

        if (!result.Success) 
            return BadRequest(new { success = false, message = result.ErrorMessage });

        return Ok(new { success = true, data = result.Data });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
    {
        var result = await _userService.GeneratePasswordResetTokenAsync(dto);
        if (!result.Success)
            return BadRequest(ApiResponse<string>.Fail(result.ErrorMessage!));

        return Ok(ApiResponse<string>.Ok(result.Data!));
    }

    [HttpPatch("{id}/reset-password")]
    [Authorize(Roles = "Administrador, Admin")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordDto dto)
    {
        if (id != dto.UserId)
            return BadRequest(new { success = false, message = "El ID no coincide." });

        var result = await _userService.ResetPasswordAsync(dto);

        if (!result.Success)
            return BadRequest(new { success = false, message = result.ErrorMessage });

        return Ok(new { success = true, message = result.Data });
    }
}