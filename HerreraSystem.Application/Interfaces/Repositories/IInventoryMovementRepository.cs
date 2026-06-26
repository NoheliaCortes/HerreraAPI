using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.InventoryMovementDtos;
using HerreraSystem.Domain.Entities;

namespace HerreraSystem.Application.Interfaces.Repositories
{
    public interface IInventoryMovementRepository
    {
        Task<InventoryMovement> CreateAsync(InventoryMovement movement);

        Task<InventoryMovementStatsDto> GetStatsAsync();

        Task<PagedResponse<InventoryMovementListItemDto>> GetAllAsync(
            InventoryMovementQueryParams queryParams);

        Task<InventoryMovementHeaderDto?> GetByIdAsync(int id);

        Task<IReadOnlyList<InventoryMovementDetailItemDto>> GetDetailsAsync(int id);
    }
}
