using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.DTOs.LineDtos
{
    public class LineDto
    {
        public int Id { get; set; }
        public string LineName { get; set; } = null!;
        public bool? IsActive { get; set; }

    }
}
