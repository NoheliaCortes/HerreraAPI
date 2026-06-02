using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HerreraSystem.Application.DTOs.InventoryMovementDtos
{
    public class CreateNegativeAdjustmentDto
    {
        public string? Notes { get; set; }

        [Required]
        public int CreatedBy { get; set; }

        [Required, MinLength(1)]
        public List<AdjustmentDetailDto> Details { get; set; } = new();
    }
}
