using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.DTOs.InventoryDtos
{
    public class InventoryProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string LineName { get; set; } = null!;
        public string PresentationName { get; set; } = null!;
        public string FlavorName { get; set; } = null!;

        public int DisplayStock { get; set; }     // Mostrador  — LocationId = 1
        public int WarehouseStock { get; set; }   // Bodega     — LocationId = 2
        public int ReservedStock { get; set; }    // Reservado  — LocationId = 3
        public int TotalStock { get; set; }       // Suma total

        public decimal? RetailPrice { get; set; }     // PriceTypeId = 1 "Detalle"
        public decimal? WholesalePrice { get; set; }  // PriceTypeId = 2 "Mayoreo"


    }
}
