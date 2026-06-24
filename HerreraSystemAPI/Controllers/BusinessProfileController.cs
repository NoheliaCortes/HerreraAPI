using HerreraSystem.Application.DTOs.Business;
using HerreraSystem.Domain.Entities;
using HerreraSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HerreraSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BusinessProfileController : ControllerBase
{
    private readonly HerreraSystemContext _context;

    public BusinessProfileController(HerreraSystemContext context)
    {
        _context = context;
    }

    // 1. OBTENER LOS DATOS DEL NEGOCIO
    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        // Como solo hay un negocio, agarramos el primer registro
        var profile = await _context.BusinessProfiles.FirstOrDefaultAsync();

        if (profile == null)
            return Ok(new BusinessProfileDto()); // Devuelve vacío si aún no hay datos

        return Ok(new BusinessProfileDto
        {
            Name = profile.Name,
            Ruc = profile.Ruc,
            Phone = profile.Phone,
            Address = profile.Address,
            Email = profile.Email,
            LogoUrl = profile.LogoUrl
        });
    }

    // 2. ACTUALIZAR O CREAR LOS DATOS Y SUBIR EL LOGO
    [HttpPost]
    public async Task<IActionResult> UpdateProfile([FromForm] BusinessProfileDto dto)
    {
        var profile = await _context.BusinessProfiles.FirstOrDefaultAsync();

        // Si no existe, lo creamos
        if (profile == null)
        {
            profile = new BusinessProfile();
            _context.BusinessProfiles.Add(profile);
        }

        profile.Name = dto.Name;
        profile.Ruc = dto.Ruc;
        profile.Phone = dto.Phone;
        profile.Address = dto.Address;
        profile.Email = dto.Email;

        // LÓGICA PARA GUARDAR LA IMAGEN (SI ENVIARON UNA)
        if (dto.Logo != null)
        {
            // Creamos la carpeta wwwroot/uploads si no existe
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Le ponemos un nombre único para que no choque con otros archivos
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + dto.Logo.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Guardamos el archivo físico en el servidor
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await dto.Logo.CopyToAsync(fileStream);
            }

            // Guardamos la ruta en la base de datos
            profile.LogoUrl = "/uploads/" + uniqueFileName;
        }

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Datos del negocio actualizados correctamente", logoUrl = profile.LogoUrl });
    }
}