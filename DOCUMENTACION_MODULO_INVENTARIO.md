# Documentacion Tecnica - Modulo de Inventario

## 1. Alcance

Este documento describe el estado actual del modulo de inventario en la API `HerreraSystem.API`, incluyendo los contratos necesarios para construir una integracion Front-end completa para:

- Listado de inventario.
- Consulta de lotes disponibles por producto y detalle completo de lote.
- Listado, creacion, consulta, actualizacion, patch y eliminacion de productos.
- Carga de imagenes de productos.
- Reabastecimientos, lotes y movimientos de inventario.
- Catalogos auxiliares requeridos por formularios: lineas, sabores, presentaciones y relaciones linea-presentacion.

La documentacion se basa en el codigo fuente actual de controladores, DTOs, entidades EF Core, servicios, repositorios y `HerreraSystemContext`.

## Zona horaria operativa

Las fechas automaticas asociadas a inventario, movimientos y detalles de movimiento usan hora local de Nicaragua (`America/Managua`, UTC-06:00), obtenida desde un servicio centralizado. Las fechas ingresadas por el usuario, como vencimientos de lote, no se convierten automaticamente.

## 2. Convenciones Generales de API

### 2.1 Envoltura `ApiResponse<T>`

Namespace: `HerreraSystem.Application.Common`

Todas las respuestas documentadas usan la envoltura:

| Propiedad | C# | TypeScript | Nullable | Descripcion |
|---|---:|---:|---:|---|
| `Success` | `bool` | `boolean` | No | `true` si la operacion fue exitosa. |
| `Message` | `string` | `string` | No | Mensaje descriptivo. Default exitoso: `Operación exitosa`. |
| `Data` | `T?` | `T \| null` | Si | Payload de respuesta. En errores queda `null`, salvo validaciones automaticas que devuelven lista de errores. |

TypeScript recomendado:

```ts
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
}
```

### 2.2 Paginacion `PaginationParams`

Clase: `HerreraSystem.Application.Common.PaginationParams`

| Query | C# | TypeScript | Default | Reglas |
|---|---:|---:|---:|---|
| `page` / `Page` | `int` | `number` | `1` | Sin Data Annotation. |
| `pageSize` / `PageSize` | `int` | `number` | `10` | Maximo por setter: `50`. Si se envia `PageSize > 50`, se usa `50`. |

### 2.3 Respuesta paginada `PagedResponse<T>`

Clase: `HerreraSystem.Application.Common.PagedResponse<T>`

| Propiedad JSON | C# | TypeScript | Nullable | Descripcion |
|---|---:|---:|---:|---|
| `data` | `IEnumerable<T>` | `T[]` | No | Registros de la pagina actual. |
| `currentPage` | `int` | `number` | No | Pagina actual. Equivale al `Page` recibido. |
| `pageSize` | `int` | `number` | No | Tamano aplicado. |
| `totalRecords` | `int` | `number` | No | Total de registros antes de `Skip/Take`. |
| `totalPages` | `int` | `number` | No | `Ceiling(totalRecords / pageSize)`. |
| `hasNextPage` | `bool` getter | `boolean` | No | `currentPage < totalPages`. |
| `hasPreviousPage` | `bool` getter | `boolean` | No | `currentPage > 1`. |

TypeScript recomendado:

```ts
export interface PagedResponse<T> {
  data: T[];
  currentPage: number;
  pageSize: number;
  totalRecords: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}
```

### 2.4 Validacion de modelo

`Program.cs` configura `InvalidModelStateResponseFactory`. Si falla una Data Annotation, la API responde:

- HTTP `400 Bad Request`.
- `ApiResponse<List<string>>`.
- `message`: `Errores de validación` (en el archivo aparece con mojibake como `Errores de validaciÃ³n`).
- `data`: lista de mensajes de validacion.

### 2.5 Seguridad y token

`Program.cs` configura autenticacion JWT Bearer:

- Header esperado cuando se use autenticacion: `Authorization: Bearer <token>`.
- Validaciones: issuer, audience, lifetime y signing key.
- Swagger declara esquema `Bearer`.
- `app.UseAuthentication()` y `app.UseAuthorization()` estan activos.

Estado real de los endpoints de inventario/productos: no tienen `[Authorize]`, `[Authorize(Roles=...)]` ni politicas. Por lo tanto, actualmente no exigen token ni rol especial aunque la aplicacion soporte JWT.

## 3. Entidades EF Core Involucradas

Las entidades estan en `HerreraSystem.Domain.Entities`. No usan Data Annotations; las restricciones de base se configuran en `HerreraSystem.Infrastructure.Data.HerreraSystemContext`.

### 3.1 `Product`

Tabla conceptual: `Products`.

| Propiedad | C# | TypeScript | Nullable | EF Core / BD | Uso Front-end |
|---|---:|---:|---:|---|---|
| `Id` | `int` | `number` | No | PK | Identificador. |
| `LinePresentationId` | `int` | `number` | No | FK `LinePresentation`, `ClientSetNull` | Combinacion linea-presentacion del producto. |
| `FlavorId` | `int` | `number` | No | FK `Flavor`, `ClientSetNull` | Sabor. |
| `ProductName` | `string` | `string` | No | `varchar(150)` | Nombre visible. |
| `IsActive` | `bool?` | `boolean \| null` | Si | Default `true` | Estado activo/inactivo. |
| `CreatedBy` | `int` | `number` | No | FK `User`, `ClientSetNull` | Usuario creador. |
| `CreatedAt` | `DateTime?` | `string \| null` | Si | `datetime`, default SQL `getdate()`; repositorio usa `DateTime.UtcNow` al crear | Fecha de creacion. |
| `ImageUrl` | `string?` | `string \| null` | Si | `varchar(2048)`, columna `ImageURL` | URL relativa o absoluta de imagen. |
| `MinimumStock` | `int` | `number` | No | Sin regla EF explicita | Stock minimo. |

Navegaciones: `Batches`, `CreatedByNavigation`, `Flavor`, `LinePresentation`, `OrderDetails`, `ProductPrices`, `SaleDetails`.

### 3.2 `LinePresentation`

| Propiedad | C# | TypeScript | Nullable | EF Core / BD |
|---|---:|---:|---:|---|
| `Id` | `int` | `number` | No | PK |
| `LineId` | `int` | `number` | No | FK `Line` |
| `PresentationId` | `int` | `number` | No | FK `Presentation` |

Restriccion: indice unico `UQ_Line_Presentation` sobre `(LineId, PresentationId)`.

### 3.3 `Line`

| Propiedad | C# | TypeScript | Nullable | EF Core / BD |
|---|---:|---:|---:|---|
| `Id` | `int` | `number` | No | PK |
| `LineName` | `string` | `string` | No | `varchar(100)` |
| `IsActive` | `bool?` | `boolean \| null` | Si | Default `true` |

### 3.4 `Presentation`

| Propiedad | C# | TypeScript | Nullable | EF Core / BD |
|---|---:|---:|---:|---|
| `Id` | `int` | `number` | No | PK |
| `PresentationName` | `string` | `string` | No | `varchar(100)` |
| `IsActive` | `bool?` | `boolean \| null` | Si | Default `true` |

### 3.5 `Flavor`

| Propiedad | C# | TypeScript | Nullable | EF Core / BD |
|---|---:|---:|---:|---|
| `Id` | `int` | `number` | No | PK |
| `FlavorName` | `string` | `string` | No | `varchar(100)` |
| `IsActive` | `bool?` | `boolean \| null` | Si | Default `true` |
| `ImageUrl` | `string?` | `string \| null` | Si | `varchar(2048)`, columna `ImageURL` |
| `FlavorColor` | `string?` | `string \| null` | Si | `varchar(7)` |

### 3.6 `ProductPrice` y `PriceType`

`ProductPrice` permite precios por producto o precios generales por `LinePresentation`. En el modulo de inventario se consultan precios generales con `ProductId == null`.

