using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HerreraSystem.Application.DTOs.RestockDtos
{
    public class CreateRestockDto
    {
        [Required(ErrorMessage = "El CreatedBy es obligatorio")]
        public int CreatedBy { get; set; }

        public string? Notes { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Debe incluir al menos un lote")]
        public List<CreateRestockBatchDto> Batches { get; set; } = new();
    }
}
