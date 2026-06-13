using System.ComponentModel.DataAnnotations;

namespace HerreraSystem.Application.DTOs.UserDto
{
    public class UpdateUserDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 50 caracteres.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La cédula es obligatoria.")]
        [RegularExpression(@"^[0-9]{13}[a-zA-Z]$", ErrorMessage = "La cédula debe tener 13 números y 1 letra (Ej: 041221006100F).")]
        public string IdNumber { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio.")]
        public string RoleName { get; set; }

        public bool IsActive { get; set; }
    }
}