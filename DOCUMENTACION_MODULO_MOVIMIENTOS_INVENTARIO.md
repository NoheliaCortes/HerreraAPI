# Documentacion tecnica - Modulo de Movimientos de Inventario

## 1. Alcance

Este documento describe los endpoints actuales del modulo `InventoryMovements`.

El modulo permite:

- Consultar estadisticas operativas del dia.
- Consultar la tabla general paginada de movimientos.
- Consultar el encabezado de un movimiento.
- Consultar los detalles de un movimiento.
- Registrar transferencias manuales.
- Registrar ajustes positivos manuales.
- Registrar ajustes negativos manuales.

No incluye el endpoint de reabastecimiento (`POST /api/Restocks`), aunque ese flujo tambien crea registros en `InventoryMovements` con `MovementTypeId = 1`.

## 2. Ruta base

```http
/api/InventoryMovements
```

Controller:

```csharp
InventoryMovementsController
```

## 3. Arquitectura

El modulo sigue el patron usado en Sales y Restock:

- Controller: expone endpoints HTTP y envuelve respuestas con `ApiResponse<T>`.
- Service: orquesta las operaciones y delega consultas al repository.
- Repository: ejecuta consultas EF Core con `AsNoTracking()` y proyecta a DTOs.
- DTOs: contratos especificos por vista o accion.
- `PagedResponse<T>`: usado en listado general.
- `ApiResponse<T>`: usado en todas las respuestas.

## 4. Seguridad

Los endpoints GET no tienen atributo `[Authorize]`, igual que los endpoints GET de Sales y Restock.

Los endpoints POST actuales tampoco tienen `[Authorize]` en el controller de movimientos. Reciben `CreatedBy` desde el body.

| Endpoint | Autenticacion actual |
|---|---|
| `GET /api/InventoryMovements/stats` | No requerida |
| `GET /api/InventoryMovements` | No requerida |
| `GET /api/InventoryMovements/{id}` | No requerida |
| `GET /api/InventoryMovements/{id}/details` | No requerida |
| `POST /api/InventoryMovements/transfer` | No requerida |
| `POST /api/InventoryMovements/positive-adjustment` | No requerida |
| `POST /api/InventoryMovements/negative-adjustment` | No requerida |

## 5. Fechas y zona horaria

Las fechas automaticas se generan usando hora local de Nicaragua por medio de `INicaraguaDateTimeService`.

Campos relevantes:

- `InventoryMovement.MovementDate`
- `MovementDetail.CreatedAt`

El endpoint de estadisticas calcula el dia actual con:

```csharp
var now = _dateTimeService.Now;
var todayStart = now.Date;
var tomorrowStart = todayStart.AddDays(1);
```

Por tanto, `GET /api/InventoryMovements/stats` usa el dia local de Nicaragua, no UTC.

## 6. Tipos de movimiento

| MovementTypeId | Nombre |
|---:|---|
| `1` | Reabastecimiento |
| `1002` | Transferencia |
| `1003` | Ajuste Positivo |
| `1004` | Ajuste Negativo |

Notas:

- Reabastecimiento normalmente ingresa stock hacia Bodega y puede no tener ubicacion origen.
- Venta normalmente representa salida de inventario y puede no tener ubicacion destino.
- Transferencia normalmente tiene origen y destino.
- Ajuste Positivo normalmente no tiene origen y usa destino como ubicacion donde se suma stock.
- Ajuste Negativo normalmente tiene origen y no tiene destino.
- El frontend debe tratar los campos nullable como informacion no aplicable o no disponible.

## 7. Envolturas de respuesta

### 7.1 `ApiResponse<T>`

Todas las respuestas del controller usan:

```ts
interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T | null;
}
```

Ejemplo exitoso:

```json
{
  "success": true,
  "message": "Operacion exitosa",
  "data": {}
}
```

Ejemplo de error:

