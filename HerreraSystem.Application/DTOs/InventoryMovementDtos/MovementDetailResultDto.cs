using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.DTOs.InventoryMovementDtos
{
    public class MovementDetailResultDto
    {
        public int Id { get; set; }
        public int BatchId { get; set; }
        public int? SourceLocationId { get; set; }
        public int? DestinationLocationId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
    }
}
