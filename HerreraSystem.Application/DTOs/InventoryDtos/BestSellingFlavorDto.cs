using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.DTOs.InventoryDtos
{
    public class BestSellingFlavorDto
    {
        public int FlavorId { get; set; }
        public string FlavorName { get; set; } = null!;
        public int QuantitySold { get; set; }
        public string Period { get; set; } = null!;
    }
}