```json
{
  "success": false,
  "message": "Movimiento de inventario con Id 999 no encontrado",
  "data": null
}
```

### 7.2 `PagedResponse<T>`

Usado en `GET /api/InventoryMovements`.

```ts
interface PagedResponse<T> {
  data: T[];
  currentPage: number;
  pageSize: number;
  totalRecords: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}
```

## 8. DTOs de consulta

### 8.1 `InventoryMovementStatsDto`

```ts
interface InventoryMovementStatsDto {
  movementsToday: number;
  restocksToday: number;
  transfersToday: number;
  positiveAdjustmentsToday: number;
  negativeAdjustmentsToday: number;
}
```

### 8.2 `InventoryMovementQueryParams`

Hereda de `PaginationParams`.

```ts
interface InventoryMovementQueryParams {
  page?: number;
  pageSize?: number;
}
```

Valores por defecto:

| Parametro | Default | Maximo |
|---|---:|---:|
| `page` | `1` | N/A |
| `pageSize` | `10` | `50` |

### 8.3 `InventoryMovementListItemDto`

```ts
interface InventoryMovementListItemDto {
  id: number;
  movementTypeId: number;
  movementTypeName: string;
  movementDate?: string | null;
  createdByUserName?: string | null;
}
```

### 8.4 `InventoryMovementHeaderDto`

```ts
interface InventoryMovementHeaderDto {
  id: number;
  movementTypeId: number;
  movementTypeName: string;
  saleId?: number | null;
  orderId?: number | null;
  movementDate?: string | null;
  notes?: string | null;
  createdByUserName?: string | null;
}
```

### 8.5 `InventoryMovementDetailItemDto`

```ts
interface InventoryMovementDetailItemDto {
  id: number;
  batchId: number;
  batchCode?: string | null;
  sourceLocationName?: string | null;
  destinationLocationName?: string | null;
  quantity: number;
  unitCost: number;
  unitPrice?: number | null;
  createdByUserName?: string | null;
  createdAt?: string | null;
}
```

## 9. DTOs de registro manual

### 9.1 `CreateTransferDto`

```ts
interface CreateTransferDto {
  notes?: string | null;
  createdBy: number;
  details: TransferDetailDto[];
}
```

### 9.2 `TransferDetailDto`

```ts
interface TransferDetailDto {
  batchId: number;
  sourceLocationId: number;
  destinationLocationId: number;
  quantity: number;
}
```

Validaciones:

- `batchId`: requerido.
- `sourceLocationId`: requerido.
- `destinationLocationId`: requerido.
- `quantity`: requerido, minimo `1`.
- `details`: requerido, minimo 1 item.

### 9.3 `CreatePositiveAdjustmentDto`

```ts
interface CreatePositiveAdjustmentDto {
  notes?: string | null;
  createdBy: number;
  details: AdjustmentDetailDto[];
}
```

### 9.4 `CreateNegativeAdjustmentDto`

```ts
interface CreateNegativeAdjustmentDto {
  notes?: string | null;
  createdBy: number;
  details: AdjustmentDetailDto[];
}
```

### 9.5 `AdjustmentDetailDto`

```ts
interface AdjustmentDetailDto {
  batchId: number;
  locationId: number;
  quantity: number;
}
```

Validaciones:

- `batchId`: requerido.
- `locationId`: requerido.
- `quantity`: requerido, minimo `1`.
- `details`: requerido, minimo 1 item.

### 9.6 `InventoryMovementResultDto`

Respuesta usada por los endpoints POST.

```ts
interface InventoryMovementResultDto {
  id: number;
  movementTypeId: number;
  movementDate?: string | null;
  notes?: string | null;
  createdBy: number;
  details: MovementDetailResultDto[];
}
```

### 9.7 `MovementDetailResultDto`

```ts
interface MovementDetailResultDto {
  id: number;
  batchId: number;
  sourceLocationId?: number | null;
  destinationLocationId?: number | null;
  quantity: number;
  unitCost: number;
}
```

