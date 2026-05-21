using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.DTOs.PresentationDtos
{
    public class PresentationDto
    {
        public int Id { get; set; }
        public string PresentationName { get; set; } = null!;
        public bool? IsActive { get; set; }


    }
}
