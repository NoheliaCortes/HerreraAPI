namespace HerreraSystem.Application.DTOs.ReportDtos;

public class TopCustomerDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string MunicipalityName { get; set; } = string.Empty;
    public int UnitsPurchased { get; set; }
}
