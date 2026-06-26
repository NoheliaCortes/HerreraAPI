using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.InventoryMovementDtos;
using HerreraSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HerreraSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryMovementsController : ControllerBase
    {
        private readonly IInventoryMovementService _movementService;

        public InventoryMovementsController(IInventoryMovementService movementService)
        {
            _movementService = movementService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var data = await _movementService.GetStatsAsync();

            return Ok(ApiResponse<InventoryMovementStatsDto>.Ok(data));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] InventoryMovementQueryParams queryParams)
        {
            var data = await _movementService.GetAllAsync(queryParams);

            return Ok(ApiResponse<PagedResponse<InventoryMovementListItemDto>>.Ok(data));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _movementService.GetByIdAsync(id);

            if (data is null)
                return NotFound(
                    ApiResponse<InventoryMovementHeaderDto>.Fail(
                        $"Movimiento de inventario con Id {id} no encontrado"));

            return Ok(ApiResponse<InventoryMovementHeaderDto>.Ok(data));
        }

        [HttpGet("{id}/details")]
        public async Task<IActionResult> GetDetails(int id)
        {
            var movement = await _movementService.GetByIdAsync(id);

            if (movement is null)
                return NotFound(
                    ApiResponse<List<InventoryMovementDetailItemDto>>.Fail(
                        $"Movimiento de inventario con Id {id} no encontrado"));

            var data = await _movementService.GetDetailsAsync(id);

            return Ok(ApiResponse<IReadOnlyList<InventoryMovementDetailItemDto>>.Ok(data));
        }

        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer(CreateTransferDto dto)
        {
            var result = await _movementService.TransferAsync(dto);
            if (!result.Success)
                return BadRequest(
                    ApiResponse<InventoryMovementResultDto>.Fail(result.ErrorMessage!));

            return CreatedAtAction(nameof(Transfer),
                ApiResponse<InventoryMovementResultDto>.Ok(
                    result.Data!, "Transferencia registrada exitosamente"));
        }

        [HttpPost("positive-adjustment")]
        public async Task<IActionResult> PositiveAdjustment(CreatePositiveAdjustmentDto dto)
        {
            var result = await _movementService.PositiveAdjustmentAsync(dto);
            if (!result.Success)
                return BadRequest(
                    ApiResponse<InventoryMovementResultDto>.Fail(result.ErrorMessage!));

            return CreatedAtAction(nameof(PositiveAdjustment),
                ApiResponse<InventoryMovementResultDto>.Ok(
                    result.Data!, "Ajuste positivo registrado exitosamente"));
        }

        [HttpPost("negative-adjustment")]
        public async Task<IActionResult> NegativeAdjustment(CreateNegativeAdjustmentDto dto)
        {
            var result = await _movementService.NegativeAdjustmentAsync(dto);
            if (!result.Success)
                return BadRequest(
                    ApiResponse<InventoryMovementResultDto>.Fail(result.ErrorMessage!));

            return CreatedAtAction(nameof(NegativeAdjustment),
                ApiResponse<InventoryMovementResultDto>.Ok(
                    result.Data!, "Ajuste negativo registrado exitosamente"));
        }
    }
}
