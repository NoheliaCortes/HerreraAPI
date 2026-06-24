namespace HerreraSystem.Application.DTOs.SaleDtos
{
    public class SaleCustomerInfoDto
    {
        public int Id { get; set; }

        public string FullName { get; set; } = null!;

        public string? DepartmentName { get; set; }

        public string? MunicipalityName { get; set; }

        public string? PointOfSale { get; set; }
    }
}
