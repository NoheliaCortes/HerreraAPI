using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.DTOs.ProductDtos
{
    public class ProductDto
    {
        public int Id { get; set; }
        public int LinePresentationId { get; set; }
        public int FlavorId { get; set; }
        public string ProductName { get; set; } = null!;
        public bool? IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? ImageUrl { get; set; }
        public int MinimumStock { get; set; }
    }
}
