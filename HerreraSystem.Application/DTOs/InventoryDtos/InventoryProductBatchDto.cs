using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.DTOs.InventoryDtos
{
    public class InventoryProductBatchDto
    {
        public int BatchId { get; set; }
        public string? BatchCode { get; set; }
        public string BatchStatusName { get; set; } = null!;
        public DateTime? EntryDate { get; set; }
        public DateOnly ExpirationDate { get; set; }
        public int StockDisplay { get; set; }
        public int StockWarehouse { get; set; }
        public int StockReserved { get; set; }
        public int TotalCurrentStock { get; set; }
        public int AvailableForSale { get; set; }
    }
}
