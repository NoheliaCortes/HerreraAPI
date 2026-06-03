using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.DTOs.RoleDtos
{
    public class UpdateRoleDto
    {
        public int Id { get; set; }
        public string RoleName { get; set; }
        public string? RoleDescription { get; set; }

    }
}
