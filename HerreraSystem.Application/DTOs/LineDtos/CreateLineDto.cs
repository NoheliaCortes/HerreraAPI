using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HerreraSystem.Application.DTOs.LineDtos
{
    public class CreateLineDto
    {
        [Required(ErrorMessage = "El nombre de la línea es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string LineName { get; set; } = null!;
        public bool? IsActive { get; set; }

    }
}
