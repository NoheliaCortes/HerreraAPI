using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HerreraSystem.Application.DTOs.CustomerDtos
{
    public class CreateCustomerDto
    {
        [Required(ErrorMessage = "El municipio es obligatorio")]
        public int MunicipalityId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
        public string LastName { get; set; } = null!;

        [Phone(ErrorMessage = "El número de teléfono no es válido")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        public string? Phone { get; set; }

        [StringLength(150, ErrorMessage = "El punto de venta no puede exceder 150 caracteres")]
        public string? PointOfSale { get; set; }

        [StringLength(250, ErrorMessage = "La dirección no puede exceder 250 caracteres")]
        public string? Posaddress { get; set; }
    }
}
