using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HerreraSystem.Application.DTOs.RoleDtos
{
    public class CreateRoleDto
    {
        [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "El nombre del rol debe tener entre 3 y 30 caracteres.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚ\s]+$", ErrorMessage = "El nombre del rol solo puede contener letras y espacios.")]
        public string RoleName { get; set; } = null!;
        public string? RoleDescription { get; set; }
    }
}