| Propiedad | C# | TypeScript | Nullable | EF Core / BD |
|---|---:|---:|---:|---|
| `Id` | `int` | `number` | No | PK |
| `PriceTypeId` | `int` | `number` | No | FK `PriceType` |
| `LinePresentationId` | `int?` | `number \| null` | Si | FK opcional |
| `ProductId` | `int?` | `number \| null` | Si | FK opcional |
| `Price` | `decimal` | `number` | No | `decimal(10,2)` |
| `ValidFrom` | `DateTime` | `string` | No | `datetime`, default `getdate()` |
| `ValidTo` | `DateTime?` | `string \| null` | Si | `datetime` |
| `IsActive` | `bool?` | `boolean \| null` | Si | Default `true` |
| `CreatedBy` | `int` | `number` | No | FK `User` |
| `CreatedAt` | `DateTime?` | `string \| null` | Si | `datetime`, default `getdate()` |

Constantes usadas:

| Id | Nombre de negocio |
|---:|---|
| `1` | Detalle / Retail |
| `2` | Mayoreo / Wholesale |

### 3.7 `Batch`

| Propiedad | C# | TypeScript | Nullable | EF Core / BD |
|---|---:|---:|---:|---|
| `Id` | `int` | `number` | No | PK |
| `RestockId` | `int` | `number` | No | FK `Restock` |
| `ProductId` | `int` | `number` | No | FK `Product` |
| `BatchStatusId` | `int` | `number` | No | FK `BatchStatus` |
| `InitialQuantity` | `int` | `number` | No | Cantidad inicial |
| `UnitProductionCost` | `decimal` | `number` | No | `decimal(10,2)` |
| `ExpirationDate` | `DateOnly` | `string` | No | Fecha `YYYY-MM-DD` |
| `BatchCode` | `string?` | `string \| null` | Si | `varchar(50)`, unico |

Reglas de servicio: al crear restock se usa `BatchStatusId = 1` y se genera `BatchCode` con formato:

```txt
{LINEA3}-{SABOR3}-{PRESENTACION3}-{YEAR}-{CORRELATIVO_4_DIGITOS}
```

### 3.8 `BatchLocation`

| Propiedad | C# | TypeScript | Nullable | EF Core / BD |
|---|---:|---:|---:|---|
| `Id` | `int` | `number` | No | PK |
| `BatchId` | `int` | `number` | No | FK `Batch` |
| `LocationId` | `int` | `number` | No | FK `Location` |
| `CurrentStock` | `int` | `number` | No | Stock actual |

Restriccion: indice unico `UQ_BatchLocation` sobre `(BatchId, LocationId)`.

### 3.9 `Location`

| Propiedad | C# | TypeScript | Nullable | EF Core / BD |
|---|---:|---:|---:|---|
| `Id` | `int` | `number` | No | PK |
| `LocationName` | `string` | `string` | No | `varchar(100)` |
| `IsActive` | `bool?` | `boolean \| null` | Si | Default `true` |

Ids usados por servicios/repositorios:

| Id | Uso observado |
|---:|---|
| `1` | Bodega / Warehouse. Restock entra aqui. |
| `2` | Mostrador / Display. Ventas minoristas usan este id en `RetailSaleService`. |
| `3` | Reservado / Reserved. |

Nota: `InventoryProductDto` contiene comentarios invertidos en el archivo fuente, pero la consulta real asigna `DisplayStock` desde `LocationId == 2` y `WarehouseStock` desde `LocationId == 1`.

### 3.10 `Restock`

| Propiedad | C# | TypeScript | Nullable | EF Core / BD |
|---|---:|---:|---:|---|
| `Id` | `int` | `number` | No | PK |
| `RestockDate` | `DateTime?` | `string \| null` | Si | `datetime`, default `getdate()`; servicio usa `DateTime.UtcNow` |
| `CreatedBy` | `int` | `number` | No | FK `User` |
| `RestockCode` | `string` | `string` | No | `varchar(50)`, unico |

Formato generado:

```txt
RST-{YEAR}-{CORRELATIVO_4_DIGITOS}
```

### 3.11 `InventoryMovement`

| Propiedad | C# | TypeScript | Nullable | EF Core / BD |
|---|---:|---:|---:|---|
| `Id` | `int` | `number` | No | PK |
| `MovementTypeId` | `int` | `number` | No | FK `MovementType` |
| `SaleId` | `int?` | `number \| null` | Si | FK opcional |
| `OrderId` | `int?` | `number \| null` | Si | FK opcional |
| `MovementDate` | `DateTime?` | `string \| null` | Si | `datetime`, default `getdate()`; servicios usan `DateTime.UtcNow` |
| `Notes` | `string?` | `string \| null` | Si | `varchar(max)` |
| `CreatedBy` | `int` | `number` | No | FK `User` |
| `IsActive` | `bool?` | `boolean \| null` | Si | Default `true` |

Tipos usados por servicios:

| Id | Uso |
|---:|---|
| `1` | Entrada por restock. |
| `1002` | Transferencia. |
| `1003` | Ajuste positivo. |
| `1004` | Ajuste negativo. |

### 3.12 `MovementDetail`

| Propiedad | C# | TypeScript | Nullable | EF Core / BD |
|---|---:|---:|---:|---|
| `Id` | `int` | `number` | No | PK |
| `MovementId` | `int` | `number` | No | FK `InventoryMovement` |
| `BatchId` | `int` | `number` | No | FK `Batch` |
| `SourceLocationId` | `int?` | `number \| null` | Si | FK opcional |
| `DestinationLocationId` | `int?` | `number \| null` | Si | FK opcional |
| `Quantity` | `int` | `number` | No | Cantidad movida |
| `UnitPrice` | `decimal?` | `number \| null` | Si | `decimal(10,2)` |
| `UnitCost` | `decimal` | `number` | No | `decimal(10,2)` |
| `CreatedBy` | `int` | `number` | No | FK `User` |
| `CreatedAt` | `DateTime?` | `string \| null` | Si | `datetime`, default `getdate()` |

### 3.13 `Sale` y `SaleType` en calculos de lote

Para diferenciar ventas al detalle y ventas al mayoreo por lote, el endpoint de detalle usa la relacion:

```txt
Batch -> MovementDetails -> InventoryMovement -> Sale -> SaleTypeId
```

No se usa `SaleDetail.BatchId` para calcular ventas por lote porque el flujo FIFO actual puede guardar en `SaleDetail.BatchId` solo el lote principal de la venta, mientras que `MovementDetail.BatchId` registra cada lote realmente descontado.

Ids de `SaleType` confirmados:

| SaleTypeId | Significado |
|---:|---|
| `1` | Detalle |
| `2` | Mayoreo |

## 4. DTOs del Modulo

### 4.1 Productos

#### `CreateProductDto`

| Propiedad | C# | TypeScript | Obligatorio | Validaciones |
|---|---:|---:|---:|---|
| `LinePresentationId` | `int` | `number` | Si | `[Required(ErrorMessage = "El LinePresentationId es obligatorio")]` |
| `FlavorId` | `int` | `number` | Si | `[Required(ErrorMessage = "El FlavorId es obligatorio")]` |
| `ProductName` | `string` | `string` | Si | `[Required("El nombre del producto es obligatorio")]`, `[StringLength(150, "El nombre no puede exceder 150 caracteres")]` |
| `CreatedBy` | `int` | `number` | Si | `[Required("El CreatedBy es obligatorio")]` |
| `ImageUrl` | `string?` | `string \| null` | No | `[Url("La URL de la imagen no es válida")]` |
| `MinimumStock` | `int` | `number` | No tecnico, default `0` si se omite | `[Range(0, int.MaxValue, "El stock mínimo no puede ser negativo")]` |

#### `UpdateProductDto`

| Propiedad | C# | TypeScript | Obligatorio | Validaciones |
|---|---:|---:|---:|---|
| `LinePresentationId` | `int` | `number` | Si | `[Required]` |
| `FlavorId` | `int` | `number` | Si | `[Required]` |
| `ProductName` | `string` | `string` | Si | `[Required]`, `[StringLength(150)]` |
| `IsActive` | `bool?` | `boolean \| null` | No | Ninguna |
| `ImageUrl` | `string?` | `string \| null` | No | `[Url]` |
| `MinimumStock` | `int` | `number` | No tecnico, default `0` si se omite | `[Range(0, int.MaxValue)]` |

#### `PatchProductDto`

