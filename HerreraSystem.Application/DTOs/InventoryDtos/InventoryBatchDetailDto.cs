using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.DTOs.InventoryDtos
{
    public class InventoryBatchDetailDto
    {
        public int BatchId { get; set; }
        public string? BatchCode { get; set; }
        public int ProductId { get; set; }
        public int RestockId { get; set; }
        public string BatchStatusName { get; set; } = null!;
        public DateTime? EntryDate { get; set; }
        public DateOnly ExpirationDate { get; set; }
        public int InitialQuantity { get; set; }
        public decimal UnitProductionCost { get; set; }
        public decimal EstimatedTotalCost { get; set; }
        public int StockDisplay { get; set; }
        public int StockWarehouse { get; set; }
        public int StockReserved { get; set; }
        public int TotalCurrentStock { get; set; }
        public int AvailableForSale { get; set; }
        public int SoldDetail { get; set; }
        public int SoldWholesale { get; set; }
        public int TotalSold { get; set; }
    }
}
