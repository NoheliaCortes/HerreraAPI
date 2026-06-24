using HerreraSystem.Application.Common;

namespace HerreraSystem.Application.DTOs.RestockDtos
{
    public class RestockQueryParams : PaginationParams
    {
        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public string? Search { get; set; }
    }
}