| Propiedad | C# | TypeScript | Obligatorio | Validaciones |
|---|---:|---:|---:|---|
| `LinePresentationId` | `int?` | `number \| null` | No | Ninguna |
| `FlavorId` | `int?` | `number \| null` | No | Ninguna |
| `ProductName` | `string?` | `string \| null` | No | `[StringLength(150)]` |
| `IsActive` | `bool?` | `boolean \| null` | No | Ninguna |
| `ImageUrl` | `string?` | `string \| null` | No | `[Url]` |
| `MinimumStock` | `int?` | `number \| null` | No | `[Range(0, int.MaxValue)]` |

#### `ProductDto`

| Propiedad | C# | TypeScript | Nullable |
|---|---:|---:|---:|
| `Id` | `int` | `number` | No |
| `LinePresentationId` | `int` | `number` | No |
| `FlavorId` | `int` | `number` | No |
| `ProductName` | `string` | `string` | No |
| `IsActive` | `bool?` | `boolean \| null` | Si |
| `CreatedBy` | `int` | `number` | No |
| `CreatedAt` | `DateTime?` | `string \| null` | Si |
| `ImageUrl` | `string?` | `string \| null` | Si |
| `MinimumStock` | `int` | `number` | No |

#### `ProductCatalogDto`

| Propiedad | C# | TypeScript | Nullable |
|---|---:|---:|---:|
| `Id` | `int` | `number` | No |
| `ProductName` | `string` | `string` | No |
| `ImageUrl` | `string?` | `string \| null` | Si |
| `IsActive` | `bool?` | `boolean \| null` | Si |
| `LineName` | `string` | `string` | No |
| `FlavorName` | `string` | `string` | No |
| `PresentationName` | `string` | `string` | No |
| `WholesalePrice` | `decimal?` | `number \| null` | Si |
| `RetailPrice` | `decimal?` | `number \| null` | Si |

### 4.2 Inventario

#### `InventoryProductDto`

| Propiedad | C# | TypeScript | Nullable | Origen |
|---|---:|---:|---:|---|
| `ProductId` | `int` | `number` | No | `Product.Id` |
| `ProductName` | `string` | `string` | No | `Product.ProductName` |
| `ImageUrl` | `string?` | `string \| null` | Si | `Product.ImageUrl`. URL de imagen del producto. |
| `LineName` | `string` | `string` | No | `Product.LinePresentation.Line.LineName` |
| `PresentationName` | `string` | `string` | No | `Product.LinePresentation.Presentation.PresentationName` |
| `FlavorName` | `string` | `string` | No | `Product.Flavor.FlavorName` |
| `DisplayStock` | `int` | `number` | No | Suma `BatchLocations.CurrentStock` con `LocationId == 2` |
| `WarehouseStock` | `int` | `number` | No | Suma `BatchLocations.CurrentStock` con `LocationId == 1` |
| `ReservedStock` | `int` | `number` | No | Suma `BatchLocations.CurrentStock` con `LocationId == 3` |
| `TotalStock` | `int` | `number` | No | Suma todo `BatchLocations.CurrentStock` |
| `RetailPrice` | `decimal?` | `number \| null` | Si | Precio general activo `PriceTypeId == 1`, `ProductId == null`, vigente |
| `WholesalePrice` | `decimal?` | `number \| null` | Si | Precio general activo `PriceTypeId == 2`, `ProductId == null`, vigente |

#### `InventoryStatsDto`

Respuesta del endpoint de estadisticas para cards superiores del modulo de inventario.

| Propiedad | C# | TypeScript | Nullable | Descripcion |
|---|---:|---:|---:|---|
| `TotalProducts` | `int` | `number` | No | Cantidad de productos activos (`Product.IsActive == true`). |
| `LowStockProducts` | `int` | `number` | No | Productos activos cuyo stock disponible es menor o igual a `MinimumStock`. |
| `BestSellingFlavor` | `BestSellingFlavorDto?` | `BestSellingFlavorDto \| null` | Si | Sabor mas vendido del periodo solicitado. Puede ser `null` si no hay ventas en el periodo. |
| `InventoryValue` | `decimal` | `number` | No | Valor economico actual del stock existente. |

#### `BestSellingFlavorDto`

| Propiedad | C# | TypeScript | Nullable | Descripcion |
|---|---:|---:|---:|---|
| `FlavorId` | `int` | `number` | No | Id del sabor. |
| `FlavorName` | `string` | `string` | No | Nombre del sabor. |
| `QuantitySold` | `int` | `number` | No | Cantidad vendida en el periodo. |
| `Period` | `string` | `string` | No | Periodo normalizado usado para el calculo. |

Reglas de calculo:

- `TotalProducts`: `Products.Count(p => p.IsActive == true)`.
- `LowStockProducts`: compara `Product.MinimumStock` contra stock disponible calculado desde lotes activos (`BatchStatusId == 1`) y ubicaciones vendibles (`LocationId == 1` Bodega + `LocationId == 2` Mostrador). No incluye reservado.
- `BestSellingFlavor`: usa `MovementDetails` vinculados a `InventoryMovement.SaleId != null`, con `DestinationLocationId == null`, agrupados por `Batch.Product.FlavorId`.
- `InventoryValue`: suma `TotalCurrentStockPorLote * UnitProductionCost` en lotes activos. `TotalCurrentStockPorLote` incluye Mostrador + Bodega + Reservado.

#### `InventoryProductBatchesDto`

Respuesta del endpoint de lotes disponibles por producto.

| Propiedad | C# | TypeScript | Nullable | Descripcion |
|---|---:|---:|---:|---|
| `ProductId` | `int` | `number` | No | Id del producto consultado. |
| `ProductName` | `string` | `string` | No | Nombre del producto. |
| `ActiveBatchCount` | `int` | `number` | No | Cantidad de lotes activos/devueltos. |
| `Batches` | `List<InventoryProductBatchDto>` | `InventoryProductBatchDto[]` | No | Lotes activos con stock actual mayor a 0. |

#### `InventoryProductBatchDto`

| Propiedad | C# | TypeScript | Nullable | Regla / Origen |
|---|---:|---:|---:|---|
| `BatchId` | `int` | `number` | No | `Batch.Id`. |
| `BatchCode` | `string?` | `string \| null` | Si | `Batch.BatchCode`. |
| `BatchStatusName` | `string` | `string` | No | `Batch.BatchStatus.BatchStatusName`. |
| `EntryDate` | `DateTime?` | `string \| null` | Si | `Batch.Restock.RestockDate`. |
| `ExpirationDate` | `DateOnly` | `string` | No | `Batch.ExpirationDate`, formato `YYYY-MM-DD`. |
| `StockDisplay` | `int` | `number` | No | Stock en Mostrador: `LocationId == 2`. |
| `StockWarehouse` | `int` | `number` | No | Stock en Bodega: `LocationId == 1`. |
| `StockReserved` | `int` | `number` | No | Stock en Reservado: `LocationId == 3`. |
| `TotalCurrentStock` | `int` | `number` | No | `StockDisplay + StockWarehouse + StockReserved`. |
| `AvailableForSale` | `int` | `number` | No | `StockDisplay + StockWarehouse`. No incluye reservado. |

#### `InventoryBatchDetailDto`

Respuesta del endpoint de detalle completo de lote.

| Propiedad | C# | TypeScript | Nullable | Regla / Origen |
|---|---:|---:|---:|---|
| `BatchId` | `int` | `number` | No | `Batch.Id`. |
| `BatchCode` | `string?` | `string \| null` | Si | `Batch.BatchCode`. |
| `ProductId` | `int` | `number` | No | `Batch.ProductId`. |
| `RestockId` | `int` | `number` | No | `Batch.RestockId`. |
| `BatchStatusName` | `string` | `string` | No | `Batch.BatchStatus.BatchStatusName`. |
| `EntryDate` | `DateTime?` | `string \| null` | Si | `Batch.Restock.RestockDate`. |
| `ExpirationDate` | `DateOnly` | `string` | No | `Batch.ExpirationDate`. |
| `InitialQuantity` | `int` | `number` | No | `Batch.InitialQuantity`. |
| `UnitProductionCost` | `decimal` | `number` | No | `Batch.UnitProductionCost`. |
| `EstimatedTotalCost` | `decimal` | `number` | No | `InitialQuantity * UnitProductionCost`. |
| `StockDisplay` | `int` | `number` | No | Stock actual en Mostrador: `LocationId == 2`. |
| `StockWarehouse` | `int` | `number` | No | Stock actual en Bodega: `LocationId == 1`. |
| `StockReserved` | `int` | `number` | No | Stock actual en Reservado: `LocationId == 3`. |
| `TotalCurrentStock` | `int` | `number` | No | `StockDisplay + StockWarehouse + StockReserved`. |
| `AvailableForSale` | `int` | `number` | No | `StockDisplay + StockWarehouse`. No incluye reservado. |
| `SoldDetail` | `int` | `number` | No | Suma `MovementDetails.Quantity` del lote donde `Movement.SaleId != null`, salida del sistema (`DestinationLocationId == null`) y `Sale.SaleTypeId == 1`. |
| `SoldWholesale` | `int` | `number` | No | Suma `MovementDetails.Quantity` del lote donde `Movement.SaleId != null`, salida del sistema (`DestinationLocationId == null`) y `Sale.SaleTypeId == 2`. |
| `TotalSold` | `int` | `number` | No | `InitialQuantity - TotalCurrentStock`. |

