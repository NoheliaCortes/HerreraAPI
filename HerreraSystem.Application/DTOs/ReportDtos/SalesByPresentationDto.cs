namespace HerreraSystem.Application.DTOs.ReportDtos;

public class SalesByPresentationDto
{
    public string PresentationName { get; set; } = string.Empty;
    public string LineName { get; set; } = string.Empty;
    public int UnitsSold { get; set; }
    public decimal ParticipationPercentage { get; set; }
}
