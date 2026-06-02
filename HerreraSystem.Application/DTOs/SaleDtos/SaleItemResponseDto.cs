using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.DTOs.SaleDtos
{
    public class SaleItemResponseDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal AppliedPrice { get; set; }
        public decimal LineSubtotal { get; set; }
    }
}
