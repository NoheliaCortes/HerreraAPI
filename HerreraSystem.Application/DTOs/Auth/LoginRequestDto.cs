using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace HerreraSystem.Application.DTOs.Auth
{
    public class LoginRequestDto
    {
        [Required (ErrorMessage = "El nombre de usuario es requerido.")]
        public string? Username { get; set; }

        [Required(ErrorMessage ="La contraseña es requerida")]
        public string? Password { get; set; }
    }
}
