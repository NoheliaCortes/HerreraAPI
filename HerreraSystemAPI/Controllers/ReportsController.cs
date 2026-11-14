using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.ReportDtos;
using HerreraSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HerreraSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportsService _reportsService;

        public ReportsController(IReportsService reportsService) => _reportsService = reportsService;

        [HttpGet("sales-by-line-flavor")]
        public async Task<IActionResult> GetSalesByLineFlavor([FromQuery] ReportDateRangeParams range)
            => Ok(ApiResponse<IReadOnlyList<SalesByLineFlavorDto>>.Ok(await _reportsService.GetSalesByLineFlavorAsync(range)));

        [HttpGet("monthly-sales-trend")]
        public async Task<IActionResult> GetMonthlySalesTrend([FromQuery] ReportDateRangeParams range)
            => Ok(ApiResponse<IReadOnlyList<MonthlySalesTrendDto>>.Ok(await _reportsService.GetMonthlySalesTrendAsync(range)));

        [HttpGet("sales-by-geography")]
        public async Task<IActionResult> GetSalesByGeography([FromQuery] ReportDateRangeParams range)
            => Ok(ApiResponse<IReadOnlyList<SalesByGeographyDto>>.Ok(await _reportsService.GetSalesByGeographyAsync(range)));

        [HttpGet("top-customers")]
        public async Task<IActionResult> GetTopCustomers([FromQuery] ReportDateRangeParams range)
            => Ok(ApiResponse<IReadOnlyList<TopCustomerDto>>.Ok(await _reportsService.GetTopCustomersAsync(range)));

        [HttpGet("sales-by-presentation")]
        public async Task<IActionResult> GetSalesByPresentation([FromQuery] ReportDateRangeParams range)
            => Ok(ApiResponse<IReadOnlyList<SalesByPresentationDto>>.Ok(await _reportsService.GetSalesByPresentationAsync(range)));
    }
}
