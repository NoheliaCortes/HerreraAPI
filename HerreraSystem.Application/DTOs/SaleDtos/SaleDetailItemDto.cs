namespace HerreraSystem.Application.DTOs.SaleDtos
{
    public class SaleDetailItemDto
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } = null!;

        public string BatchCode { get; set; } = null!;

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal LineSubtotal { get; set; }
    }
}
