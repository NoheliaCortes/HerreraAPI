# Documentacion tecnica Front-end - Modulo de Reabastecimientos

Este documento describe el contrato real disponible en la API para construir el modulo de reabastecimientos o `Restocks` en frontend. La documentacion se basa en controladores, DTOs, servicios, repositorios, entidades EF Core y reglas implementadas en el codigo actual.

Fuentes principales revisadas:

- `HerreraSystemAPI/Controllers/RestocksController.cs`
- `HerreraSystem.Application/DTOs/RestockDtos/*.cs`
- `HerreraSystem.Application/Services/RestockService.cs`
- `HerreraSystem.Infrastructure/Repositories/RestockRepository.cs`
- `HerreraSystem.Infrastructure/Repositories/BatchRepository.cs`
- `HerreraSystem.Infrastructure/Data/HerreraSystemContext.cs`
- `HerreraSystem.Domain/Entities/Restock.cs`
- `HerreraSystem.Domain/Entities/Batch.cs`
- `HerreraSystem.Domain/Entities/Product.cs`
- `HerreraSystem.Domain/Entities/User.cs`
- `HerreraSystem.Domain/Entities/BatchStatus.cs`
- `HerreraSystem.Domain/Entities/BatchLocation.cs`
- `HerreraSystem.Domain/Entities/InventoryMovement.cs`
- `HerreraSystem.Domain/Entities/MovementDetail.cs`

## 1. Resumen para Front-end

Base path del controlador:

```http
/api/Restocks
```

ASP.NET Core no distingue mayusculas/minusculas en rutas por defecto, por lo que tambien se puede consumir como:

```http
/api/restocks
```

Endpoints disponibles:

| Operacion | Metodo | Ruta | JWT requerido | Respuesta |
|---|---:|---|---:|---|
| Listar reabastecimientos paginados | GET | `/api/restocks` | No | `ApiResponse<PagedResponse<RestockListItemDto>>` |
| Obtener detalle de reabastecimiento | GET | `/api/restocks/{id}/detail` | No | `ApiResponse<RestockDetailDto>` |
| Obtener estadisticas del mes actual | GET | `/api/restocks/statistics` | No | `ApiResponse<RestockStatisticsDto>` |
| Crear/publicar reabastecimiento | POST | `/api/restocks` | Si | `ApiResponse<RestockResponseDto>` |

Estado actual importante:

- No existe tabla ni entidad `RestockDetail`.
- `Restock` funciona como cabecera.
- `Batch` funciona como detalle del reabastecimiento.
- Cada lote tiene `RestockId`.
- Los totales del modulo se calculan desde `Batches`.
- Crear un reabastecimiento tambien crea:
  - Un `InventoryMovement` de entrada.
  - Uno o varios `Batch`.
  - Un `BatchLocation` inicial en ubicacion `1`.
  - Un `MovementDetail` por lote.
- Las respuestas controladas usan `ApiResponse<T>`.
- El endpoint `POST /api/restocks` esta protegido con `[Authorize]`.
- Los endpoints `GET` de consulta siguen abiertos, igual que el patron actual de Products.
- El frontend ya no debe enviar `createdBy`; el backend lo obtiene desde el JWT mediante `ICurrentUserService.CurrentUserId`.
- Las fechas `DateTime` se consumen como string ISO.
- Las fechas `DateOnly` se consumen como string `YYYY-MM-DD`.
- Los montos `decimal` se consumen como `number` en TypeScript.

## 2. Convenciones comunes

### 2.1 `ApiResponse<T>`

Todas las respuestas exitosas/controladas usan:

