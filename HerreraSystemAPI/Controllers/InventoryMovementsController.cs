using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.InventoryMovementDtos;
using HerreraSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
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
