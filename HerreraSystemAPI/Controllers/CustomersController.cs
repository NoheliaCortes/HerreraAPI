using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.CustomerDtos;
using HerreraSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HerreraSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(
            ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] PaginationParams paginationParams,
            [FromQuery] string? search,
            [FromQuery] int? departmentId,
            [FromQuery] int? municipalityId)
        {
            var data = await _customerService
                .GetAllAsync(paginationParams, search, departmentId, municipalityId);

            return Ok(ApiResponse<PagedResponse<CustomerDto>>.Ok(data));
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var result = await _customerService.GetStatsAsync();

            if (!result.Success)
                return BadRequest(ApiResponse<CustomerStatsDto>.Fail(result.ErrorMessage!));

            return Ok(ApiResponse<CustomerStatsDto>.Ok(result.Data!));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _customerService
                .GetByIdAsync(id);

            if (!result.Success)
                return NotFound(
                    ApiResponse<CustomerDto>
                        .Fail(result.ErrorMessage!));

            return Ok(
                ApiResponse<CustomerDto>
                    .Ok(result.Data!));
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            CreateCustomerDto dto)
        {
            var result = await _customerService
                .CreateAsync(dto);

            if (!result.Success)
                return BadRequest(
                    ApiResponse<CustomerDto>
                        .Fail(result.ErrorMessage!));

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Data!.Id },
                ApiResponse<CustomerDto>.Ok(
                    result.Data,
                    "Cliente creado exitosamente"));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            UpdateCustomerDto dto)
        {
            var result = await _customerService
                .UpdateAsync(id, dto);

            if (!result.Success)
            {
                if (result.ErrorMessage!
                    .Contains("no encontrado"))
                {
                    return NotFound(
                        ApiResponse<object>
                            .Fail(result.ErrorMessage));
                }

                return BadRequest(
                    ApiResponse<object>
                        .Fail(result.ErrorMessage));
            }

            return Ok(
                ApiResponse<object>
                    .Ok(null!,
                        "Cliente actualizado exitosamente"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _customerService
                .DeleteAsync(id);

            if (!result.Success)
            {
                if (result.ErrorMessage!
                    .Contains("no encontrado"))
                {
                    return NotFound(
                        ApiResponse<object>
                            .Fail(result.ErrorMessage));
                }

                return BadRequest(
                    ApiResponse<object>
                        .Fail(result.ErrorMessage));
            }

            return Ok(
                ApiResponse<object>
                    .Ok(null!,
                        "Cliente eliminado exitosamente"));
        }

    }
}
