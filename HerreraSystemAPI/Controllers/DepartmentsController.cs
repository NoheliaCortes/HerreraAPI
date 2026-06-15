using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.DepartmentDtos;
using HerreraSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HerreraSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _departmentService.GetAllAsync();

            return Ok(
                ApiResponse<IReadOnlyList<DepartmentDto>>
                    .Ok(data));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _departmentService.GetByIdAsync(id);

            if (!result.Success)
                return NotFound(
                    ApiResponse<DepartmentDto>
                        .Fail(result.ErrorMessage!));

            return Ok(
                ApiResponse<DepartmentDto>
                    .Ok(result.Data!));
        }
    }
}
