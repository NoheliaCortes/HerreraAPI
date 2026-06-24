using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;


namespace HerreraSystem.Application.DTOs.Business
{
    public class BusinessProfileDto
    {
        public string Name { get; set; }
        public string Ruc { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public IFormFile? Logo { get; set; }
        public string? LogoUrl { get; set; }


    }
}
