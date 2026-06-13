using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.MunicipalityDtos;
using HerreraSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HerreraSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MunicipalitiesController : ControllerBase
    {
        private readonly IMunicipalityService _municipalityService;

        public MunicipalitiesController(IMunicipalityService municipalityService)
        {
            _municipalityService = municipalityService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _municipalityService.GetAllAsync();

            return Ok(
                ApiResponse<IReadOnlyList<MunicipalityDto>>
                    .Ok(data));
        }

        // GET /api/municipalities/by-department/1
        [HttpGet("by-department/{departmentId}")]
        public async Task<IActionResult> GetByDepartment(int departmentId)
        {
            var result = await _municipalityService
                .GetByDepartmentAsync(departmentId);

            return Ok(
                ApiResponse<IReadOnlyList<MunicipalityDto>>
                    .Ok(result.Data!));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _municipalityService.GetByIdAsync(id);

            if (!result.Success)
                return NotFound(
                    ApiResponse<MunicipalityDto>
                        .Fail(result.ErrorMessage!));

            return Ok(
                ApiResponse<MunicipalityDto>
                    .Ok(result.Data!));
        }
    }
}
