using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.DTOs.CustomerDtos
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public int MunicipalityId { get; set; }
        public string MunicipalityName { get; set; } = null!;
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? Phone { get; set; }
        public string? PointOfSale { get; set; }
        public string? Posaddress { get; set; }
        public bool? IsActive { get; set; }
    }
}
