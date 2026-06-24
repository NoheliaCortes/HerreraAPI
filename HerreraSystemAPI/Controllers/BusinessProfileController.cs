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

 
    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        
        var profile = await _context.BusinessProfiles.FirstOrDefaultAsync();

        if (profile == null)
            return Ok(new BusinessProfileDto()); 

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

    [HttpPost]
    public async Task<IActionResult> UpdateProfile([FromForm] BusinessProfileDto dto)
    {
        var profile = await _context.BusinessProfiles.FirstOrDefaultAsync();

        
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

        
        if (dto.Logo != null)
        {
          
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

         
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + dto.Logo.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await dto.Logo.CopyToAsync(fileStream);
            }

            profile.LogoUrl = "/uploads/" + uniqueFileName;
        }

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Datos del negocio actualizados correctamente", logoUrl = profile.LogoUrl });
    }
}