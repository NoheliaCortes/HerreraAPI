using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.DTOs.UserDto
{
    public class UserDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }  
        public string Email { get; set; }
        public string FirstName { get; set; }  
        public string LastName { get; set; }  
        public bool IsActive { get; set; }
        public List<string> Roles { get; set; } = new();



    }
}
