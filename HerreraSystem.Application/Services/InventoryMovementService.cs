using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.InventoryMovementDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Application.Interfaces.Services;
using HerreraSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Services
{
    public class InventoryMovementService:IInventoryMovementService
    {
        private const int TransferenciaId = 1002;
        private const int AjustePositivoId = 1003;
        private const int AjusteNegativoId = 1004;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IBatchRepository _batchRepository;
        private readonly IBatchLocationRepository _batchLocationRepository;
        private readonly IInventoryMovementRepository _inventoryMovementRepository;
        private readonly IMovementDetailRepository _movementDetailRepository;

        public InventoryMovementService(
            IUnitOfWork unitOfWork,
            IBatchRepository batchRepository,
            IBatchLocationRepository batchLocationRepository,
            IInventoryMovementRepository inventoryMovementRepository,
            IMovementDetailRepository movementDetailRepository)
        {
            _unitOfWork = unitOfWork;
            _batchRepository = batchRepository;
            _batchLocationRepository = batchLocationRepository;
            _inventoryMovementRepository = inventoryMovementRepository;
            _movementDetailRepository = movementDetailRepository;
        }

        // ── Transferencia ────────────────────────────────────────────────────

        public async Task<ServiceResult<InventoryMovementResultDto>> TransferAsync(
            CreateTransferDto dto)
        {
            // Validación de forma antes de abrir transacción
            foreach (var item in dto.Details)
            {
                if (item.SourceLocationId == item.DestinationLocationId)
                    return ServiceResult<InventoryMovementResultDto>.Fail(
                        $"La ubicación origen y destino no pueden ser la misma " +
                        $"(lote {item.BatchId})");
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var movement = await _inventoryMovementRepository.CreateAsync(
                    new InventoryMovement
                    {
                        MovementTypeId = TransferenciaId,
                        MovementDate = DateTime.UtcNow,
                        Notes = dto.Notes,
                        CreatedBy = dto.CreatedBy,
                        IsActive = true
                    });

                var createdDetails = new List<MovementDetail>();

                foreach (var item in dto.Details)
                {
                    var batch = await _batchRepository.GetByIdAsync(item.BatchId);
                    if (batch is null)
                        return await RollbackAndFail(
                            $"El lote con Id {item.BatchId} no existe");

                    // Validar origen
                    var source = await _batchLocationRepository
                        .GetByBatchAndLocationAsync(item.BatchId, item.SourceLocationId);

                    if (source is null)
                        return await RollbackAndFail(
                            $"El lote '{batch.BatchCode ?? batch.Id.ToString()}' " +
                            $"no tiene stock en la ubicación origen {item.SourceLocationId}");

                    if (source.CurrentStock < item.Quantity)
                        return await RollbackAndFail(
                            $"Stock insuficiente en lote '{batch.BatchCode ?? batch.Id.ToString()}' " +
                            $"ubicación {item.SourceLocationId}. " +
                            $"Disponible: {source.CurrentStock}, Solicitado: {item.Quantity}");

                    // Buscar o crear destino
                    var destination = await _batchLocationRepository
                        .GetByBatchAndLocationAsync(item.BatchId, item.DestinationLocationId);

                    if (destination is null)
                    {
                        // Primera vez que el lote llega a esta ubicación
                        destination = new BatchLocation
                        {
                            BatchId = item.BatchId,
                            LocationId = item.DestinationLocationId,
                            CurrentStock = 0
                        };
                        await _batchLocationRepository.CreateAsync(destination);
                    }

                    // Mover stock
                    source.CurrentStock -= item.Quantity;
                    destination.CurrentStock += item.Quantity;

                    await _batchLocationRepository.UpdateStockAsync(source);
                    await _batchLocationRepository.UpdateStockAsync(destination);

                    // Registrar detalle
                    var detail = await _movementDetailRepository.CreateAsync(
                        new MovementDetail
                        {
                            MovementId = movement.Id,
                            BatchId = item.BatchId,
                            SourceLocationId = item.SourceLocationId,
                            DestinationLocationId = item.DestinationLocationId,
                            Quantity = item.Quantity,
                            UnitCost = batch.UnitProductionCost,
                            CreatedBy = dto.CreatedBy,
                            CreatedAt = DateTime.UtcNow
                        });

                    createdDetails.Add(detail);
                }

                await _unitOfWork.CommitAsync();
                return ServiceResult<InventoryMovementResultDto>.Ok(
                    BuildResponse(movement, createdDetails));
            }
            catch (InvalidOperationException ex)
            {
                await _unitOfWork.RollbackAsync();
                return ServiceResult<InventoryMovementResultDto>.Fail(ex.Message);
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        // ── Ajuste Positivo ──────────────────────────────────────────────────

        public async Task<ServiceResult<InventoryMovementResultDto>> PositiveAdjustmentAsync(
            CreatePositiveAdjustmentDto dto)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var movement = await _inventoryMovementRepository.CreateAsync(
                    new InventoryMovement
                    {
                        MovementTypeId = AjustePositivoId,
                        MovementDate = DateTime.UtcNow,
                        Notes = dto.Notes,
                        CreatedBy = dto.CreatedBy,
                        IsActive = true
                    });

                var createdDetails = new List<MovementDetail>();

                foreach (var item in dto.Details)
                {
                    var batch = await _batchRepository.GetByIdAsync(item.BatchId);
                    if (batch is null)
                        return await RollbackAndFail(
                            $"El lote con Id {item.BatchId} no existe");

                    var batchLocation = await _batchLocationRepository
                        .GetByBatchAndLocationAsync(item.BatchId, item.LocationId);

                    if (batchLocation is null)
                    {
                        // Stock encontrado en una ubicación donde el sistema no
                        // tenía registro — se crea el BatchLocation automáticamente
                        batchLocation = new BatchLocation
                        {
                            BatchId = item.BatchId,
                            LocationId = item.LocationId,
                            CurrentStock = 0
                        };
                        await _batchLocationRepository.CreateAsync(batchLocation);
                    }

                    batchLocation.CurrentStock += item.Quantity;
                    await _batchLocationRepository.UpdateStockAsync(batchLocation);

                    var detail = await _movementDetailRepository.CreateAsync(
                        new MovementDetail
                        {
                            MovementId = movement.Id,
                            BatchId = item.BatchId,
                            SourceLocationId = null,       // sin origen: hallazgo físico
                            DestinationLocationId = item.LocationId,
                            Quantity = item.Quantity,
                            UnitCost = batch.UnitProductionCost,
                            CreatedBy = dto.CreatedBy,
                            CreatedAt = DateTime.UtcNow
                        });

                    createdDetails.Add(detail);
                }

                await _unitOfWork.CommitAsync();
                return ServiceResult<InventoryMovementResultDto>.Ok(
                    BuildResponse(movement, createdDetails));
            }
            catch (InvalidOperationException ex)
            {
                await _unitOfWork.RollbackAsync();
                return ServiceResult<InventoryMovementResultDto>.Fail(ex.Message);
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        // ── Ajuste Negativo ──────────────────────────────────────────────────

        public async Task<ServiceResult<InventoryMovementResultDto>> NegativeAdjustmentAsync(
            CreateNegativeAdjustmentDto dto)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var movement = await _inventoryMovementRepository.CreateAsync(
                    new InventoryMovement
                    {
                        MovementTypeId = AjusteNegativoId,
                        MovementDate = DateTime.UtcNow,
                        Notes = dto.Notes,
                        CreatedBy = dto.CreatedBy,
                        IsActive = true
                    });

                var createdDetails = new List<MovementDetail>();

                foreach (var item in dto.Details)
                {
                    var batch = await _batchRepository.GetByIdAsync(item.BatchId);
                    if (batch is null)
                        return await RollbackAndFail(
                            $"El lote con Id {item.BatchId} no existe");

                    var batchLocation = await _batchLocationRepository
                        .GetByBatchAndLocationAsync(item.BatchId, item.LocationId);

                    if (batchLocation is null)
                        return await RollbackAndFail(
                            $"El lote '{batch.BatchCode ?? batch.Id.ToString()}' " +
                            $"no tiene stock en la ubicación {item.LocationId}");

                    if (batchLocation.CurrentStock < item.Quantity)
                        return await RollbackAndFail(
                            $"Stock insuficiente para ajuste negativo en lote " +
                            $"'{batch.BatchCode ?? batch.Id.ToString()}' " +
                            $"ubicación {item.LocationId}. " +
                            $"Disponible: {batchLocation.CurrentStock}, Solicitado: {item.Quantity}");

                    batchLocation.CurrentStock -= item.Quantity;
                    await _batchLocationRepository.UpdateStockAsync(batchLocation);

                    var detail = await _movementDetailRepository.CreateAsync(
                        new MovementDetail
                        {
                            MovementId = movement.Id,
                            BatchId = item.BatchId,
                            SourceLocationId = item.LocationId,
                            DestinationLocationId = null,       // sin destino: pérdida física
                            Quantity = item.Quantity,
                            UnitCost = batch.UnitProductionCost,
                            CreatedBy = dto.CreatedBy,
                            CreatedAt = DateTime.UtcNow
                        });

                    createdDetails.Add(detail);
                }

                await _unitOfWork.CommitAsync();
                return ServiceResult<InventoryMovementResultDto>.Ok(
                    BuildResponse(movement, createdDetails));
            }
            catch (InvalidOperationException ex)
            {
                await _unitOfWork.RollbackAsync();
                return ServiceResult<InventoryMovementResultDto>.Fail(ex.Message);
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        // ── Helpers privados ─────────────────────────────────────────────────

        private async Task<ServiceResult<InventoryMovementResultDto>> RollbackAndFail(
            string message)
        {
            await _unitOfWork.RollbackAsync();
            return ServiceResult<InventoryMovementResultDto>.Fail(message);
        }

        private static InventoryMovementResultDto BuildResponse(
            InventoryMovement movement,
            List<MovementDetail> details)
        {
            return new InventoryMovementResultDto
            {
                Id = movement.Id,
                MovementTypeId = movement.MovementTypeId,
                MovementDate = movement.MovementDate,
                Notes = movement.Notes,
                CreatedBy = movement.CreatedBy,
                Details = details.Select(d => new MovementDetailResultDto
                {
                    Id = d.Id,
                    BatchId = d.BatchId,
                    SourceLocationId = d.SourceLocationId,
                    DestinationLocationId = d.DestinationLocationId,
                    Quantity = d.Quantity,
                    UnitCost = d.UnitCost
                }).ToList()
            };
        }

    }
}
