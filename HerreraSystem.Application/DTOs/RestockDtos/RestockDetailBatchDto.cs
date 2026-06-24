namespace HerreraSystem.Application.DTOs.RestockDtos
{
    public class RestockDetailBatchDto
    {
        public int BatchId { get; set; }

        public string? BatchCode { get; set; }

        public string ProductName { get; set; } = null!;

        public string BatchStatusName { get; set; } = null!;

        public int InitialQuantity { get; set; }

        public decimal UnitProductionCost { get; set; }

        public decimal TotalCost { get; set; }

        public DateOnly ExpirationDate { get; set; }
    }
}
