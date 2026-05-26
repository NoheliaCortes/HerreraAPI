using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.DTOs.LinePresentationDtos
{
    public class LinePresentationDto
    {
        public int Id { get; set; }

        public LineReferenceDto Line { get; set; } = null!;

        public PresentationReferenceDto Presentation { get; set; } = null!;

    }
}
