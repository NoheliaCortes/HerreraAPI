using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.DTOs.SaleDtos
{
    public class RetailSaleResponseDto
    {
        public int SaleId { get; set; }
        public string SaleCode { get; set; } = null!;
        public decimal TotalSale { get; set; }
        public DateTime SaleDate { get; set; }
        public string PaymentStatus { get; set; } = null!;
        public int InventoryMovementId { get; set; }
        public List<SaleItemResponseDto> Items { get; set; } = new();
    }
}
