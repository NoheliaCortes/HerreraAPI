using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.DTOs.FlavorDtos
{
    public class FlavorDto
    {

        public int Id { get; set; }
        public string FlavorName { get; set; } = null!;
        public bool? IsActive { get; set; }
        public string? ImageUrl { get; set; }
        public string? FlavorColor { get; set; }

    }
}