Reglas importantes:

- Si el lote no tiene `BatchLocation` en una ubicacion, esa ubicacion devuelve `0`.
- `AvailableForSale` nunca suma stock reservado.
- `SoldDetail` y `SoldWholesale` dependen de movimientos de inventario vinculados a ventas. Si una venta no genera `InventoryMovement`/`MovementDetail`, no aparecera en estos totales.

### 4.3 Movimientos de inventario

#### `TransferDetailDto`

| Propiedad | C# | TypeScript | Obligatorio | Validaciones |
|---|---:|---:|---:|---|
| `BatchId` | `int` | `number` | Si | `[Required]` |
| `SourceLocationId` | `int` | `number` | Si | `[Required]` |
| `DestinationLocationId` | `int` | `number` | Si | `[Required]` |
| `Quantity` | `int` | `number` | Si | `[Required, Range(1, int.MaxValue)]` |

#### `CreateTransferDto`

| Propiedad | C# | TypeScript | Obligatorio | Validaciones |
|---|---:|---:|---:|---|
| `Notes` | `string?` | `string \| null` | No | Ninguna |
| `CreatedBy` | `int` | `number` | Si | `[Required]` |
| `Details` | `List<TransferDetailDto>` | `TransferDetailDto[]` | Si | `[Required, MinLength(1)]` |

Reglas de servicio:

- `SourceLocationId` y `DestinationLocationId` no pueden ser iguales.
- El lote debe existir.
- El lote debe tener stock en la ubicacion origen.
- El stock origen debe ser suficiente.
- Si no existe registro destino `(BatchId, DestinationLocationId)`, se crea con stock `0`.
- Resta en origen, suma en destino.
- Crea `InventoryMovement` con `MovementTypeId = 1002`.

#### `AdjustmentDetailDto`

| Propiedad | C# | TypeScript | Obligatorio | Validaciones |
|---|---:|---:|---:|---|
| `BatchId` | `int` | `number` | Si | `[Required]` |
| `LocationId` | `int` | `number` | Si | `[Required]` |
| `Quantity` | `int` | `number` | Si | `[Required, Range(1, int.MaxValue)]` |

#### `CreatePositiveAdjustmentDto`

| Propiedad | C# | TypeScript | Obligatorio | Validaciones |
|---|---:|---:|---:|---|
| `Notes` | `string?` | `string \| null` | No | Ninguna |
| `CreatedBy` | `int` | `number` | Si | `[Required]` |
| `Details` | `List<AdjustmentDetailDto>` | `AdjustmentDetailDto[]` | Si | `[Required, MinLength(1)]` |

Reglas: lote debe existir; si no existe `BatchLocation`, se crea; suma stock; `MovementTypeId = 1003`.

#### `CreateNegativeAdjustmentDto`

Misma estructura que `CreatePositiveAdjustmentDto`.

Reglas: lote debe existir; debe existir `BatchLocation`; stock debe ser suficiente; resta stock; `MovementTypeId = 1004`.

#### `InventoryMovementResultDto`

| Propiedad | C# | TypeScript | Nullable |
|---|---:|---:|---:|
| `Id` | `int` | `number` | No |
| `MovementTypeId` | `int` | `number` | No |
| `MovementDate` | `DateTime?` | `string \| null` | Si |
| `Notes` | `string?` | `string \| null` | Si |
| `CreatedBy` | `int` | `number` | No |
| `Details` | `List<MovementDetailResultDto>` | `MovementDetailResultDto[]` | No |

#### `MovementDetailResultDto`

| Propiedad | C# | TypeScript | Nullable |
|---|---:|---:|---:|
| `Id` | `int` | `number` | No |
| `BatchId` | `int` | `number` | No |
| `SourceLocationId` | `int?` | `number \| null` | Si |
| `DestinationLocationId` | `int?` | `number \| null` | Si |
| `Quantity` | `int` | `number` | No |
| `UnitCost` | `decimal` | `number` | No |

### 4.4 Reabastecimientos

#### `CreateRestockBatchDto`

| Propiedad | C# | TypeScript | Obligatorio | Validaciones |
|---|---:|---:|---:|---|
| `ProductId` | `int` | `number` | Si | `[Required("El ProductId es obligatorio")]` |
| `Quantity` | `int` | `number` | Si | `[Required]`, `[Range(1, int.MaxValue, "La cantidad debe ser mayor a 0")]` |
| `UnitProductionCost` | `decimal` | `number` | Si | `[Required]`, `[Range(0.01, double.MaxValue, "El costo unitario debe ser mayor a 0")]` |
| `ExpirationDate` | `DateOnly` | `string` | Si | `[Required("La fecha de vencimiento es obligatoria")]`; servicio exige fecha futura |

#### `CreateRestockDto`

| Propiedad | C# | TypeScript | Obligatorio | Validaciones |
|---|---:|---:|---:|---|
| `CreatedBy` | `int` | `number` | Si | `[Required("El CreatedBy es obligatorio")]` |
| `Notes` | `string?` | `string \| null` | No | Ninguna |
| `Batches` | `List<CreateRestockBatchDto>` | `CreateRestockBatchDto[]` | Si | `[Required]`, `[MinLength(1, "Debe incluir al menos un lote")]` |

Reglas de servicio:

- Cada producto debe existir.
- `ExpirationDate` debe ser mayor a la fecha UTC actual.
- Crea `InventoryMovement` tipo `1`.
- Crea `Restock`.
- Por cada lote, crea `Batch`, crea `BatchLocation` en `LocationId = 1`, y crea `MovementDetail` con destino `1`.

#### `RestockResponseDto`

| Propiedad | C# | TypeScript | Nullable |
|---|---:|---:|---:|
| `RestockId` | `int` | `number` | No |
| `RestockCode` | `string` | `string` | No |
| `InventoryMovementId` | `int` | `number` | No |
| `RestockDate` | `DateTime` | `string` | No |
| `Batches` | `List<RestockBatchResponseDto>` | `RestockBatchResponseDto[]` | No |

#### `RestockBatchResponseDto`

| Propiedad | C# | TypeScript | Nullable |
|---|---:|---:|---:|
| `BatchId` | `int` | `number` | No |
| `BatchCode` | `string` | `string` | No |
| `ProductName` | `string` | `string` | No |
| `Quantity` | `int` | `number` | No |
| `UnitProductionCost` | `decimal` | `number` | No |
| `ExpirationDate` | `DateOnly` | `string` | No |

### 4.5 Catalogos auxiliares

#### `LineDto`, `CreateLineDto`, `UpdateLineDto`

| DTO | Propiedad | C# | TypeScript | Obligatorio | Validaciones |
|---|---|---:|---:|---:|---|
| `LineDto` | `Id` | `int` | `number` | No aplica | Ninguna |
| `LineDto` | `LineName` | `string` | `string` | No aplica | Ninguna |
| `LineDto` | `IsActive` | `bool?` | `boolean \| null` | No aplica | Ninguna |
| `CreateLineDto` | `LineName` | `string` | `string` | Si | `[Required]`, `[StringLength(100)]` |
| `UpdateLineDto` | `LineName` | `string` | `string` | Si | `[Required]`, `[StringLength(100)]` |
| `UpdateLineDto` | `IsActive` | `bool?` | `boolean \| null` | No | Ninguna |

#### `FlavorDto`, `CreateFlavorDto`, `UpdateFlavorDto`

