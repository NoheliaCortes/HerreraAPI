using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.DTOs.MunicipalityDtos
{
    public class MunicipalityDto
    {
        public int Id { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = null!;
        public string MunicipalityName { get; set; } = null!;
        public bool? IsActive { get; set; }

    }
}
