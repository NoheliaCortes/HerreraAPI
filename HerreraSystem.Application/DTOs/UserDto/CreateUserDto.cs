using System.ComponentModel.DataAnnotations;

namespace HerreraSystem.Application.DTOs.UserDto
{
    public class CreateUserDto
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 50 caracteres.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La cédula es obligatoria.")]
        [RegularExpression(@"^[0-9]{13}[a-zA-Z]$", ErrorMessage = "La cédula nicaragüense debe tener 13 números seguidos de una letra (Ej: 041221006100F).")]
        public string IdNumber { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d).{6,}$",
             ErrorMessage = "Usa una contraseña más segura. Intenta combinar letras, números y símbolos.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "El rol es obligatorio.")]
        public string RoleName { get; set; }
    }
}