using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HerreraSystem.Application.DTOs.InventoryMovementDtos
{
    public class AdjustmentDetailDto
    {
        [Required]
        public int BatchId { get; set; }

        [Required]
        public int LocationId { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
