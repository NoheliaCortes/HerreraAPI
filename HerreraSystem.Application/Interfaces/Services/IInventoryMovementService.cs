using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.InventoryMovementDtos;

namespace HerreraSystem.Application.Interfaces.Services
{
    public interface IInventoryMovementService
    {
        Task<InventoryMovementStatsDto> GetStatsAsync();

        Task<PagedResponse<InventoryMovementListItemDto>> GetAllAsync(
            InventoryMovementQueryParams queryParams);

        Task<InventoryMovementHeaderDto?> GetByIdAsync(int id);

        Task<IReadOnlyList<InventoryMovementDetailItemDto>> GetDetailsAsync(int id);

        Task<ServiceResult<InventoryMovementResultDto>> TransferAsync(
            CreateTransferDto dto);

        Task<ServiceResult<InventoryMovementResultDto>> PositiveAdjustmentAsync(
            CreatePositiveAdjustmentDto dto);

        Task<ServiceResult<InventoryMovementResultDto>> NegativeAdjustmentAsync(
            CreateNegativeAdjustmentDto dto);
    }
}