## 10. Endpoints de consulta

### 10.1 Estadisticas del dia

```http
GET /api/InventoryMovements/stats
```

Devuelve estadisticas operativas del dia actual usando hora local de Nicaragua.

Parametros: ninguno.

Respuesta:

```ts
ApiResponse<InventoryMovementStatsDto>
```

Ejemplo:

```json
{
  "success": true,
  "message": "Operacion exitosa",
  "data": {
    "movementsToday": 28,
    "restocksToday": 8,
    "transfersToday": 12,
    "positiveAdjustmentsToday": 5,
    "negativeAdjustmentsToday": 3
  }
}
```

Reglas:

- Cuenta movimientos con `MovementDate >= inicioDelDiaLocal`.
- Cuenta movimientos con `MovementDate < inicioDelDiaSiguienteLocal`.
- Ignora movimientos sin `MovementDate`.

### 10.2 Listado general paginado

```http
GET /api/InventoryMovements?page=1&pageSize=10
```

Devuelve la tabla general de movimientos.

Parametros:

| Nombre | Ubicacion | Tipo | Requerido | Default |
|---|---|---|---|---|
| `page` | Query | `int` | No | `1` |
| `pageSize` | Query | `int` | No | `10` |

Orden:

1. `MovementDate` descendente.
2. `Id` descendente.

Respuesta:

```ts
ApiResponse<PagedResponse<InventoryMovementListItemDto>>
```

Ejemplo:

```json
{
  "success": true,
  "message": "Operacion exitosa",
  "data": {
    "data": [
      {
        "id": 15,
        "movementTypeId": 1002,
        "movementTypeName": "Transferencia",
        "movementDate": "2026-06-25T09:45:00",
        "createdByUserName": "admin"
      }
    ],
    "currentPage": 1,
    "pageSize": 10,
    "totalRecords": 1,
    "totalPages": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
  }
}
```

### 10.3 Encabezado de movimiento

```http
GET /api/InventoryMovements/{id}
```

Devuelve el encabezado de un movimiento.

Parametros:

| Nombre | Ubicacion | Tipo | Requerido |
|---|---|---|---|
| `id` | Route | `int` | Si |

Respuesta exitosa:

```ts
ApiResponse<InventoryMovementHeaderDto>
```

Ejemplo:

```json
{
  "success": true,
  "message": "Operacion exitosa",
  "data": {
    "id": 15,
    "movementTypeId": 1002,
    "movementTypeName": "Transferencia",
    "saleId": null,
    "orderId": null,
    "movementDate": "2026-06-25T09:45:00",
    "notes": "Traslado a mostrador",
    "createdByUserName": "admin"
  }
}
```

Respuesta si no existe:

```http
404 Not Found
```

```json
{
  "success": false,
  "message": "Movimiento de inventario con Id 999 no encontrado",
  "data": null
}
```

### 10.4 Detalles de movimiento

```http
GET /api/InventoryMovements/{id}/details
```

Devuelve los detalles de lote, ubicaciones, cantidades y costos asociados al movimiento.

Parametros:

| Nombre | Ubicacion | Tipo | Requerido |
|---|---|---|---|
| `id` | Route | `int` | Si |

Respuesta exitosa:

```ts
ApiResponse<IReadOnlyList<InventoryMovementDetailItemDto>>
```

Ejemplo:

```json
{
  "success": true,
  "message": "Operacion exitosa",
  "data": [
    {
      "id": 1,
      "batchId": 25,
      "batchCode": "PRE-FRE-LIT-2026-001",
      "sourceLocationName": "Bodega",
      "destinationLocationName": "Mostrador",
      "quantity": 15,
      "unitCost": 35,
      "unitPrice": null,
      "createdByUserName": "admin",
      "createdAt": "2026-06-25T09:45:00"
    }
  ]
}
```

Respuesta si el movimiento existe pero no tiene detalles:

