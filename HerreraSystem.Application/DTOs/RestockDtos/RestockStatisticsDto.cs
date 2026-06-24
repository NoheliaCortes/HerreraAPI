namespace HerreraSystem.Application.DTOs.RestockDtos
{
    public class RestockStatisticsDto
    {
        public int RestocksThisMonth { get; set; }

        public decimal TotalInvestmentThisMonth { get; set; }

        public int BatchesCreatedThisMonth { get; set; }
    }
}
