namespace HerreraSystem.Application.DTOs.SaleDtos
{
    public class SalePaymentDto
    {
        public int Id { get; set; }

        public decimal Amount { get; set; }

        public string PaymentMethodName { get; set; } = null!;

        public DateTime? PaymentDate { get; set; }

        public string? TransactionReference { get; set; }

        public string? RegisteredByUserName { get; set; }
    }
}