| DTO | Propiedad | C# | TypeScript | Obligatorio | Validaciones |
|---|---|---:|---:|---:|---|
| `FlavorDto` | `Id` | `int` | `number` | No aplica | Ninguna |
| `FlavorDto` | `FlavorName` | `string` | `string` | No aplica | Ninguna |
| `FlavorDto` | `IsActive` | `bool?` | `boolean \| null` | No aplica | Ninguna |
| `FlavorDto` | `ImageUrl` | `string?` | `string \| null` | No aplica | Ninguna |
| `FlavorDto` | `FlavorColor` | `string?` | `string \| null` | No aplica | Ninguna |
| `CreateFlavorDto` | `FlavorName` | `string` | `string` | Si | `[Required]`, `[StringLength(100)]` |
| `CreateFlavorDto` | `ImageUrl` | `string?` | `string \| null` | No | `[Url]` |
| `CreateFlavorDto` | `FlavorColor` | `string?` | `string \| null` | No | `[StringLength(7)]`; no regex en create |
| `UpdateFlavorDto` | `FlavorName` | `string` | `string` | Si | `[Required]`, `[StringLength(100)]` |
| `UpdateFlavorDto` | `IsActive` | `bool?` | `boolean \| null` | No | Ninguna |
| `UpdateFlavorDto` | `ImageUrl` | `string?` | `string \| null` | No | `[Url]` |
| `UpdateFlavorDto` | `FlavorColor` | `string?` | `string \| null` | No | `[RegularExpression("^#([A-Fa-f0-9]{6})$")]` |

#### `PresentationDto`, `CreatePresentationDto`, `UpdatePresentationDto`

| DTO | Propiedad | C# | TypeScript | Obligatorio | Validaciones |
|---|---|---:|---:|---:|---|
| `PresentationDto` | `Id` | `int` | `number` | No aplica | Ninguna |
| `PresentationDto` | `PresentationName` | `string` | `string` | No aplica | Ninguna |
| `PresentationDto` | `IsActive` | `bool?` | `boolean \| null` | No aplica | Ninguna |
| `CreatePresentationDto` | `PresentationName` | `string` | `string` | Si | `[Required]`, `[StringLength(100)]` |
| `UpdatePresentationDto` | `PresentationName` | `string` | `string` | Si | `[Required]`, `[StringLength(100)]` |
| `UpdatePresentationDto` | `IsActive` | `bool?` | `boolean \| null` | No | Ninguna |

#### `LinePresentationDto`

| DTO | Propiedad | C# | TypeScript | Obligatorio | Validaciones |
|---|---|---:|---:|---:|---|
| `CreateLinePresentationDto` | `LineId` | `int` | `number` | Si | `[Required("La línea es obligatoria")]` |
| `CreateLinePresentationDto` | `PresentationId` | `int` | `number` | Si | `[Required("La presentación es obligatoria")]` |
| `LinePresentationDto` | `Id` | `int` | `number` | No aplica | Ninguna |
| `LinePresentationDto` | `Line` | `LineReferenceDto` | `{ id: number; name: string }` | No aplica | Ninguna |
| `LinePresentationDto` | `Presentation` | `PresentationReferenceDto` | `{ id: number; name: string }` | No aplica | Ninguna |

### 4.6 Subida de imagen

#### `ProductImageUploadResponseDto`

| Propiedad | C# | TypeScript | Nullable |
|---|---:|---:|---:|
| `ImageUrl` | `string` | `string` | No |

Reglas de `ProductImageService`:

- Campo multipart esperado: `file`.
- Extensiones permitidas: `.jpg`, `.jpeg`, `.png`, `.webp`.
- Rechaza archivo nulo.
- Rechaza `fileLength <= 0`.
- Guarda en `wwwroot/uploads/products`.
- Nombre almacenado: `Guid` + extension en minuscula.
- Retorna URL relativa: `/uploads/products/{guid}.{ext}`.

## 5. Endpoints

Todos los endpoints estan bajo `api/[controller]`. A menos que se indique lo contrario:

- No tienen `[Authorize]`.
- No exigen rol.
- Aceptan JWT Bearer si el cliente lo envia, pero no es requerido por atributo.
- Los cuerpos JSON no tienen `[FromBody]` explicito; con `[ApiController]` ASP.NET Core infiere body para DTOs complejos.

### 5.1 Inventario

#### `GET /api/Inventory/stats`

Devuelve las estadisticas necesarias para las cards superiores del modulo de inventario.

Autenticacion: no requerida actualmente. Token opcional `Bearer JWT`. Roles: ninguno.

Parametros:

| Nombre | Ubicacion | C# | Requerido | Default | Valores soportados |
|---|---|---:|---:|---:|---|
| `period` | Query | `string` | No | `week` | `day`, `week`, `month`, `year`, `all`. Si llega otro valor, se normaliza a `week`. |

Respuesta `200 OK`:

```ts
ApiResponse<InventoryStatsDto>
```

Mensaje exitoso:

```txt
Estadísticas de inventario obtenidas exitosamente
```

Ejemplo:

```http
GET /api/Inventory/stats
GET /api/Inventory/stats?period=month
```

Ejemplo de respuesta:

```json
{
  "success": true,
  "message": "Estadísticas de inventario obtenidas exitosamente",
  "data": {
    "totalProducts": 35,
    "lowStockProducts": 4,
    "bestSellingFlavor": {
      "flavorId": 2,
      "flavorName": "Fresa",
      "quantitySold": 120,
      "period": "week"
    },
    "inventoryValue": 18500.50
  }
}
```

Notas:

- Si no hay ventas en el periodo, `bestSellingFlavor` puede venir como `null`.
- El periodo `week` usa los ultimos 7 dias desde `DateTime.UtcNow`.
- `all` no aplica filtro por fecha.

#### `GET /api/Inventory`

Listado paginado de inventario por producto activo.

Autenticacion: no requerida actualmente. Token opcional `Bearer JWT`. Roles: ninguno.

Parametros:

| Nombre | Ubicacion | C# | Requerido | Descripcion |
|---|---|---:|---:|---|
| `search` | Query | `string?` | No | Filtra por `ProductName.Contains(search)`. |
| `lineId` | Query | `int?` | No | Filtra por `Product.LinePresentation.LineId`. |
| `flavorId` | Query | `int?` | No | Filtra por `Product.FlavorId`. |
| `presentationId` | Query | `int?` | No | Filtra por `Product.LinePresentation.PresentationId`. |
| `page` / `Page` | Query | `int` | No | Default `1`. |
| `pageSize` / `PageSize` | Query | `int` | No | Default `10`, max `50`. |

Respuesta `200 OK`:

```ts
ApiResponse<PagedResponse<InventoryProductDto>>
```

Notas de implementacion:

- Solo incluye productos con `IsActive == true`.
- Ordena por `ProductName`.
- Stock por ubicacion se calcula con subconsultas sobre `Batches -> BatchLocations`.
- Precios son generales de `LinePresentation`, no precios especificos por producto, porque exige `ProductId == null`.

Ejemplo:

```http
GET /api/Inventory?page=1&pageSize=10&search=chocolate&lineId=2
```

#### `GET /api/Inventory/products`

Listado no paginado de inventario por producto activo. Se mantiene para compatibilidad.

Autenticacion: no requerida actualmente. Token opcional `Bearer JWT`. Roles: ninguno.

Parametros:

| Nombre | Ubicacion | C# | Requerido |
|---|---|---:|---:|
| `search` | Query | `string?` | No |
| `lineId` | Query | `int?` | No |
| `flavorId` | Query | `int?` | No |
| `presentationId` | Query | `int?` | No |

Respuesta `200 OK`:

```ts
ApiResponse<InventoryProductDto[]>
```

#### `GET /api/Inventory/products/{productId}/batches`

Devuelve una lista resumida de lotes activos/disponibles de un producto para mostrar una tabla o lista en el frontend.

Autenticacion: no requerida actualmente. Token opcional `Bearer JWT`. Roles: ninguno.

Parametros:

| Nombre | Ubicacion | C# | Requerido | Descripcion |
|---|---|---:|---:|---|
| `productId` | Route | `int` | Si | Id del producto. |

Respuesta `200 OK`:

```ts
ApiResponse<InventoryProductBatchesDto>
```

Errores:

- `404 Not Found` si el producto no existe: `ApiResponse<InventoryProductBatchesDto>.Fail("Producto con Id {productId} no encontrado")`.

Reglas:

