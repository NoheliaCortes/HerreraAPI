using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.DTOs.DepartmentDtos
{
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string DepartmentName { get; set; } = null!;
        public bool? IsActive { get; set; }
    }
}