```json
{
  "success": true,
  "message": "Operacion exitosa",
  "data": []
}
```

Respuesta si no existe:

```http
404 Not Found
```

```json
{
  "success": false,
  "message": "Movimiento de inventario con Id 999 no encontrado",
  "data": null
}
```

Campos nullable importantes:

- `batchCode`
- `sourceLocationName`
- `destinationLocationName`
- `unitPrice`
- `createdByUserName`
- `createdAt`

## 11. Endpoints de registro manual

### 11.1 Transferencia

```http
POST /api/InventoryMovements/transfer
```

Registra una transferencia de stock entre ubicaciones.

Body:

```json
{
  "notes": "Traslado a mostrador",
  "createdBy": 1,
  "details": [
    {
      "batchId": 25,
      "sourceLocationId": 1,
      "destinationLocationId": 2,
      "quantity": 15
    }
  ]
}
```

Respuesta exitosa:

```http
201 Created
```

```json
{
  "success": true,
  "message": "Transferencia registrada exitosamente",
  "data": {
    "id": 15,
    "movementTypeId": 1002,
    "movementDate": "2026-06-25T09:45:00",
    "notes": "Traslado a mostrador",
    "createdBy": 1,
    "details": [
      {
        "id": 1,
        "batchId": 25,
        "sourceLocationId": 1,
        "destinationLocationId": 2,
        "quantity": 15,
        "unitCost": 35
      }
    ]
  }
}
```

Reglas de negocio:

- `SourceLocationId` y `DestinationLocationId` no pueden ser iguales.
- El lote debe existir.
- El lote debe tener stock en la ubicacion origen.
- El stock origen debe ser suficiente.
- Si no existe `BatchLocation` en destino, se crea automaticamente con stock inicial `0`.
- Resta stock de origen.
- Suma stock en destino.
- Crea `InventoryMovement` con `MovementTypeId = 1002`.
- Crea `MovementDetail` con origen y destino.

Errores conocidos:

```http
400 Bad Request
```

Casos:

- Origen y destino iguales.
- Lote inexistente.
- El lote no tiene stock en origen.
- Stock insuficiente.

### 11.2 Ajuste positivo

```http
POST /api/InventoryMovements/positive-adjustment
```

Registra una entrada manual de stock en una ubicacion.

Body:

```json
{
  "notes": "Conteo fisico encontro unidades adicionales",
  "createdBy": 1,
  "details": [
    {
      "batchId": 25,
      "locationId": 1,
      "quantity": 5
    }
  ]
}
```

Respuesta exitosa:

```http
201 Created
```

```json
{
  "success": true,
  "message": "Ajuste positivo registrado exitosamente",
  "data": {
    "id": 16,
    "movementTypeId": 1003,
    "movementDate": "2026-06-25T10:10:00",
    "notes": "Conteo fisico encontro unidades adicionales",
    "createdBy": 1,
    "details": [
      {
        "id": 2,
        "batchId": 25,
        "sourceLocationId": null,
        "destinationLocationId": 1,
        "quantity": 5,
        "unitCost": 35
      }
    ]
  }
}
```

Reglas de negocio:

- El lote debe existir.
- Si no existe `BatchLocation` para lote y ubicacion, se crea automaticamente con stock inicial `0`.
- Suma stock en la ubicacion indicada.
- Crea `InventoryMovement` con `MovementTypeId = 1003`.
- Crea `MovementDetail` con `SourceLocationId = null` y `DestinationLocationId = locationId`.

Errores conocidos:

```http
400 Bad Request
```

Casos:

- Lote inexistente.
- Validaciones de body.

### 11.3 Ajuste negativo

```http
POST /api/InventoryMovements/negative-adjustment
```

Registra una salida manual de stock desde una ubicacion.

Body:

```json
{
  "notes": "Merma por producto danado",
  "createdBy": 1,
  "details": [
    {
      "batchId": 25,
      "locationId": 1,
      "quantity": 3
    }
  ]
}
```

