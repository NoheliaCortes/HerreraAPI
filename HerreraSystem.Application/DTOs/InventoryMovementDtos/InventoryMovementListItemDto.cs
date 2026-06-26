namespace HerreraSystem.Application.DTOs.InventoryMovementDtos
{
    public class InventoryMovementListItemDto
    {
        public int Id { get; set; }

        public int MovementTypeId { get; set; }

        public string MovementTypeName { get; set; } = null!;

        public DateTime? MovementDate { get; set; }

        public string? CreatedByUserName { get; set; }
    }
}