```ts
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
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

Ejemplo de error controlado:

```json
{
  "success": false,
  "message": "Restock con Id 99 no encontrado",
  "data": null
}
```

### 2.2 `PagedResponse<T>`

Usado por el listado principal:

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

Parametros de paginacion:

| Query | Tipo | Default | Regla |
|---|---:|---:|---|
| `page` / `Page` | `number` | `1` | Pagina solicitada. |
| `pageSize` / `PageSize` | `number` | `10` | Maximo aplicado por backend: `50`. |

Ejemplo:

```http
GET /api/restocks?page=1&pageSize=20
```

## 3. Modelo de datos relevante

### 3.1 Relacion principal

El modelo real es:

```txt
Restock 1 ---- N Batch
```

`Restock` es la cabecera del reabastecimiento. `Batch` representa cada lote creado dentro de ese reabastecimiento.

No se debe modelar en frontend una tabla intermedia `RestockDetail`, porque no existe en backend.

### 3.2 `Restock`

Entidad base de cabecera.

| Propiedad | C# | TypeScript | Nullable | Uso |
|---|---:|---:|---:|---|
| `Id` | `int` | `number` | No | Identificador del reabastecimiento. |
| `RestockDate` | `DateTime?` | `string \| null` | Si | Fecha de registro. En creacion se asigna `DateTime.UtcNow`. |
| `CreatedBy` | `int` | `number` | No | Usuario que registro el reabastecimiento. |
| `RestockCode` | `string` | `string` | No | Codigo unico generado por backend. |

Navegaciones importantes:

| Navegacion | Relacion | Uso en modulo |
|---|---|---|
| `Batches` | Uno a muchos | Fuente de lotes, cantidades y costos. |
| `CreatedByNavigation` | Muchos a uno con `User` | Fuente del `userName`. |

### 3.3 `Batch`

Entidad que funciona como detalle del restock.

| Propiedad | C# | TypeScript | Nullable | Uso |
|---|---:|---:|---:|---|
| `Id` | `int` | `number` | No | Identificador del lote. |
| `RestockId` | `int` | `number` | No | FK al reabastecimiento. |
| `ProductId` | `int` | `number` | No | Producto del lote. |
| `BatchStatusId` | `int` | `number` | No | Estado del lote. |
| `InitialQuantity` | `int` | `number` | No | Cantidad inicial producida/ingresada. |
| `UnitProductionCost` | `decimal` | `number` | No | Costo unitario de produccion. |
| `ExpirationDate` | `DateOnly` | `string` | No | Fecha de vencimiento, formato `YYYY-MM-DD`. |
| `BatchCode` | `string?` | `string \| null` | Si | Codigo unico del lote, generado por backend. |

Navegaciones usadas en consultas:

| Navegacion | Uso |
|---|---|
| `Product.ProductName` | Nombre visible del producto. |
| `BatchStatus.BatchStatusName` | Nombre del estado del lote. |
| `Restock.RestockDate` | Fecha de entrada del lote en otros modulos. |
| `BatchLocations` | Stock por ubicacion. |
| `MovementDetails` | Trazabilidad de movimientos. |

### 3.4 `Product`

Solo se expone indirectamente en este modulo para mostrar `productName` en el detalle de lotes.

| Propiedad usada | TypeScript | Uso |
|---|---:|---|
| `ProductName` | `string` | Nombre del producto mostrado en detalle. |

Para construir el formulario de creacion, el frontend debe obtener productos desde el modulo de productos/inventario, por ejemplo:

```http
GET /api/Products/catalog
GET /api/inventory/products
```

La eleccion depende de la pantalla y de la informacion visual requerida.

### 3.5 `User`

`Restock.CreatedBy` apunta a `User.Id`.

| Propiedad usada | TypeScript | Uso |
|---|---:|---|
| `UserName` | `string` | Campo `userName` en listados y detalle. |

En creacion, el frontend no envia `createdBy`. El backend obtiene el usuario desde el token JWT y guarda ese id en `Restock.CreatedBy`.

### 3.6 `BatchStatus`

En creacion, backend fuerza:

```txt
BatchStatusId = 1
```

En consultas de detalle se devuelve:

| Campo | TypeScript | Uso |
|---|---:|---|
| `batchStatusName` | `string` | Texto visible del estado del lote. |

### 3.7 `BatchLocation`, `InventoryMovement` y `MovementDetail`

Estas entidades no se exponen directamente por los endpoints de Restock, pero se crean al publicar un reabastecimiento:

| Entidad | Registro creado | Regla actual |
|---|---|---|
| `InventoryMovement` | 1 por restock | `MovementTypeId = 1`, `MovementDate = DateTime.UtcNow`, `CreatedBy = CurrentUserId.Value`, `Notes = dto.Notes`. |
| `BatchLocation` | 1 por lote | `LocationId = 1`, `CurrentStock = quantity`. |
| `MovementDetail` | 1 por lote | `DestinationLocationId = 1`, `Quantity = quantity`, `UnitCost = unitProductionCost`, `CreatedBy = CurrentUserId.Value`. |

Esto significa que despues de crear un restock, el inventario queda afectado automaticamente.

## 4. Reglas de calculo

Los calculos del modulo se hacen desde los lotes asociados al reabastecimiento.

| Campo | Formula |
|---|---|
| `batchCount` | `restock.batches.count()` |
| `totalUnits` | `sum(batch.initialQuantity)` |
| `totalInvestment` | `sum(batch.initialQuantity * batch.unitProductionCost)` |
| `differentProductsCount` | `count(distinct batch.productId)` |
| `totalCost` de lote | `batch.initialQuantity * batch.unitProductionCost` |
| `restocksThisMonth` | Cantidad de restocks con `RestockDate` dentro del mes actual UTC. |
| `totalInvestmentThisMonth` | Suma de inversion de lotes cuyos restocks son del mes actual UTC. |
| `batchesCreatedThisMonth` | Cantidad de lotes asociados a restocks del mes actual UTC. |

Nota de fechas:

- Si el listado no recibe `fromDate` ni `toDate`, backend filtra por el mes actual usando `DateTime.UtcNow`.
- Si se envia `toDate`, backend la trata de forma inclusiva a nivel de dia: internamente usa `< toDate.Date.AddDays(1)`.
- Si se envia solo `fromDate`, no hay limite superior practico.
- Si se envia solo `toDate`, no hay limite inferior practico.

## 5. DTOs TypeScript recomendados

### 5.1 Listado: `RestockListItemDto`

Usado por:

```http
GET /api/restocks
```

```ts
export interface RestockListItemDto {
  restockId: number;
  restockCode: string;
  restockDate: string | null;
  userName: string;
  batchCount: number;
  totalUnits: number;
  totalInvestment: number;
}
```

### 5.2 Filtros de listado: `RestockQueryParams`

```ts
export interface RestockQueryParams {
  fromDate?: string | null;
  toDate?: string | null;
  search?: string | null;
  page?: number;
  pageSize?: number;
}
```

Formato recomendado para fechas:

```txt
YYYY-MM-DD
```

Ejemplo:

```ts
const params: RestockQueryParams = {
  fromDate: "2026-06-01",
  toDate: "2026-06-30",
  search: "RST-2026",
  page: 1,
  pageSize: 10
};
```

### 5.3 Detalle: `RestockDetailDto`

Usado por:

```http
GET /api/restocks/{id}/detail
```

```ts
export interface RestockDetailDto {
  restockId: number;
  restockCode: string;
  restockDate: string | null;
  userName: string;
  batchCount: number;
  totalUnits: number;
  totalInvestment: number;
  differentProductsCount: number;
  batches: RestockDetailBatchDto[];
}
```

### 5.4 Lote en detalle: `RestockDetailBatchDto`

```ts
export interface RestockDetailBatchDto {
  batchId: number;
  batchCode: string | null;
  productName: string;
  batchStatusName: string;
  initialQuantity: number;
  unitProductionCost: number;
  totalCost: number;
  expirationDate: string;
}
```

### 5.5 Estadisticas: `RestockStatisticsDto`

Usado por:

```http
GET /api/restocks/statistics
```

```ts
export interface RestockStatisticsDto {
  restocksThisMonth: number;
  totalInvestmentThisMonth: number;
  batchesCreatedThisMonth: number;
}
```

### 5.6 Crear restock: `CreateRestockDto`

Usado por:

```http
POST /api/restocks
```

```ts
export interface CreateRestockRequest {
  notes?: string | null;
  batches: CreateRestockBatchRequest[];
}
```

### 5.7 Crear lote dentro de restock: `CreateRestockBatchDto`

```ts
export interface CreateRestockBatchRequest {
  productId: number;
  quantity: number;
  unitProductionCost: number;
  expirationDate: string;
}
```

Reglas de formulario:

| Campo | Obligatorio | Regla |
|---|---:|---|
| `notes` | No | Texto libre. |
| `batches` | Si | Debe incluir al menos un lote. |
| `productId` | Si | Debe existir en productos. |
| `quantity` | Si | Mayor a `0`. |
| `unitProductionCost` | Si | Mayor a `0`. |
| `expirationDate` | Si | Debe ser futura segun validacion de servicio. |

Advertencia de model binding:

- `productId` y `quantity` son `int` no-nullable.
- Si el frontend omite alguno, ASP.NET puede bindearlo como `0`; `[Required]` no siempre detecta ausencia en tipos valor.
- El frontend debe validar explicitamente que sean mayores a `0`.
- El usuario autenticado no forma parte del body; debe viajar en el header `Authorization: Bearer <token>`.

### 5.8 Respuesta al crear: `RestockResponseDto`

```ts
export interface RestockResponseDto {
  restockId: number;
  restockCode: string;
  inventoryMovementId: number;
  restockDate: string;
  batches: RestockBatchResponseDto[];
}
```

```ts
export interface RestockBatchResponseDto {
  batchId: number;
  batchCode: string;
  productName: string;
  quantity: number;
  unitProductionCost: number;
  expirationDate: string;
}
```

## 6. Endpoints detallados

### 6.1 Listar reabastecimientos

```http
GET /api/restocks
```

Query params:

| Query | Tipo | Obligatorio | Default backend | Descripcion |
|---|---:|---:|---|---|
| `fromDate` / `FromDate` | `string` | No | Inicio del mes actual si no hay fechas | Fecha inicial. |
| `toDate` / `ToDate` | `string` | No | Inicio del mes siguiente si no hay fechas | Fecha final inclusiva por dia. |
| `search` / `Search` | `string` | No | `null` | Busca por codigo de restock o usuario. |
| `page` / `Page` | `number` | No | `1` | Pagina. |
| `pageSize` / `PageSize` | `number` | No | `10` | Maximo `50`. |

Ejemplo:

```http
GET /api/restocks?fromDate=2026-06-01&toDate=2026-06-30&search=RST&page=1&pageSize=10
```

Respuesta:

```json
{
  "success": true,
  "message": "Operacion exitosa",
  "data": {
    "data": [
      {
        "restockId": 12,
        "restockCode": "RST-2026-0007",
        "restockDate": "2026-06-23T14:30:00",
        "userName": "admin",
        "batchCount": 3,
        "totalUnits": 180,
        "totalInvestment": 2700.00
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

Uso recomendado en UI:

- Tabla principal con columnas: codigo, fecha, usuario, lotes, unidades, inversion y accion "Ver detalle".
- Cards superiores pueden consumir `/api/restocks/statistics`.
- Si el usuario no selecciona fechas, no enviar fechas para usar el mes actual del backend.
- Para busqueda, usar debounce de 300 a 500 ms.

### 6.2 Obtener detalle de reabastecimiento

```http
GET /api/restocks/{id}/detail
```

Ejemplo:

```http
GET /api/restocks/12/detail
```

Respuesta exitosa:

```json
{
  "success": true,
  "message": "Operacion exitosa",
  "data": {
    "restockId": 12,
    "restockCode": "RST-2026-0007",
    "restockDate": "2026-06-23T14:30:00",
    "userName": "admin",
    "batchCount": 3,
    "totalUnits": 180,
    "totalInvestment": 2700.00,
    "differentProductsCount": 2,
    "batches": [
      {
        "batchId": 45,
        "batchCode": "HEL-CHO-500-2026-0045",
        "productName": "Helado Chocolate 500ml",
        "batchStatusName": "Activo",
        "initialQuantity": 100,
        "unitProductionCost": 15.00,
        "totalCost": 1500.00,
        "expirationDate": "2026-12-31"
      }
    ]
  }
}
```

Respuesta si no existe:

```http
HTTP/1.1 404 Not Found
```

```json
{
  "success": false,
  "message": "Restock con Id 12 no encontrado",
  "data": null
}
```

Uso recomendado en UI:

- Encabezado con `restockCode`, fecha y usuario.
- Cards: lotes, unidades, inversion y productos distintos.
- Tabla de lotes: codigo, producto, estado, cantidad inicial, costo unitario, costo total y vencimiento.
- `totalCost` ya viene calculado por backend; no recalcular para persistencia, solo para validacion visual si se desea.

### 6.3 Estadisticas de reabastecimientos

```http
GET /api/restocks/statistics
```

Respuesta:

```json
{
  "success": true,
  "message": "Operacion exitosa",
  "data": {
    "restocksThisMonth": 7,
    "totalInvestmentThisMonth": 18250.00,
    "batchesCreatedThisMonth": 21
  }
}
```

Uso recomendado en UI:

- Card 1: "Reabastecimientos del mes" -> `restocksThisMonth`.
- Card 2: "Inversion del mes" -> `totalInvestmentThisMonth`.
- Card 3: "Lotes creados del mes" -> `batchesCreatedThisMonth`.

Nota: estas estadisticas siempre son del mes actual calculado en backend con `DateTime.UtcNow`; no aceptan filtros.

### 6.4 Crear/publicar reabastecimiento

```http
POST /api/restocks
Content-Type: application/json
```

Request:

```json
{
  "notes": "Produccion semanal",
  "batches": [
    {
      "productId": 10,
      "quantity": 100,
      "unitProductionCost": 15.50,
      "expirationDate": "2026-12-31"
    },
    {
      "productId": 11,
      "quantity": 80,
      "unitProductionCost": 12.25,
      "expirationDate": "2026-11-30"
    }
  ]
}
```

Respuesta exitosa:

```http
HTTP/1.1 201 Created
```

```json
{
  "success": true,
  "message": "Restock creado exitosamente",
  "data": {
    "restockId": 12,
    "restockCode": "RST-2026-0007",
    "inventoryMovementId": 34,
    "restockDate": "2026-06-23T14:30:00Z",
    "batches": [
      {
        "batchId": 45,
        "batchCode": "HEL-CHO-500-2026-0045",
        "productName": "Helado Chocolate 500ml",
        "quantity": 100,
        "unitProductionCost": 15.50,
        "expirationDate": "2026-12-31"
      }
    ]
  }
}
```

Errores controlados posibles:

```json
{
  "success": false,
  "message": "Debe incluir al menos un lote",
  "data": null
}
```

```json
{
  "success": false,
  "message": "El producto con Id 10 no existe",
  "data": null
}
```

```json
{
  "success": false,
  "message": "La fecha de vencimiento del producto Id 10 debe ser futura",
  "data": null
}
```

Validaciones automaticas por Data Annotations devuelven HTTP `400 Bad Request` con `ApiResponse<List<string>>`.

Ejemplo:

```json
{
  "success": false,
  "message": "Errores de validacion",
  "data": [
    "La cantidad debe ser mayor a 0"
  ]
}
```

## 7. Generacion de codigos

### 7.1 Codigo de restock

Se genera en backend con:

```txt
RST-{YEAR}-{CORRELATIVO_4_DIGITOS}
```

Ejemplo:

```txt
RST-2026-0007
```

El correlativo se calcula contando restocks del mismo anio.

### 7.2 Codigo de lote

Se genera en backend usando linea, sabor, presentacion, anio y correlativo:

```txt
{LINEA3}-{SABOR3}-{PRESENTACION3}-{YEAR}-{CORRELATIVO_4_DIGITOS}
```

Ejemplo aproximado:

```txt
HEL-CHO-500-2026-0045
```

Reglas observadas:

- Toma los primeros 3 caracteres de la linea.
- Toma los primeros 3 caracteres del sabor.
- Toma los primeros 3 caracteres de la presentacion despues de quitar espacios.
- Convierte a mayusculas.
- El correlativo se calcula contando lotes del mismo anio.

El frontend no debe generar ni enviar `restockCode` ni `batchCode`.

## 8. Flujo recomendado de pantalla

### 8.1 Pantalla principal

Al montar la pantalla:

1. Llamar `GET /api/restocks/statistics`.
2. Llamar `GET /api/restocks?page=1&pageSize=10`.
3. Mostrar filtros de fecha y busqueda.
4. Si no hay fechas seleccionadas, no enviar `fromDate` ni `toDate`.

Estados UI sugeridos:

| Estado | Comportamiento |
|---|---|
| Loading inicial | Skeleton de cards y tabla. |
| Sin registros | Empty state: "No hay reabastecimientos en este periodo". |
| Error | Mostrar `message` de `ApiResponse` si existe. |
| Cambio de filtros | Resetear `page` a `1`. |

### 8.2 Detalle

Al hacer click en una fila:

```http
GET /api/restocks/{restockId}/detail
```

Mostrar:

- Datos generales del reabastecimiento.
- Resumen financiero.
- Tabla de lotes creados.

### 8.3 Crear restock

Formulario recomendado:

- Selector de producto por lote.
- Cantidad.
- Costo unitario de produccion.
- Fecha de vencimiento.
- Boton para agregar/remover lotes.
- Notas generales opcionales.

Validaciones frontend antes de enviar:

- Debe existir al menos un lote.
- Cada lote debe tener producto seleccionado.
- `quantity > 0`.
- `unitProductionCost > 0`.
- `expirationDate` futura.
- No enviar `createdBy`, `userId`, `registeredBy` ni campos equivalentes de usuario.
- Enviar `Authorization: Bearer <token>` al crear.

Despues de crear exitosamente:

1. Mostrar `restockCode`.
2. Mostrar resumen de lotes creados con sus `batchCode`.
3. Refrescar listado y estadisticas.
4. Opcionalmente navegar a `/restocks/{restockId}` o abrir el detalle.

## 9. Cliente TypeScript sugerido

```ts
const API_BASE = "/api/restocks";

export async function getRestocks(params: RestockQueryParams) {
  const search = new URLSearchParams();

  if (params.fromDate) search.set("fromDate", params.fromDate);
  if (params.toDate) search.set("toDate", params.toDate);
  if (params.search) search.set("search", params.search);
  if (params.page) search.set("page", String(params.page));
  if (params.pageSize) search.set("pageSize", String(params.pageSize));

  const response = await fetch(`${API_BASE}?${search.toString()}`);
  return response.json() as Promise<ApiResponse<PagedResponse<RestockListItemDto>>>;
}

export async function getRestockDetail(id: number) {
  const response = await fetch(`${API_BASE}/${id}/detail`);
  return response.json() as Promise<ApiResponse<RestockDetailDto>>;
}

export async function getRestockStatistics() {
  const response = await fetch(`${API_BASE}/statistics`);
  return response.json() as Promise<ApiResponse<RestockStatisticsDto>>;
}

export async function createRestock(payload: CreateRestockRequest) {
  const response = await fetch(API_BASE, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "Authorization": `Bearer ${token}`
    },
    body: JSON.stringify(payload)
  });

  return response.json() as Promise<ApiResponse<RestockResponseDto>>;
}
```

Para el `POST /api/restocks`, el header de autenticacion es obligatorio:

```ts
headers: {
  "Content-Type": "application/json",
  "Authorization": `Bearer ${token}`
}
```

Los endpoints `GET` pueden consumirse sin token en el estado actual del proyecto.

## 10. Formateo recomendado

### 10.1 Dinero

```ts
export function formatCurrency(value: number) {
  return new Intl.NumberFormat("es-NI", {
    style: "currency",
    currency: "NIO"
  }).format(value);
}
```

Si el negocio maneja otra moneda visual, cambiar `currency`.

### 10.2 Fechas

Para `DateTime`:

```ts
export function formatDateTime(value: string | null) {
  if (!value) return "-";
  return new Intl.DateTimeFormat("es-NI", {
    dateStyle: "medium",
    timeStyle: "short"
  }).format(new Date(value));
}
```

Para `DateOnly` (`YYYY-MM-DD`), evitar desfases de zona horaria al mostrar:

```ts
export function formatDateOnly(value: string) {
  const [year, month, day] = value.split("-").map(Number);
  return new Intl.DateTimeFormat("es-NI", {
    dateStyle: "medium"
  }).format(new Date(year, month - 1, day));
}
```

## 11. Checklist de integracion Front-end

- Usar `/api/restocks/statistics` para cards del mes actual.
- Usar `/api/restocks` para tabla paginada.
- No enviar fechas si se quiere el mes actual por defecto.
- Enviar `fromDate` y `toDate` en formato `YYYY-MM-DD`.
- Implementar busqueda por codigo o usuario con `search`.
- Consumir detalle desde `/api/restocks/{id}/detail`.
- No inventar `RestockDetail`; los detalles son `batches`.
- No enviar `restockCode` ni `batchCode` al crear.
- Validar al menos un lote antes de enviar.
- Validar cantidades, costos y vencimientos en frontend.
- Refrescar estadisticas/listado despues de crear.
- Manejar HTTP `404` en detalle con empty/error state.
- Mostrar montos con formato de moneda.
- Mostrar `expirationDate` como fecha sin conversion UTC.

## 12. Contratos completos resumidos

```ts
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
}

export interface PagedResponse<T> {
  data: T[];
  currentPage: number;
  pageSize: number;
  totalRecords: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface RestockQueryParams {
  fromDate?: string | null;
  toDate?: string | null;
  search?: string | null;
  page?: number;
  pageSize?: number;
}

export interface RestockListItemDto {
  restockId: number;
  restockCode: string;
  restockDate: string | null;
  userName: string;
  batchCount: number;
  totalUnits: number;
  totalInvestment: number;
}

export interface RestockDetailDto {
  restockId: number;
  restockCode: string;
  restockDate: string | null;
  userName: string;
  batchCount: number;
  totalUnits: number;
  totalInvestment: number;
  differentProductsCount: number;
  batches: RestockDetailBatchDto[];
}

export interface RestockDetailBatchDto {
  batchId: number;
  batchCode: string | null;
  productName: string;
  batchStatusName: string;
  initialQuantity: number;
  unitProductionCost: number;
  totalCost: number;
  expirationDate: string;
}

export interface RestockStatisticsDto {
  restocksThisMonth: number;
  totalInvestmentThisMonth: number;
  batchesCreatedThisMonth: number;
}

export interface CreateRestockRequest {
  notes?: string | null;
  batches: CreateRestockBatchRequest[];
}

export interface CreateRestockBatchRequest {
  productId: number;
  quantity: number;
  unitProductionCost: number;
  expirationDate: string;
}

export interface RestockResponseDto {
  restockId: number;
  restockCode: string;
  inventoryMovementId: number;
  restockDate: string;
  batches: RestockBatchResponseDto[];
}

export interface RestockBatchResponseDto {
  batchId: number;
  batchCode: string;
  productName: string;
  quantity: number;
  unitProductionCost: number;
  expirationDate: string;
}
```
