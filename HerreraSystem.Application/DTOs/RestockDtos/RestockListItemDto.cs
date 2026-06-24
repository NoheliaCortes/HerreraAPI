namespace HerreraSystem.Application.DTOs.RestockDtos
{
    public class RestockListItemDto
    {
        public int RestockId { get; set; }

        public string RestockCode { get; set; } = null!;

        public DateTime? RestockDate { get; set; }

        public string UserName { get; set; } = null!;

        public int BatchCount { get; set; }

        public int TotalUnits { get; set; }

        public decimal TotalInvestment { get; set; }
    }
}
