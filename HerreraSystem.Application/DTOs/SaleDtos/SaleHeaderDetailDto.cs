namespace HerreraSystem.Application.DTOs.SaleDtos
{
    public class SaleHeaderDetailDto
    {
        public int Id { get; set; }

        public string SaleCode { get; set; } = null!;

        public DateTime? SaleDate { get; set; }

        public string? OrderCode { get; set; }

        public SaleCustomerInfoDto Customer { get; set; } = null!;

        public decimal Total { get; set; }

        public string PaymentStatusName { get; set; } = null!;

        public decimal? PendingBalance { get; set; }

        public string? CreatedByUserName { get; set; }

        public int PaymentTypeId { get; set; }

        public string PaymentTypeName { get; set; } = null!;

        public int SaleTypeId { get; set; }

        public string SaleTypeName { get; set; } = null!;
    }
}