Respuesta exitosa:

```http
201 Created
```

```json
{
  "success": true,
  "message": "Ajuste negativo registrado exitosamente",
  "data": {
    "id": 17,
    "movementTypeId": 1004,
    "movementDate": "2026-06-25T10:30:00",
    "notes": "Merma por producto danado",
    "createdBy": 1,
    "details": [
      {
        "id": 3,
        "batchId": 25,
        "sourceLocationId": 1,
        "destinationLocationId": null,
        "quantity": 3,
        "unitCost": 35
      }
    ]
  }
}
```

Reglas de negocio:

- El lote debe existir.
- Debe existir `BatchLocation` para lote y ubicacion.
- El stock en la ubicacion debe ser suficiente.
- Resta stock en la ubicacion indicada.
- Crea `InventoryMovement` con `MovementTypeId = 1004`.
- Crea `MovementDetail` con `SourceLocationId = locationId` y `DestinationLocationId = null`.

Errores conocidos:

```http
400 Bad Request
```

Casos:

- Lote inexistente.
- El lote no tiene stock en la ubicacion.
- Stock insuficiente.
- Validaciones de body.

## 12. Codigos HTTP

| Escenario | Codigo | Forma |
|---|---:|---|
| Consulta exitosa | `200 OK` | `ApiResponse<T>` |
| Creacion exitosa | `201 Created` | `ApiResponse<InventoryMovementResultDto>` |
| Recurso no encontrado | `404 Not Found` | `ApiResponse<T>.Fail(message)` |
| Regla de negocio fallida | `400 Bad Request` | `ApiResponse<InventoryMovementResultDto>.Fail(message)` |
| Validacion de body | `400 Bad Request` | Segun comportamiento global de validacion |

## 13. Guia rapida para frontend

### Dashboard

1. Consumir `GET /api/InventoryMovements/stats`.
2. Pintar cards:
   - `movementsToday`
   - `restocksToday`
   - `transfersToday`
   - `positiveAdjustmentsToday`
   - `negativeAdjustmentsToday`

### Tabla principal

1. Consumir `GET /api/InventoryMovements?page=1&pageSize=10`.
2. Usar `data.data` como filas.
3. Usar metadatos de `PagedResponse<T>` para paginacion.
4. Mostrar columnas:
   - ID
   - Tipo de movimiento
   - Fecha
   - Usuario creador

### Drawer de detalle

1. Al seleccionar una fila, llamar `GET /api/InventoryMovements/{id}`.
2. Llamar `GET /api/InventoryMovements/{id}/details`.
3. Renderizar `null` como "No aplica" o vacio, segun UX.
4. No asumir que existen origen y destino para todos los tipos de movimiento.

## 14. Resumen de endpoints

| Metodo | Endpoint | Uso | Respuesta |
|---|---|---|---|
| `GET` | `/api/InventoryMovements/stats` | Cards de dashboard | `ApiResponse<InventoryMovementStatsDto>` |
| `GET` | `/api/InventoryMovements` | Tabla paginada | `ApiResponse<PagedResponse<InventoryMovementListItemDto>>` |
| `GET` | `/api/InventoryMovements/{id}` | Encabezado drawer | `ApiResponse<InventoryMovementHeaderDto>` |
| `GET` | `/api/InventoryMovements/{id}/details` | Detalles drawer | `ApiResponse<IReadOnlyList<InventoryMovementDetailItemDto>>` |
| `POST` | `/api/InventoryMovements/transfer` | Registrar transferencia | `ApiResponse<InventoryMovementResultDto>` |
| `POST` | `/api/InventoryMovements/positive-adjustment` | Registrar ajuste positivo | `ApiResponse<InventoryMovementResultDto>` |
| `POST` | `/api/InventoryMovements/negative-adjustment` | Registrar ajuste negativo | `ApiResponse<InventoryMovementResultDto>` |
