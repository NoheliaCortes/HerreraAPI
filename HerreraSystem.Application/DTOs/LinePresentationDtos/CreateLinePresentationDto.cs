using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HerreraSystem.Application.DTOs.LinePresentationDtos
{
    public class CreateLinePresentationDto
    {
        [Required(ErrorMessage = "La línea es obligatoria")]
        public int LineId { get; set; }

        [Required(ErrorMessage = "La presentación es obligatoria")]
        public int PresentationId { get; set; }
    }
}
