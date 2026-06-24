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

    public class CreateGeneralPriceDto
    {
        public int LinePresentationId { get; set; }

        public int PriceTypeId { get; set; }

        public decimal Price { get; set; }

        public DateTime ValidFrom { get; set; }

        public DateTime? ValidTo { get; set; }

        public int CreatedBy { get; set; }
    }

    public class ChangeGeneralPriceDto
    {
        public int PriceTypeId { get; set; }

        public decimal Price { get; set; }

        public DateTime ValidFrom { get; set; }

        public DateTime? ValidTo { get; set; }

        public int CreatedBy { get; set; }
    }

    public class GeneralPriceDetailDto
    {
        public int Id { get; set; }

        public int LinePresentationId { get; set; }

        public string LineName { get; set; } = null!;

        public string PresentationName { get; set; } = null!;

        public int PriceTypeId { get; set; }

        public string PriceTypeName { get; set; } = null!;

        public decimal Price { get; set; }

        public DateTime ValidFrom { get; set; }

        public DateTime? ValidTo { get; set; }

        public bool IsActive { get; set; }

        public int CreatedBy { get; set; }

        public DateTime? CreatedAt { get; set; }
    }

    public class PriceStatisticsDto
    {
        public int ProductsWithPrice { get; set; }

        public int ActiveSpecialPrices { get; set; }

        public int PromotionsExpiringSoon { get; set; }

        public DateTime? LastUpdate { get; set; }
    }
}