- Solo incluye lotes con `BatchStatusId == 1`.
- Solo incluye lotes con `TotalCurrentStock > 0`.
- Ordena por `EntryDate` y luego `BatchId`.
- `StockDisplay` usa `LocationId == 2`.
- `StockWarehouse` usa `LocationId == 1`.
- `StockReserved` usa `LocationId == 3`.
- `AvailableForSale = StockDisplay + StockWarehouse`.

Ejemplo:

```http
GET /api/Inventory/products/10/batches
```

#### `GET /api/Inventory/batches/{batchId}/detail`

Devuelve el detalle completo de un lote especifico para abrir un modal/drawer.

Autenticacion: no requerida actualmente. Token opcional `Bearer JWT`. Roles: ninguno.

Parametros:

| Nombre | Ubicacion | C# | Requerido | Descripcion |
|---|---|---:|---:|---|
| `batchId` | Route | `int` | Si | Id del lote. |

Respuesta `200 OK`:

```ts
ApiResponse<InventoryBatchDetailDto>
```

Errores:

- `404 Not Found` si el lote no existe: `ApiResponse<InventoryBatchDetailDto>.Fail("Lote con Id {batchId} no encontrado")`.

Reglas:

- `EstimatedTotalCost = InitialQuantity * UnitProductionCost`.
- `TotalCurrentStock = StockDisplay + StockWarehouse + StockReserved`.
- `AvailableForSale = StockDisplay + StockWarehouse`.
- `TotalSold = InitialQuantity - TotalCurrentStock`.
- `SoldDetail` se calcula con `MovementDetails` vinculados a una venta con `SaleTypeId == 1`.
- `SoldWholesale` se calcula con `MovementDetails` vinculados a una venta con `SaleTypeId == 2`.
- En ambos vendidos se exige `DestinationLocationId == null`, porque representa salida del sistema.

Ejemplo:

```http
GET /api/Inventory/batches/25/detail
```

### 5.2 Productos CRUD

#### `GET /api/Products`

Listado paginado de productos basico.

Autenticacion: no requerida actualmente. Token opcional `Bearer JWT`. Roles: ninguno.

Parametros:

| Nombre | Ubicacion | C# | Requerido |
|---|---|---:|---:|
| `page` / `Page` | Query | `int` | No |
| `pageSize` / `PageSize` | Query | `int` | No |

Respuesta:

```ts
ApiResponse<PagedResponse<ProductDto>>
```

Orden: `ProductName`.

#### `GET /api/Products/catalog`

Listado paginado para catalogo visual/gestion de productos.

Autenticacion: no requerida actualmente. Token opcional `Bearer JWT`. Roles: ninguno.

Parametros:

| Nombre | Ubicacion | C# | Requerido | Descripcion |
|---|---|---:|---:|---|
| `lineId` | Query | `int?` | No | Filtra por linea. |
| `flavorId` | Query | `int?` | No | Filtra por sabor. |
| `search` | Query | `string?` | No | Filtra por nombre de producto. |
| `active` | Query | `bool?` | No | Filtra `IsActive`. |
| `page` / `Page` | Query | `int` | No | Paginacion. |
| `pageSize` / `PageSize` | Query | `int` | No | Paginacion. |

Respuesta:

```ts
ApiResponse<PagedResponse<ProductCatalogDto>>
```

Notas:

- Precios se buscan en `ProductPrices` por `LinePresentationId`, `PriceTypeId`, `IsActive == true` y vigencia.
- Toma el precio mas reciente por `ValidFrom DESC`.

#### `GET /api/Products/{id}`

Autenticacion: no requerida actualmente. Token opcional `Bearer JWT`. Roles: ninguno.

Parametros:

| Nombre | Ubicacion | C# | Requerido |
|---|---|---:|---:|
| `id` | Route | `int` | Si |

Respuesta exitosa:

```ts
ApiResponse<ProductDto>
```

Errores:

- `404 Not Found` si no existe: `ApiResponse<ProductDto>.Fail("Producto con Id {id} no encontrado")`.

#### `POST /api/Products`

Crea producto.

Autenticacion: no requerida actualmente. Token opcional `Bearer JWT`. Roles: ninguno.

Body:

```ts
CreateProductDto
```

Respuesta exitosa:

- HTTP `201 Created`.
- Header/location generado por `CreatedAtAction(nameof(GetById), new { id = result.Data!.Id })`.
- Body:

```ts
ApiResponse<ProductDto>
```

Mensaje: `Producto creado exitosamente`.

Reglas de negocio:

- `FlavorId` debe existir.
- `LinePresentationId` debe existir.
- No puede existir otro producto con mismo `ProductName`, `LinePresentationId` y `FlavorId`.
- El repositorio fuerza `IsActive = true`.
- El repositorio asigna `CreatedAt = DateTime.UtcNow`.

Errores:

- `400 Bad Request` por validaciones Data Annotation.
- `400 Bad Request` si falla regla de negocio.

Ejemplo body:

```json
{
  "linePresentationId": 1,
  "flavorId": 2,
  "productName": "Helado Chocolate 1L",
  "createdBy": 1,
  "imageUrl": "/uploads/products/archivo.png",
  "minimumStock": 10
}
```

#### `PUT /api/Products/{id}`

Actualiza producto completo.

Autenticacion: no requerida actualmente. Token opcional `Bearer JWT`. Roles: ninguno.

Parametros:

| Nombre | Ubicacion | C# | Requerido |
|---|---|---:|---:|
| `id` | Route | `int` | Si |

Body:

```ts
UpdateProductDto
```

Respuesta exitosa:

```ts
ApiResponse<object>
```

Mensaje: `Producto actualizado exitosamente`.

Reglas:

- Producto debe existir.
- `FlavorId` debe existir.
- `LinePresentationId` debe existir.
- No puede duplicar `ProductName + LinePresentationId + FlavorId` contra otro id.
- `IsActive` puede enviarse `true`, `false` o `null`. El repositorio asigna exactamente `dto.IsActive`.

Errores:

- `404 Not Found` si el mensaje contiene `no encontrado`.
- `400 Bad Request` para otras reglas de negocio o validacion.

#### `PATCH /api/Products/{id}`

Actualizacion parcial.

Autenticacion: no requerida actualmente. Token opcional `Bearer JWT`. Roles: ninguno.

Parametros:

| Nombre | Ubicacion | C# | Requerido |
|---|---|---:|---:|
| `id` | Route | `int` | Si |

Body:

```ts
PatchProductDto
```

Reglas:

- Producto debe existir.
- Si se envia `FlavorId`, debe existir.
- Si se envia `LinePresentationId`, debe existir.
- Si se envia `ProductName`, valida duplicado usando los nuevos valores enviados o los actuales para campos omitidos.
- Solo modifica propiedades presentes/no nulas. Para `ImageUrl`, si se envia `null`, no borra la imagen porque el repositorio solo asigna si `dto.ImageUrl is not null`.
- Para `IsActive`, solo modifica si `HasValue`.

Respuesta exitosa:

```ts
ApiResponse<object>
```

Mensaje: `Producto actualizado exitosamente`.

#### `DELETE /api/Products/{id}`

Elimina fisicamente el producto.

Autenticacion: no requerida actualmente. Token opcional `Bearer JWT`. Roles: ninguno.

Parametros:

| Nombre | Ubicacion | C# | Requerido |
|---|---|---:|---:|
| `id` | Route | `int` | Si |

Reglas:

- Producto debe existir.
- No se puede eliminar si tiene lotes registrados (`Batches.Any(b => b.ProductId == id)`).
- No se puede eliminar si tiene precios activos especificos de producto (`ProductPrices.Any(pp => pp.ProductId == id && pp.IsActive == true)`).
- Si pasa validaciones, se usa `_context.Products.Remove(product)`.

Respuesta exitosa:

```ts
ApiResponse<object>
```

Mensaje: `Producto eliminado exitosamente`.

Errores:

- `404 Not Found` si no existe.
- `400 Bad Request` si tiene lotes o precios activos.

### 5.3 Subida de imagen de producto

#### `POST /api/Uploads/products`

Autenticacion: no requerida actualmente. Token opcional `Bearer JWT`. Roles: ninguno.

Content-Type:

```http
multipart/form-data
```

Parametros:

