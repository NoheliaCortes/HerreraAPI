using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HerreraSystem.Application.DTOs.InventoryMovementDtos
{
    public class TransferDetailDto
    {
        [Required]
        public int BatchId { get; set; }

        [Required]
        public int SourceLocationId { get; set; }

        [Required]
        public int DestinationLocationId { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
