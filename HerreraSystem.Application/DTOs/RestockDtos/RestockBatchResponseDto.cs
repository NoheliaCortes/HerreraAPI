using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.DTOs.RestockDtos
{
    public class RestockBatchResponseDto
    {
        public int BatchId { get; set; }
        public string BatchCode { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitProductionCost { get; set; }
        public DateOnly ExpirationDate { get; set; }
    }
}
