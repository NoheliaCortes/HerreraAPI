namespace HerreraSystem.Application.DTOs.ReportDtos;

public class SalesByLineFlavorDto
{
    public string LineName { get; set; } = string.Empty;
    public string FlavorName { get; set; } = string.Empty;
    public int UnitsSold { get; set; }
    public decimal TotalAmount { get; set; }
}
