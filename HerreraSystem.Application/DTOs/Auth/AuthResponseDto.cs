using System;
using System.Collections.Generic;

namespace HerreraSystem.Application.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string Token { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; } 
        public string LastName { get; set; }  
        public string Email { get; set; }     
        public List<string> Roles { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}