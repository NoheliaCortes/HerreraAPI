using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.DTOs.ProductDtos
{
    public class ProductCatalogDto
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public bool? IsActive { get; set; }

        // Badges descriptivos — sin IDs expuestos al frontend
        public string LineName { get; set; } = null!;
        public string FlavorName { get; set; } = null!;
        public string PresentationName { get; set; } = null!;

        // Precios listos para mostrar
        public decimal? WholesalePrice { get; set; }   // Mayoreo
        public decimal? RetailPrice { get; set; }      // Detalle





    }
}
