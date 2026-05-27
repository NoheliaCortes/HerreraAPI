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
