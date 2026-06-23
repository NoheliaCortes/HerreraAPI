using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Domain.Entities
{
    public class BusinessProfile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Ruc { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string? LogoUrl { get; set; }
    }
}
