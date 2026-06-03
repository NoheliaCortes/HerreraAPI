using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.RoleDtos;
using HerreraSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HerreraSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
        => _roleService = roleService;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _roleService.GetAllRolesAsync();
        return Ok(ApiResponse<IEnumerable<RoleDto>>.Ok(result.Data!));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _roleService.GetByIdAsync(id);
        if (!result.Success)
            return NotFound(ApiResponse<string>.Fail(result.ErrorMessage!));

        return Ok(ApiResponse<RoleDto>.Ok(result.Data!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
    {
        var result = await _roleService.CreateAsync(dto);
        if (!result.Success)
            return BadRequest(ApiResponse<string>.Fail(result.ErrorMessage!));

        return Ok(ApiResponse<string>.Ok(result.Data!));
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateRoleDto dto)
    {
        var result = await _roleService.UpdateAsync(dto);
        if (!result.Success)
            return BadRequest(ApiResponse<string>.Fail(result.ErrorMessage!));

        return Ok(ApiResponse<string>.Ok(result.Data!));
    }

    [HttpPatch("{id}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var result = await _roleService.ToggleStatusAsync(id);
        if (!result.Success)
            return BadRequest(ApiResponse<string>.Fail(result.ErrorMessage!));

        return Ok(ApiResponse<string>.Ok(result.Data!));
    }
}