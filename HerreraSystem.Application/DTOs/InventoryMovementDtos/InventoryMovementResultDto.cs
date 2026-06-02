using System;
using System.Collections.Generic;
using System.Text;
using HerreraSystem.Application.DTOs.InventoryMovementDtos;

namespace HerreraSystem.Application.DTOs.InventoryMovementDtos
{
    public class InventoryMovementResultDto
    {
        public int Id { get; set; }
        public int MovementTypeId { get; set; }
        public DateTime? MovementDate { get; set; }
        public string? Notes { get; set; }
        public int CreatedBy { get; set; }
        public List<MovementDetailResultDto> Details { get; set; } = new();
    }
}
