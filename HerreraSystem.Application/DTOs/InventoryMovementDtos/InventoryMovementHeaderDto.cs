namespace HerreraSystem.Application.DTOs.InventoryMovementDtos
{
    public class InventoryMovementHeaderDto
    {
        public int Id { get; set; }

        public int MovementTypeId { get; set; }

        public string MovementTypeName { get; set; } = null!;

        public int? SaleId { get; set; }

        public int? OrderId { get; set; }

        public DateTime? MovementDate { get; set; }

        public string? Notes { get; set; }

        public string? CreatedByUserName { get; set; }
    }
}
