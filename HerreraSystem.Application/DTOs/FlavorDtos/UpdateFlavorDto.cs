using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HerreraSystem.Application.DTOs.FlavorDtos
{
    public class UpdateFlavorDto
    {
        [Required(ErrorMessage = "El nombre del sabor es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string FlavorName { get; set; } = null!;

        public bool? IsActive { get; set; }

        [Url(ErrorMessage = "La URL de la imagen no es válida")]
        public string? ImageUrl { get; set; }

        [RegularExpression("^#([A-Fa-f0-9]{6})$",
            ErrorMessage = "El color debe ser un código HEX válido")]
        public string? FlavorColor { get; set; }

    }
}
