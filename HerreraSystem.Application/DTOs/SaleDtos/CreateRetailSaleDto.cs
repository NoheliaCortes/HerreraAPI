using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HerreraSystem.Application.DTOs.SaleDtos
{
    public class CreateRetailSaleDto
    {
        [Required(ErrorMessage = "El CreatedBy es obligatorio")]
        public int CreatedBy { get; set; }

        [Required(ErrorMessage = "El método de pago es obligatorio")]
        public int PaymentMethodId { get; set; }


        public string? TransactionReference { get; set; }

        public string? Notes { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Debe incluir al menos un producto")]
        public List<CreateSaleItemDto> Items { get; set; } = new();
    }
}
