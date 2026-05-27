using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.DTOs.RestockDtos
{
    public class RestockResponseDto
    {
        public int RestockId { get; set; }
        public string RestockCode { get; set; } = null!;
        public int InventoryMovementId { get; set; }
        public DateTime RestockDate { get; set; }
        public List<RestockBatchResponseDto> Batches { get; set; } = new();
    }
}
