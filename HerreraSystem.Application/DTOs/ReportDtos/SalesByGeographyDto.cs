namespace HerreraSystem.Application.DTOs.ReportDtos;

public class SalesByGeographyDto
{
    public string DepartmentName { get; set; } = string.Empty;
    public string MunicipalityName { get; set; } = string.Empty;
    public int UnitsSold { get; set; }
    public decimal TotalAmount { get; set; }
}
