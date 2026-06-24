using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.RestockDtos;
using HerreraSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HerreraSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestocksController : ControllerBase
    {
        private readonly IRestockService _restockService;

        public RestocksController(IRestockService restockService)
        {
            _restockService = restockService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] RestockQueryParams queryParams)
        {
            var data = await _restockService.GetAllAsync(queryParams);

            return Ok(ApiResponse<PagedResponse<RestockListItemDto>>.Ok(data));
        }

        [HttpGet("{id}/detail")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var data = await _restockService.GetDetailAsync(id);

            if (data is null)
                return NotFound(
                    ApiResponse<RestockDetailDto>.Fail($"Restock con Id {id} no encontrado"));

            return Ok(ApiResponse<RestockDetailDto>.Ok(data));
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var data = await _restockService.GetStatisticsAsync();

            return Ok(ApiResponse<RestockStatisticsDto>.Ok(data));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateRestockDto dto)
        {
            var result = await _restockService.CreateRestockAsync(dto);

            if (!result.Success)
                return BadRequest(ApiResponse<RestockResponseDto>.Fail(result.ErrorMessage!));

            return CreatedAtAction(
                nameof(Create),
                ApiResponse<RestockResponseDto>.Ok(result.Data!, "Restock creado exitosamente"));
        }
    }
}