| Nombre | Ubicacion | C# | Requerido | Reglas |
|---|---|---:|---:|---|
| `file` | Form-data | `IFormFile?` | Si | `.jpg`, `.jpeg`, `.png`, `.webp`; longitud > 0 |

Respuesta exitosa:

```ts
ApiResponse<ProductImageUploadResponseDto>
```

Mensaje: `Imagen subida correctamente`.

Ejemplo TypeScript:

```ts
const form = new FormData();
form.append("file", file);
await fetch("/api/Uploads/products", { method: "POST", body: form });
```

### 5.4 Reabastecimientos

#### `POST /api/Restocks`

Crea entrada de inventario, restock, lotes, ubicaciones iniciales y detalles de movimiento.

Autenticacion: no requerida actualmente. Token opcional `Bearer JWT`. Roles: ninguno.

Body:

```ts
CreateRestockDto
```

Respuesta exitosa:

- HTTP `201 Created`.
- Body:

```ts
ApiResponse<RestockResponseDto>
```

Mensaje: `Restock creado exitosamente`.

Errores:

- `400 Bad Request` si no incluye lotes.
- `400 Bad Request` si un producto no existe.
- `400 Bad Request` si una fecha de vencimiento no es futura.
- `400 Bad Request` por Data Annotations.

Ejemplo:

```json
{
  "createdBy": 1,
  "notes": "Produccion inicial",
  "batches": [
    {
      "productId": 10,
      "quantity": 50,
      "unitProductionCost": 12.5,
      "expirationDate": "2026-12-31"
    }
  ]
}
```

### 5.5 Movimientos de inventario

#### `POST /api/InventoryMovements/transfer`

Autenticacion: no requerida actualmente. Token opcional `Bearer JWT`. Roles: ninguno.

Body:

```ts
CreateTransferDto
```

Respuesta exitosa:

- HTTP `201 Created`.
- Body:

```ts
ApiResponse<InventoryMovementResultDto>
```

Mensaje: `Transferencia registrada exitosamente`.

Errores:

- `400 Bad Request` por origen igual a destino.
- `400 Bad Request` por lote inexistente.
- `400 Bad Request` si el lote no tiene stock en origen.
- `400 Bad Request` si stock insuficiente.

#### `POST /api/InventoryMovements/positive-adjustment`

Autenticacion: no requerida actualmente. Token opcional `Bearer JWT`. Roles: ninguno.

Body:

```ts
CreatePositiveAdjustmentDto
```

Respuesta exitosa:

```ts
ApiResponse<InventoryMovementResultDto>
```

Mensaje: `Ajuste positivo registrado exitosamente`.

#### `POST /api/InventoryMovements/negative-adjustment`

Autenticacion: no requerida actualmente. Token opcional `Bearer JWT`. Roles: ninguno.

Body:

```ts
CreateNegativeAdjustmentDto
```

Respuesta exitosa:

```ts
ApiResponse<InventoryMovementResultDto>
```

Mensaje: `Ajuste negativo registrado exitosamente`.

Errores adicionales:

- El lote debe tener stock en la ubicacion.
- Stock suficiente.

### 5.6 Precios generales

#### `GET /api/GeneralPrices/general`

Retorna precios generales agrupados por `LinePresentation` para vistas de gestion.

Autenticacion: no requerida actualmente. Token opcional `Bearer JWT`. Roles: ninguno.

Parametros:

| Nombre | Ubicacion | C# | Requerido |
|---|---|---:|---:|
| `lineId` | Query | `int?` | No |

Respuesta:

```ts
ApiResponse<GeneralPriceDto[]>
```

`GeneralPriceDto`:

| Propiedad | C# | TypeScript | Nullable |
|---|---:|---:|---:|
| `LinePresentationId` | `int` | `number` | No |
| `LineName` | `string` | `string` | No |
| `PresentationName` | `string` | `string` | No |
| `RetailPrice` | `decimal?` | `number \| null` | Si |
| `WholesalePrice` | `decimal?` | `number \| null` | Si |
| `ProductsCount` | `int` | `number` | No |

### 5.7 Catalogos auxiliares para formularios

#### Lines

| Metodo | Ruta | Body | Respuesta |
|---|---|---|---|
| `GET` | `/api/Lines` | No | `ApiResponse<LineDto[]>` |
| `GET` | `/api/Lines/{id}` | No | `ApiResponse<LineDto>` |
| `POST` | `/api/Lines` | `CreateLineDto` | `201 ApiResponse<LineDto>` |
| `PUT` | `/api/Lines/{id}` | `UpdateLineDto` | `ApiResponse<object>` |
| `DELETE` | `/api/Lines/{id}` | No | `ApiResponse<object>` |
| `GET` | `/api/Lines/{lineId}/presentations` | No | `ApiResponse<PresentationDto[]>` |

Nota tecnica: `LinesController` instancia manualmente `LinePresentationRepository(new HerreraSystemContext())` en vez de usar DI para `_linePresentationRepository`; esto puede fallar si se usa el endpoint `/api/Lines/{lineId}/presentations` sin configurar ese contexto. El endpoint recomendado para relaciones es `/api/LinePresentations`.

#### Flavors

| Metodo | Ruta | Body | Query | Respuesta |
|---|---|---|---|---|
| `GET` | `/api/Flavors` | No | `page`, `pageSize` | `ApiResponse<PagedResponse<FlavorDto>>` |
| `GET` | `/api/Flavors/{id}` | No | No | `ApiResponse<FlavorDto>` |
| `POST` | `/api/Flavors` | `CreateFlavorDto` | No | `201 ApiResponse<FlavorDto>` |
| `PUT` | `/api/Flavors/{id}` | `UpdateFlavorDto` | No | `ApiResponse<object>` |
| `DELETE` | `/api/Flavors/{id}` | No | No | `ApiResponse<object>` |

#### Presentations

| Metodo | Ruta | Body | Respuesta |
|---|---|---|---|
| `GET` | `/api/Presentations` | No | `ApiResponse<PresentationDto[]>` |
| `GET` | `/api/Presentations/{id}` | No | `ApiResponse<PresentationDto>` |
| `POST` | `/api/Presentations` | `CreatePresentationDto` | `201 ApiResponse<PresentationDto>` |
| `PUT` | `/api/Presentations/{id}` | `UpdatePresentationDto` | `ApiResponse<object>` |
| `DELETE` | `/api/Presentations/{id}` | No | `ApiResponse<object>` |

#### LinePresentations

| Metodo | Ruta | Body | Respuesta |
|---|---|---|---|
| `GET` | `/api/LinePresentations` | No | `ApiResponse<LinePresentationDto[]>` |
| `GET` | `/api/LinePresentations/{id}` | No | `ApiResponse<LinePresentationDto>` |
| `POST` | `/api/LinePresentations` | `CreateLinePresentationDto` | `201 ApiResponse<LinePresentationDto>` |
| `DELETE` | `/api/LinePresentations/{id}` | No | `ApiResponse<object>` |

Regla de creacion: devuelve `400` si la linea no existe, la presentacion no existe o la combinacion ya existe.

## 6. Contratos TypeScript Sugeridos

