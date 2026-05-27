using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HerreraSystem.Application.DTOs.RestockDtos
{
    public class CreateRestockBatchDto
    {
        [Required(ErrorMessage = "El ProductId es obligatorio")]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El costo unitario debe ser mayor a 0")]
        public decimal UnitProductionCost { get; set; }

        [Required(ErrorMessage = "La fecha de vencimiento es obligatoria")]
        public DateOnly ExpirationDate { get; set; }
    }
}
