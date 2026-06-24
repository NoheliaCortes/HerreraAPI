using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HerreraSystem.Application.DTOs.FlavorDtos
{
    public class CreateFlavorDto
    {
        [Required(ErrorMessage = "El nombre del sabor es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string FlavorName { get; set; } = null!;

        [StringLength(7, ErrorMessage = "El color debe ser un código hex válido")]
        public string? FlavorColor { get; set; }

        
        public IFormFile? ImageURL { get; set; }

        public bool IsActive { get; set; } = true;


    }
}
