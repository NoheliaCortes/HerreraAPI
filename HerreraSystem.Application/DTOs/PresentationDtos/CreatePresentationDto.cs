using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HerreraSystem.Application.DTOs.PresentationDtos
{
    public class CreatePresentationDto
    {
        [Required(ErrorMessage = "El nombre de la presentación es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string PresentationName { get; set; } = null!;

    }
}
