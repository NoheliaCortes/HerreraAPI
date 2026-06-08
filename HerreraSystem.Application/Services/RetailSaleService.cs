using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.SaleDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Application.Interfaces.Services;
using HerreraSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Services
{
    public class RetailSaleService : IRetailSaleService
    {
        // IDs fijos de negocio — valores conocidos del sistema
        private const int GenericCustomerId = 1;
        private const int MostradorLocationId = 2; // Location "Mostrador"
        private const int PaymentTypeContado = 1; // PaymentType "Contado"
        private const int SaleTypeDetalle = 1; // SaleType "Detalle"
        private const int MovementTypeEgreso = 2; // MovementType Sign = -1 (salida)

        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductRepository _productRepository;
        private readonly IProductPriceRepository _productPriceRepository;
        private readonly ISaleRepository _saleRepository;
        private readonly ISaleDetailRepository _saleDetailRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IBatchLocationRepository _batchLocationRepository;
        private readonly IInventoryMovementRepository _inventoryMovementRepository;
        private readonly IMovementDetailRepository _movementDetailRepository;

        public RetailSaleService(
            IUnitOfWork unitOfWork,
            IProductRepository productRepository,
            IProductPriceRepository productPriceRepository,
            ISaleRepository saleRepository,
            ISaleDetailRepository saleDetailRepository,
            IPaymentRepository paymentRepository,
            IBatchLocationRepository batchLocationRepository,
            IInventoryMovementRepository inventoryMovementRepository,
            IMovementDetailRepository movementDetailRepository)
        {
            _unitOfWork = unitOfWork;
            _productRepository = productRepository;
            _productPriceRepository = productPriceRepository;
            _saleRepository = saleRepository;
            _saleDetailRepository = saleDetailRepository;
            _paymentRepository = paymentRepository;
            _batchLocationRepository = batchLocationRepository;
            _inventoryMovementRepository = inventoryMovementRepository;
            _movementDetailRepository = movementDetailRepository;
        }

        public async Task<ServiceResult<RetailSaleResponseDto>> CreateRetailSaleAsync(
            CreateRetailSaleDto dto)
        {
            // ══════════════════════════════════════════════════════════════════
            // FASE 1 — VALIDACIONES Y PRECIOS (fuera de la transacción)
            // Validamos todo antes de abrir la transacción para fallar rápido
            // sin consumir recursos de BD innecesariamente.
            // ══════════════════════════════════════════════════════════════════

            // Paso 1 — Validar productos y obtener precios vigentes
            var itemPrices = new Dictionary<int, decimal>(); // productId → precio

            foreach (var item in dto.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product is null)
                    return ServiceResult<RetailSaleResponseDto>.Fail(
                        $"El producto con Id {item.ProductId} no existe");

                if (product.IsActive != true)
                    return ServiceResult<RetailSaleResponseDto>.Fail(
                        $"El producto '{product.ProductName}' no está activo");

                // Paso 2 — Precio vigente tipo "Detalle"
                var price = await _productPriceRepository
                    .GetActivePriceAsync(item.ProductId, "Detalle");

                if (price is null)
                    return ServiceResult<RetailSaleResponseDto>.Fail(
                        $"El producto Id {item.ProductId} no tiene precio de Detalle vigente");

                itemPrices[item.ProductId] = price.Value;
            }

            // Paso 3 — Calcular TotalSale
            decimal totalSale = dto.Items
                .Sum(item => itemPrices[item.ProductId] * item.Quantity);

            // Generar SaleCode antes de la transacción (solo es un conteo)
            int year = DateTime.UtcNow.Year;
            int saleCount = await _saleRepository.CountByYearAsync(year);
            string saleCode = $"VTA-{year}-{(saleCount + 1):D4}";

            // ══════════════════════════════════════════════════════════════════
            // FASE 2 — TRANSACCIÓN
            // Todo lo que escribe en BD vive aquí. Si falla cualquier paso,
            // RollbackAsync deja la BD exactamente como estaba.
            // ══════════════════════════════════════════════════════════════════

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Paso 4 — Crear Sale
                var sale = await _saleRepository.CreateAsync(new Sale
                {
                    OrderId = null,
                    CustomerId = GenericCustomerId,
                    SaleDate = DateTime.UtcNow,
                    TotalSale = totalSale,
                    PaymentStatus = "Pagado",
                    PendingBalance = 0,
                    CreatedBy = dto.CreatedBy,
                    PaymentTypeId = PaymentTypeContado,
                    SaleTypeId = SaleTypeDetalle,
                    SaleCode = saleCode
                });

                // Paso 5 — Crear InventoryMovement (egreso, Sign = -1)
                var movement = await _inventoryMovementRepository.CreateAsync(
                    new InventoryMovement
                    {
                        MovementTypeId = MovementTypeEgreso,
                        SaleId = sale.Id,
                        OrderId = null,
                        MovementDate = DateTime.UtcNow,
                        Notes = dto.Notes,
                        CreatedBy = dto.CreatedBy,
                        IsActive = true
                    });

                // Paso 6 — Crear Payment
                await _paymentRepository.CreateAsync(new Payment
                {
                    SaleId = sale.Id,
                    PaymentMethodId = dto.PaymentMethodId,
                    AmountReceived = totalSale,
                    PaymentDate = DateTime.UtcNow,
                    TransactionReference = dto.TransactionReference,
                    RegisteredBy = dto.CreatedBy
                });

                // Paso 7, 8, 9, 10 — FIFO por cada producto
                // Estos pasos están unidos porque el FIFO produce tanto
                // SaleDetails como MovementDetails y descuentos de stock.
                var itemResponses = new List<SaleItemResponseDto>();

                foreach (var item in dto.Items)
                {
                    decimal appliedPrice = itemPrices[item.ProductId];
                    int remainingQty = item.Quantity;

                    // Paso 7 — Obtener lotes disponibles en Mostrador (FIFO)
                    // La consulta ya viene ordenada por RestockDate ASC desde el repository.
                    // Se ejecuta DENTRO de la transacción para evitar condiciones de carrera:
                    // otro proceso podría vaciar el stock entre la validación y el descuento.
                    var fifoLots = await _batchLocationRepository
                        .GetAvailableStockFifoAsync(item.ProductId, MostradorLocationId);

                    // Validar stock total antes de descontar
                    int totalAvailable = fifoLots.Sum(l => l.CurrentStock);
                    if (totalAvailable < item.Quantity)
                    {
                        // Stock insuficiente — lanzar excepción para activar el catch
                        throw new InvalidOperationException(
                            $"Stock insuficiente en Mostrador para el producto Id {item.ProductId}. " +
                            $"Disponible: {totalAvailable}, solicitado: {item.Quantity}");
                    }

                    // Paso 8 — SaleDetail (uno por producto, precio aplicado)
                    await _saleDetailRepository.CreateAsync(new SaleDetail
                    {
                        SaleId = sale.Id,
                        ProductId = item.ProductId,
                        BatchId = fifoLots.First().BatchId, // lote FIFO principal
                        Quantity = item.Quantity,
                        AppliedPrice = appliedPrice,
                        LineSubtotal = appliedPrice * item.Quantity
                    });

                    // Pasos 9 y 10 — Descontar stock lote por lote (FIFO)
                    // y crear un MovementDetail por cada lote tocado
                    foreach (var lot in fifoLots)
                    {
                        if (remainingQty <= 0) break;

                        int qtyFromThisLot = Math.Min(remainingQty, lot.CurrentStock);

                        // Paso 9 — Actualizar BatchLocation
                        var batchLocation = await _batchLocationRepository
                            .GetByBatchAndLocationAsync(lot.BatchId, MostradorLocationId);

                        batchLocation!.CurrentStock -= qtyFromThisLot;
                        await _batchLocationRepository.UpdateStockAsync(batchLocation);

                        // Paso 10 — MovementDetail por lote
                        await _movementDetailRepository.CreateAsync(new MovementDetail
                        {
                            MovementId = movement.Id,
                            BatchId = lot.BatchId,
                            SourceLocationId = MostradorLocationId,
                            DestinationLocationId = null,  // sale del sistema
                            Quantity = qtyFromThisLot,
                            UnitPrice = appliedPrice,
                            UnitCost = 0,     // costo ya registrado en el restock
                            CreatedBy = dto.CreatedBy,
                            CreatedAt = DateTime.UtcNow
                        });

                        remainingQty -= qtyFromThisLot;
                    }

                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    itemResponses.Add(new SaleItemResponseDto
                    {
                        ProductId = item.ProductId,
                        ProductName = product!.ProductName,
                        Quantity = item.Quantity,
                        AppliedPrice = appliedPrice,
                        LineSubtotal = appliedPrice * item.Quantity
                    });
                }

                // Paso 12 — Commit: todo persiste atómicamente
                await _unitOfWork.CommitAsync();

                return ServiceResult<RetailSaleResponseDto>.Ok(new RetailSaleResponseDto
                {
                    SaleId = sale.Id,
                    SaleCode = saleCode,
                    TotalSale = totalSale,
                    SaleDate = sale.SaleDate!.Value,
                    PaymentStatus = "Pagado",
                    InventoryMovementId = movement.Id,
                    Items = itemResponses
                });
            }
            catch (InvalidOperationException ex)
            {
                // Stock insuficiente — rollback y mensaje limpio al cliente
                await _unitOfWork.RollbackAsync();
                return ServiceResult<RetailSaleResponseDto>.Fail(ex.Message);
            }
            catch
            {
                // Cualquier otro error inesperado — rollback y relanzar
                // El ExceptionMiddleware lo captura y retorna 500
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
    }
}
