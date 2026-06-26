namespace HerreraSystem.Application.DTOs.InventoryMovementDtos
{
    public class InventoryMovementDetailItemDto
    {
        public int Id { get; set; }

        public int BatchId { get; set; }

        public string? BatchCode { get; set; }

        public string? SourceLocationName { get; set; }

        public string? DestinationLocationName { get; set; }

        public int Quantity { get; set; }

        public decimal UnitCost { get; set; }

        public decimal? UnitPrice { get; set; }

        public string? CreatedByUserName { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
