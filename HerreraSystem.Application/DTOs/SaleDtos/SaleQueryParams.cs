using HerreraSystem.Application.Common;

namespace HerreraSystem.Application.DTOs.SaleDtos
{
    public class SaleQueryParams : PaginationParams
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
