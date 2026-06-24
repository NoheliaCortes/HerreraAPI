namespace HerreraSystem.Application.DTOs.SaleDtos
{
    public class SaleListItemDto
    {
        public int Id { get; set; }

        public string SaleCode { get; set; } = null!;

        public DateTime? SaleDate { get; set; }

        public string CustomerName { get; set; } = null!;

        public int SaleTypeId { get; set; }

        public string SaleTypeName { get; set; } = null!;

        public int PaymentTypeId { get; set; }

        public string PaymentTypeName { get; set; } = null!;

        public decimal Total { get; set; }
    }
}
