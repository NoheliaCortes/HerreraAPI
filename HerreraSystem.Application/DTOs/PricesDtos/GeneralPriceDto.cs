using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.DTOs.PricesDtos
{
    public class GeneralPriceDto
    {
        public int LinePresentationId { get; set; }
        public string LineName { get; set; } = null!;
        public string PresentationName { get; set; } = null!;
        public decimal? RetailPrice { get; set; }       // PriceType = "Detalle"
        public decimal? WholesalePrice { get; set; }    // PriceType = "Mayoreo"
        public int ProductsCount { get; set; }

    }
}
