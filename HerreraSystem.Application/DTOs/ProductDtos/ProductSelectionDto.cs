using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.DTOs.ProductDtos
{
    public class ProductSelectionDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int LinePresentationId { get; set; }
        public string LineName { get; set; } = null!;
        public string PresentationName { get; set; } = null!;
        public int FlavorId { get; set; }
        public string FlavorName { get; set; } = null!;
    }
}
