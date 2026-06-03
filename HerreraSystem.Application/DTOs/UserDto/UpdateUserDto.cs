using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.DTOs.UserDto
{
    public class UpdateUserDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string RoleName { get; set; }
    }
}
