using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.DTOs.InventoryDtos
{
    public class InventoryProductBatchesDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int ActiveBatchCount { get; set; }
        public List<InventoryProductBatchDto> Batches { get; set; } = new();
    }
}
