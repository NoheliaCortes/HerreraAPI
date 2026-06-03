using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HerreraSystem.Application.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string Token { get; set; }
        public string UserName { get; set; }
        public List<string> Roles { get; set; }
        public DateTime ExpiresAt { get; set; }

    }
}
