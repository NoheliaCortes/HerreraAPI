namespace HerreraSystem.Application.DTOs.InventoryMovementDtos
{
    public class InventoryMovementStatsDto
    {
        public int MovementsToday { get; set; }

        public int RestocksToday { get; set; }

        public int TransfersToday { get; set; }

        public int PositiveAdjustmentsToday { get; set; }

        public int NegativeAdjustmentsToday { get; set; }
    }
}
