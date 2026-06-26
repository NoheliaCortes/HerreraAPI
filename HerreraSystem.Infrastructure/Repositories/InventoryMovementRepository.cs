using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.InventoryMovementDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Application.Interfaces.Services;
using HerreraSystem.Domain.Entities;
using HerreraSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HerreraSystem.Infrastructure.Repositories
{
    public class InventoryMovementRepository : IInventoryMovementRepository
    {
        private const int RestockId = 1;
        private const int TransferId = 1002;
        private const int PositiveAdjustmentId = 1003;
        private const int NegativeAdjustmentId = 1004;

        private readonly HerreraSystemContext _context;
        private readonly INicaraguaDateTimeService _dateTimeService;

        public InventoryMovementRepository(
            HerreraSystemContext context,
            INicaraguaDateTimeService dateTimeService)
        {
            _context = context;
            _dateTimeService = dateTimeService;
        }

        public async Task<InventoryMovement> CreateAsync(InventoryMovement movement)
        {
            _context.InventoryMovements.Add(movement);
            await _context.SaveChangesAsync();
            return movement;
        }

        public async Task<InventoryMovementStatsDto> GetStatsAsync()
        {
            var now = _dateTimeService.Now;
            var todayStart = now.Date;
            var tomorrowStart = todayStart.AddDays(1);

            var movementsToday = _context.InventoryMovements
                .AsNoTracking()
                .Where(m =>
                    m.MovementDate.HasValue &&
                    m.MovementDate.Value >= todayStart &&
                    m.MovementDate.Value < tomorrowStart);

            return new InventoryMovementStatsDto
            {
                MovementsToday = await movementsToday.CountAsync(),
                RestocksToday = await movementsToday.CountAsync(m => m.MovementTypeId == RestockId),
                TransfersToday = await movementsToday.CountAsync(m => m.MovementTypeId == TransferId),
                PositiveAdjustmentsToday = await movementsToday
                    .CountAsync(m => m.MovementTypeId == PositiveAdjustmentId),
                NegativeAdjustmentsToday = await movementsToday
                    .CountAsync(m => m.MovementTypeId == NegativeAdjustmentId)
            };
        }

        public async Task<PagedResponse<InventoryMovementListItemDto>> GetAllAsync(
            InventoryMovementQueryParams queryParams)
        {
            return await _context.InventoryMovements
                .AsNoTracking()
                .OrderByDescending(m => m.MovementDate)
                .ThenByDescending(m => m.Id)
                .Select(m => new InventoryMovementListItemDto
                {
                    Id = m.Id,
                    MovementTypeId = m.MovementTypeId,
                    MovementTypeName = m.MovementType.MovementTypeName,
                    MovementDate = m.MovementDate,
                    CreatedByUserName = m.CreatedByNavigation.UserName
                })
                .ToPagedResponseAsync(queryParams);
        }

        public async Task<InventoryMovementHeaderDto?> GetByIdAsync(int id)
        {
            return await _context.InventoryMovements
                .AsNoTracking()
                .Where(m => m.Id == id)
                .Select(m => new InventoryMovementHeaderDto
                {
                    Id = m.Id,
                    MovementTypeId = m.MovementTypeId,
                    MovementTypeName = m.MovementType.MovementTypeName,
                    SaleId = m.SaleId,
                    OrderId = m.OrderId,
                    MovementDate = m.MovementDate,
                    Notes = m.Notes,
                    CreatedByUserName = m.CreatedByNavigation.UserName
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<InventoryMovementDetailItemDto>> GetDetailsAsync(int id)
        {
            return await _context.MovementDetails
                .AsNoTracking()
                .Where(d => d.MovementId == id)
                .OrderBy(d => d.Id)
                .Select(d => new InventoryMovementDetailItemDto
                {
                    Id = d.Id,
                    BatchId = d.BatchId,
                    BatchCode = d.Batch.BatchCode,
                    SourceLocationName = d.SourceLocation == null
                        ? null
                        : d.SourceLocation.LocationName,
                    DestinationLocationName = d.DestinationLocation == null
                        ? null
                        : d.DestinationLocation.LocationName,
                    Quantity = d.Quantity,
                    UnitCost = d.UnitCost,
                    UnitPrice = d.UnitPrice,
                    CreatedByUserName = d.CreatedByNavigation.UserName,
                    CreatedAt = d.CreatedAt
                })
                .ToListAsync();
        }
    }
}
