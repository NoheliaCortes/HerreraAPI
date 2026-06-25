using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.RestockDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Application.Interfaces.Services;
using HerreraSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;


namespace HerreraSystem.Application.Services
{
    public class RestockService:IRestockService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductRepository _productRepository;
        private readonly IRestockRepository _restockRepository;
        private readonly IBatchRepository _batchRepository;
        private readonly IBatchLocationRepository _batchLocationRepository;
        private readonly IInventoryMovementRepository _inventoryMovementRepository;
        private readonly IMovementDetailRepository _movementDetailRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly INicaraguaDateTimeService _dateTimeService;

        public RestockService(
            IUnitOfWork unitOfWork,
            IProductRepository productRepository,
            IRestockRepository restockRepository,
            IBatchRepository batchRepository,
            IBatchLocationRepository batchLocationRepository,
            IInventoryMovementRepository inventoryMovementRepository,
            IMovementDetailRepository movementDetailRepository,
            ICurrentUserService currentUserService,
            INicaraguaDateTimeService dateTimeService)
        {
            _unitOfWork = unitOfWork;
            _productRepository = productRepository;
            _restockRepository = restockRepository;
            _batchRepository = batchRepository;
            _batchLocationRepository = batchLocationRepository;
            _inventoryMovementRepository = inventoryMovementRepository;
            _movementDetailRepository = movementDetailRepository;
            _currentUserService = currentUserService;
            _dateTimeService = dateTimeService;
        }

        public async Task<ServiceResult<RestockResponseDto>> CreateRestockAsync(CreateRestockDto dto)
        {
            // ── VALIDACIONES (fuera de la transacción) ───────────────────────
            if (!_currentUserService.IsAuthenticated || _currentUserService.CurrentUserId is null)
                return ServiceResult<RestockResponseDto>.Fail(
                    "No se pudo identificar el usuario autenticado");

            var currentUserId = _currentUserService.CurrentUserId.Value;
            var now = _dateTimeService.Now;

            if (!dto.Batches.Any())
                return ServiceResult<RestockResponseDto>.Fail(
                    "Debe incluir al menos un lote");

            foreach (var batchDto in dto.Batches)
            {
                var product = await _productRepository.GetByIdAsync(batchDto.ProductId);
                if (product is null)
                    return ServiceResult<RestockResponseDto>.Fail(
                        $"El producto con Id {batchDto.ProductId} no existe");

                if (batchDto.ExpirationDate <= DateOnly.FromDateTime(now))
                    return ServiceResult<RestockResponseDto>.Fail(
                        $"La fecha de vencimiento del producto Id {batchDto.ProductId} debe ser futura");
            }

            // ── GENERACIÓN DE CÓDIGOS ────────────────────────────────────────
            int year = now.Year;
            int restockCount = await _restockRepository.CountByYearAsync(year);
            string restockCode = $"RST-{year}-{(restockCount + 1):D4}";
            int batchCount = await _batchRepository.CountByYearAsync(year);

            // ── TRANSACCIÓN ──────────────────────────────────────────────────
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // PASO 1 — InventoryMovement
                var movement = await _inventoryMovementRepository.CreateAsync(new InventoryMovement
                {
                    MovementTypeId = 1,
                    SaleId = null,
                    OrderId = null,
                    MovementDate = now,
                    Notes = dto.Notes,
                    CreatedBy = currentUserId,
                    IsActive = true
                });

                // PASO 2 — Restock
                var restock = await _restockRepository.CreateAsync(new Restock
                {
                    RestockDate = now,
                    CreatedBy = currentUserId,
                    RestockCode = restockCode
                });

                // PASOS 3-4-5 — Por cada lote
                var batchResponses = new List<RestockBatchResponseDto>();

                foreach (var (batchDto, index) in dto.Batches.Select((b, i) => (b, i)))
                {
                    string batchCode = await _batchRepository.BuildBatchCodeAsync(
                        batchDto.ProductId, year, batchCount + index + 1);

                    var batch = await _batchRepository.CreateAsync(new Batch
                    {
                        RestockId = restock.Id,
                        ProductId = batchDto.ProductId,
                        BatchStatusId = 1,
                        InitialQuantity = batchDto.Quantity,
                        UnitProductionCost = batchDto.UnitProductionCost,
                        ExpirationDate = batchDto.ExpirationDate,
                        BatchCode = batchCode
                    });

                    await _batchLocationRepository.CreateAsync(new BatchLocation
                    {
                        BatchId = batch.Id,
                        LocationId = 1,
                        CurrentStock = batchDto.Quantity
                    });

                    var productDetail = await _productRepository.GetByIdAsync(batchDto.ProductId);

                    await _movementDetailRepository.CreateAsync(new MovementDetail
                    {
                        MovementId = movement.Id,
                        BatchId = batch.Id,
                        SourceLocationId = null,
                        DestinationLocationId = 1,
                        Quantity = batchDto.Quantity,
                        UnitPrice = null,
                        UnitCost = batchDto.UnitProductionCost,
                        CreatedBy = currentUserId,
                        CreatedAt = now
                    });

                    batchResponses.Add(new RestockBatchResponseDto
                    {
                        BatchId = batch.Id,
                        BatchCode = batchCode,
                        ProductName = productDetail!.ProductName,
                        Quantity = batchDto.Quantity,
                        UnitProductionCost = batchDto.UnitProductionCost,
                        ExpirationDate = batchDto.ExpirationDate
                    });
                }

                await _unitOfWork.CommitAsync();

                return ServiceResult<RestockResponseDto>.Ok(new RestockResponseDto
                {
                    RestockId = restock.Id,
                    RestockCode = restockCode,
                    InventoryMovementId = movement.Id,
                    RestockDate = restock.RestockDate!.Value,
                    Batches = batchResponses
                });
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<PagedResponse<RestockListItemDto>> GetAllAsync(RestockQueryParams queryParams)
            => await _restockRepository.GetAllAsync(queryParams);

        public async Task<RestockDetailDto?> GetDetailAsync(int id)
            => await _restockRepository.GetDetailAsync(id);

        public async Task<RestockStatisticsDto> GetStatisticsAsync()
            => await _restockRepository.GetStatisticsAsync();
    }
}
