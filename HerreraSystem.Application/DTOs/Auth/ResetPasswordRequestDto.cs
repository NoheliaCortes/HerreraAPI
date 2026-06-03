using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.DTOs.Auth
{
    public class ResetPasswordRequestDto
    {
        public string Token { get; set; }
        public string NewPassword {  get; set; }
    }
}
