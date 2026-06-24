using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HerreraSystem.Application.DTOs.ProductDtos
{
    public class CreateProductDto
    {
        [Required(ErrorMessage = "El LinePresentationId es obligatorio")]
        public int LinePresentationId { get; set; }

        [Required(ErrorMessage = "El FlavorId es obligatorio")]
        public int FlavorId { get; set; }

        [Required(ErrorMessage = "El nombre del producto es obligatorio")]
        [StringLength(150, ErrorMessage = "El nombre no puede exceder 150 caracteres")]
        public string ProductName { get; set; } = null!;

        [Url(ErrorMessage = "La URL de la imagen no es válida")]
        public string? ImageUrl { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo")]
        public int MinimumStock { get; set; }


    }
}
