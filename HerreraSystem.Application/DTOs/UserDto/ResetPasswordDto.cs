using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HerreraSystem.Application.DTOs.UserDto
{
    public class ResetPasswordDto
    {
        public int UserId   { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d).{6,}$",
        ErrorMessage = "Usa una contraseña más segura. Intenta combinar letras, números y símbolos.")]
        public string NewPassword { get; set; } 
    }
}
