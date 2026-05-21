using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.FlavorDtos;
using HerreraSystem.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class FlavorsController : ControllerBase
{
    private readonly IFlavorRepository _flavorRepository;

    // ASP.NET inyecta automáticamente el FlavorRepository
    // porque lo registramos en Program.cs con AddScoped
    public FlavorsController(IFlavorRepository flavorRepository)
    {
        _flavorRepository = flavorRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _flavorRepository.GetAllAsync();
        return Ok(ApiResponse<List<FlavorDto>>.Ok(data));
        // Retorna: { success: true, message: "Operación exitosa", data: [...] }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var flavor = await _flavorRepository.GetByIdAsync(id);
        if (flavor is null)
            return NotFound(ApiResponse<FlavorDto>.Fail($"Sabor con Id {id} no encontrado"));
        // Retorna 404: { success: false, message: "Sabor con Id 5 no encontrado", data: null }

        return Ok(ApiResponse<FlavorDto>.Ok(flavor));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateFlavorDto dto)
    {
        // Si el DTO tiene validaciones fallidas ASP.NET retorna 400
        // antes de llegar aquí gracias al ConfigureApiBehaviorOptions
        var created = await _flavorRepository.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById),
            new { id = created.Id },
            ApiResponse<FlavorDto>.Ok(created, "Sabor creado exitosamente"));
        // Retorna 201 con la URL del nuevo recurso en el header Location
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateFlavorDto dto)
    {
        var updated = await _flavorRepository.UpdateAsync(id, dto);
        if (!updated)
            return NotFound(ApiResponse<FlavorDto>.Fail($"Sabor con Id {id} no encontrado"));

        return Ok(ApiResponse<object>.Ok(null!, "Sabor actualizado exitosamente"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _flavorRepository.DeleteAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<FlavorDto>.Fail($"Sabor con Id {id} no encontrado"));

        return Ok(ApiResponse<object>.Ok(null!, "Sabor eliminado exitosamente"));
    }
}