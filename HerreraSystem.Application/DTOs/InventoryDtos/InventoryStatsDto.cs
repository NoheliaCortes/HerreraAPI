using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.DTOs.InventoryDtos
{
    public class InventoryStatsDto
    {
        public int TotalProducts { get; set; }
        public int LowStockProducts { get; set; }
        public BestSellingFlavorDto? BestSellingFlavor { get; set; }
        public decimal InventoryValue { get; set; }
    }
}