```ts
export interface ProductDto {
  id: number;
  linePresentationId: number;
  flavorId: number;
  productName: string;
  isActive: boolean | null;
  createdBy: number;
  createdAt: string | null;
  imageUrl: string | null;
  minimumStock: number;
}

export interface CreateProductDto {
  linePresentationId: number;
  flavorId: number;
  productName: string;
  createdBy: number;
  imageUrl?: string | null;
  minimumStock: number;
}

export interface UpdateProductDto {
  linePresentationId: number;
  flavorId: number;
  productName: string;
  isActive?: boolean | null;
  imageUrl?: string | null;
  minimumStock: number;
}

export interface PatchProductDto {
  linePresentationId?: number | null;
  flavorId?: number | null;
  productName?: string | null;
  isActive?: boolean | null;
  imageUrl?: string | null;
  minimumStock?: number | null;
}

export interface ProductCatalogDto {
  id: number;
  productName: string;
  imageUrl: string | null;
  isActive: boolean | null;
  lineName: string;
  flavorName: string;
  presentationName: string;
  wholesalePrice: number | null;
  retailPrice: number | null;
}

export interface InventoryProductDto {
  productId: number;
  productName: string;
  imageUrl: string | null;
  lineName: string;
  presentationName: string;
  flavorName: string;
  displayStock: number;
  warehouseStock: number;
  reservedStock: number;
  totalStock: number;
  retailPrice: number | null;
  wholesalePrice: number | null;
}

export interface InventoryStatsDto {
  totalProducts: number;
  lowStockProducts: number;
  bestSellingFlavor: BestSellingFlavorDto | null;
  inventoryValue: number;
}

export interface BestSellingFlavorDto {
  flavorId: number;
  flavorName: string;
  quantitySold: number;
  period: string;
}

export interface InventoryProductBatchesDto {
  productId: number;
  productName: string;
  activeBatchCount: number;
  batches: InventoryProductBatchDto[];
}

export interface InventoryProductBatchDto {
  batchId: number;
  batchCode: string | null;
  batchStatusName: string;
  entryDate: string | null;
  expirationDate: string;
  stockDisplay: number;
  stockWarehouse: number;
  stockReserved: number;
  totalCurrentStock: number;
  availableForSale: number;
}

export interface InventoryBatchDetailDto {
  batchId: number;
  batchCode: string | null;
  productId: number;
  restockId: number;
  batchStatusName: string;
  entryDate: string | null;
  expirationDate: string;
  initialQuantity: number;
  unitProductionCost: number;
  estimatedTotalCost: number;
  stockDisplay: number;
  stockWarehouse: number;
  stockReserved: number;
  totalCurrentStock: number;
  availableForSale: number;
  soldDetail: number;
  soldWholesale: number;
  totalSold: number;
}

export interface CreateRestockBatchDto {
  productId: number;
  quantity: number;
  unitProductionCost: number;
  expirationDate: string;
}

export interface CreateRestockDto {
  createdBy: number;
  notes?: string | null;
  batches: CreateRestockBatchDto[];
}

export interface TransferDetailDto {
  batchId: number;
  sourceLocationId: number;
  destinationLocationId: number;
  quantity: number;
}

export interface CreateTransferDto {
  notes?: string | null;
  createdBy: number;
  details: TransferDetailDto[];
}

export interface AdjustmentDetailDto {
  batchId: number;
  locationId: number;
  quantity: number;
}

export interface CreatePositiveAdjustmentDto {
  notes?: string | null;
  createdBy: number;
  details: AdjustmentDetailDto[];
}

export interface CreateNegativeAdjustmentDto {
  notes?: string | null;
  createdBy: number;
  details: AdjustmentDetailDto[];
}
```

## 7. Flujos Front-end Recomendados

### 7.0 Cards superiores de inventario

1. Llamar `GET /api/Inventory/stats` al cargar el modulo.
2. Usar `totalProducts` para la card de productos activos.
3. Usar `lowStockProducts` para la card de alertas de stock bajo.
4. Usar `bestSellingFlavor` para la card de sabor mas vendido. Si viene `null`, mostrar estado vacio.
5. Usar `inventoryValue` para la card de valor actual del inventario.
6. Para cambiar periodo del sabor mas vendido, enviar `period`: `day`, `week`, `month`, `year` o `all`.

### 7.1 Pantalla de listado de inventario

1. Llamar `GET /api/Inventory?page=1&pageSize=10`.
2. Usar `data.data` como filas.
3. Usar `currentPage`, `pageSize`, `totalRecords`, `totalPages`, `hasNextPage`, `hasPreviousPage` para controles de paginacion.
4. Para filtros: enviar `search`, `lineId`, `flavorId`, `presentationId`.
5. Para compatibilidad con clientes antiguos, `GET /api/Inventory/products` devuelve el mismo DTO sin paginacion.
6. Al seleccionar un producto, llamar `GET /api/Inventory/products/{productId}/batches` para mostrar los lotes disponibles.
7. Al seleccionar un lote, llamar `GET /api/Inventory/batches/{batchId}/detail` para abrir el modal/drawer de detalle.

### 7.1.1 Tabla de lotes por producto

Columnas sugeridas:

| Campo | Uso UI |
|---|---|
| `batchCode` | Codigo de lote. |
| `batchStatusName` | Estado. |
| `entryDate` | Fecha de ingreso. |
| `expirationDate` | Fecha de vencimiento. |
| `stockDisplay` | Stock en Mostrador. |
| `stockWarehouse` | Stock en Bodega. |
| `stockReserved` | Stock reservado. |
| `totalCurrentStock` | Stock total actual. |
| `availableForSale` | Disponible real para venta. |

Recordatorio: `availableForSale` no incluye reservado.

### 7.1.2 Modal o drawer de detalle de lote

Usar `GET /api/Inventory/batches/{batchId}/detail`.

Secciones sugeridas:

- Identificacion: `batchCode`, `batchId`, `productId`, `restockId`, `batchStatusName`.
- Fechas: `entryDate`, `expirationDate`.
- Costos: `initialQuantity`, `unitProductionCost`, `estimatedTotalCost`.
- Stock: `stockDisplay`, `stockWarehouse`, `stockReserved`, `totalCurrentStock`, `availableForSale`.
- Ventas: `soldDetail`, `soldWholesale`, `totalSold`.

`soldDetail` se basa en `SaleTypeId == 1`; `soldWholesale` se basa en `SaleTypeId == 2`.

### 7.2 Pantalla de gestion de productos

1. Cargar lista principal con `GET /api/Products/catalog`.
2. Cargar combos:
   - `GET /api/LinePresentations` para obtener `linePresentationId`.
   - `GET /api/Flavors?page=1&pageSize=50` para sabores.
   - Opcional: `GET /api/Lines`, `GET /api/Presentations`.
3. Para imagen:
   - Subir primero con `POST /api/Uploads/products`.
   - Guardar `imageUrl` retornado en `CreateProductDto` o `UpdateProductDto`.
4. Crear con `POST /api/Products`.
5. Editar completo con `PUT /api/Products/{id}`.
6. Editar parcial con `PATCH /api/Products/{id}`.
7. Eliminar con `DELETE /api/Products/{id}`; manejar errores por lotes o precios activos.

### 7.3 Pantalla de entrada de stock

1. Seleccionar productos existentes.
2. Enviar `POST /api/Restocks` con uno o mas lotes.
3. Mostrar `restockCode`, `inventoryMovementId` y lista de `batches`.

### 7.4 Pantalla de movimientos

Transferencia:

- Usar `batchId`, `sourceLocationId`, `destinationLocationId`, `quantity`.
- La API valida stock suficiente.

Ajuste positivo:

- Permite crear registro de stock en ubicacion si no existe.

Ajuste negativo:

- Requiere stock existente y suficiente.

## 8. Codigos de Estado Observados

| Escenario | Codigo | Forma |
|---|---:|---|
| Consulta exitosa | `200 OK` | `ApiResponse<T>` |
| Creacion exitosa | `201 Created` | `ApiResponse<T>` |
| Validacion Data Annotation | `400 Bad Request` | `ApiResponse<List<string>>` |
| Regla de negocio fallida | `400 Bad Request` | `ApiResponse<T>.Fail(message)` |
| Recurso no encontrado | `404 Not Found` | `ApiResponse<T>.Fail(message)` |
| Excepcion no controlada | Depende de `ExceptionMiddleware` | No documentado aqui |

## 9. Observaciones Tecnicas Importantes

- Los endpoints del modulo no requieren autenticacion por atributo aunque JWT este configurado globalmente.
- La serializacion JSON de ASP.NET Core usa camelCase por defecto en proyectos Web API modernos; por eso `ProductName` se consume como `productName`.
- `DateOnly` se debe enviar como string `YYYY-MM-DD`.
- `DateTime` se recibe como string ISO.
- `decimal` se consume como `number` en TypeScript.
- `MinimumStock` es `int` no nullable; si el cliente omite el campo, el binder puede dejar `0`.
- `PUT /api/Products/{id}` asigna `IsActive = dto.IsActive`, por lo que enviar `null` puede persistir `null`.
- `PATCH /api/Products/{id}` no permite borrar `ImageUrl` enviando `null`; solo actualiza si el valor no es null.
- La eliminacion de productos es fisica, no soft delete.
- El listado de inventario solo muestra productos activos.
- El listado de catalogo de productos no filtra activos salvo que se envie `active`.
- Existen endpoints publicos de inventario para listar lotes por producto y obtener detalle de lote. No existe todavia un endpoint dedicado para listar todas las ubicaciones.
