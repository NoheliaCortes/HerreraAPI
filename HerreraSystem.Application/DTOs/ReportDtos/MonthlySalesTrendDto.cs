namespace HerreraSystem.Application.DTOs.ReportDtos;

public class MonthlySalesTrendDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string LineName { get; set; } = string.Empty;
    public int UnitsSold { get; set; }
}
