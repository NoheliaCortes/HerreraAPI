using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.FlavorDtos;
using HerreraSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class FlavorsController : ControllerBase
{
    private readonly IFlavorService _flavorService;

    public FlavorsController(IFlavorService flavorService)
    {
        _flavorService = flavorService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams paginationParams)
    {
        var data = await _flavorService.GetAllAsync(paginationParams);

        return Ok(ApiResponse<PagedResponse<FlavorDto>>.Ok(data));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _flavorService.GetByIdAsync(id);

        if (!result.Success)
            return NotFound(ApiResponse<FlavorDto>.Fail(result.ErrorMessage!));

        return Ok(ApiResponse<FlavorDto>.Ok(result.Data!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateFlavorDto dto)
    {
        var result = await _flavorService.CreateAsync(dto);

        if (!result.Success)
            return BadRequest(ApiResponse<FlavorDto>.Fail(result.ErrorMessage!));

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Data!.Id },
            ApiResponse<FlavorDto>.Ok(result.Data, "Sabor creado exitosamente"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateFlavorDto dto)
    {
        var result = await _flavorService.UpdateAsync(id, dto);

        if (!result.Success)
        {
            if (result.ErrorMessage!.Contains("no encontrado"))
            {
                return NotFound(ApiResponse<object>.Fail(result.ErrorMessage));
            }

            return BadRequest(ApiResponse<object>.Fail(result.ErrorMessage));
        }

        return Ok(ApiResponse<object>.Ok(null!, "Sabor actualizado exitosamente"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _flavorService.DeleteAsync(id);

        if (!result.Success)
        {
            if (result.ErrorMessage!.Contains("no encontrado"))
            {
                return NotFound(ApiResponse<object>.Fail(result.ErrorMessage));
            }

            return BadRequest(ApiResponse<object>.Fail(result.ErrorMessage));
        }

        return Ok(ApiResponse<object>.Ok(null!, "Sabor eliminado exitosamente"));
    }
}