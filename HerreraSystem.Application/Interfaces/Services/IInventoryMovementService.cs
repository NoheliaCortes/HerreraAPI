using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.InventoryMovementDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Interfaces.Services
{
    public interface IInventoryMovementService
    {
        Task<ServiceResult<InventoryMovementResultDto>> TransferAsync(
            CreateTransferDto dto);

        Task<ServiceResult<InventoryMovementResultDto>> PositiveAdjustmentAsync(
            CreatePositiveAdjustmentDto dto);

        Task<ServiceResult<InventoryMovementResultDto>> NegativeAdjustmentAsync(
            CreateNegativeAdjustmentDto dto);
    }
}
