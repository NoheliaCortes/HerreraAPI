using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.DTOs.Auth
{
    public class RegisterRequestDto
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string RoleName { get; set; }

    }
}
