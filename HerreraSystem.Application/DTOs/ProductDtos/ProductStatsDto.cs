using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.DTOs.ProductDtos
{
    public class ProductStatsDto
    {
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int InactiveProducts { get; set; }
    }
}
