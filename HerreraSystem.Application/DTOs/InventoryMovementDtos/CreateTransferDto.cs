using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace HerreraSystem.Application.DTOs.InventoryMovementDtos
{
    public class CreateTransferDto
    {
        public string? Notes { get; set; }

        [Required]
        public int CreatedBy { get; set; }

        [Required, MinLength(1)]
        public List<TransferDetailDto> Details { get; set; } = new();
    }
}
