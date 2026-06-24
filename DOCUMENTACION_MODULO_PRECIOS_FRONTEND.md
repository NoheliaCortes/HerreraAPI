# Documentacion tecnica Front-end - Modulo de Precios

Este documento describe el contrato real disponible en la API para construir el modulo de precios en frontend. La documentacion se basa en controladores, DTOs, servicios, repositorios, entidades EF Core y las reglas implementadas en el codigo actual.

Fuentes principales revisadas:

- `HerreraSystemAPI/Controllers/GeneralPricesController.cs`
- `HerreraSystemAPI/Controllers/PricesController.cs`
- `HerreraSystem.Application/DTOs/PricesDtos/GeneralPriceDto.cs`
- `HerreraSystem.Application/Services/GeneralPriceService.cs`
- `HerreraSystem.Infrastructure/Repositories/GeneralPriceRepository.cs`
- `HerreraSystem.Infrastructure/Repositories/ProductPriceRepository.cs`
- `HerreraSystem.Infrastructure/Data/HerreraSystemContext.cs`
- `HerreraSystem.Domain/Entities/ProductPrice.cs`
- `HerreraSystem.Domain/Entities/PriceType.cs`
- `HerreraSystem.Domain/Entities/Product.cs`
- `HerreraSystem.Domain/Entities/LinePresentation.cs`

## 1. Resumen para Front-end

El modulo usa la tabla/entidad `ProductPrice` para dos conceptos distintos:

| Concepto | Como se identifica | Asignacion | Uso |
|---|---|---|---|
| Precio general | `ProductId == null` y `LinePresentationId != null` | Por combinacion linea-presentacion | Precio base compartido por todos los productos de esa linea y presentacion. |
| Precio especial | `ProductId != null` | Por producto especifico | Promociones, descuentos o precios temporales. |

Endpoints disponibles para precios:

| Operacion | Metodo | Ruta | Respuesta |
|---|---:|---|---|
| Listar vista agrupada de precios generales | GET | `/api/GeneralPrices/general` | `ApiResponse<GeneralPriceDto[]>` |
| Crear precio general | POST | `/api/GeneralPrices/general` | `ApiResponse<GeneralPriceDetailDto>` |
| Cambiar precio general con historial | PUT | `/api/GeneralPrices/general/{linePresentationId}` | `ApiResponse<GeneralPriceDetailDto>` |
| Obtener precios generales vigentes | GET | `/api/GeneralPrices/general/current` | `ApiResponse<GeneralPriceDetailDto[]>` |
| Obtener historial paginado | GET | `/api/GeneralPrices/general/history` | `ApiResponse<PagedResponse<GeneralPriceDetailDto>>` |
| Estadisticas del dashboard | GET | `/api/prices/statistics` | `ApiResponse<PriceStatisticsDto>` |

Estado actual importante:

- No existe endpoint publico para CRUD de precios especiales.
- No existe endpoint publico para listar o administrar `PriceTypes`.
- `PriceTypeConstants` define:
  - `1`: Detalle / Retail.
  - `2`: Mayoreo / Wholesale.
- En creacion de precios generales, la API fuerza siempre `PriceTypeId = 1`; el frontend no debe mostrar este campo como editable.
- Los endpoints actuales no tienen `[Authorize]`; JWT esta configurado globalmente, pero estas rutas no exigen token por atributo.
- Las respuestas controladas usan `ApiResponse<T>`.
- Las fechas se envian como strings ISO (`DateTime`).
- Los montos `decimal` se consumen como `number` en TypeScript.

## 2. Convenciones comunes

### 2.1 `ApiResponse<T>`

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
  "message": "El precio debe ser mayor que cero",
  "data": null
}
```

### 2.2 `PagedResponse<T>`

Usado por el historial.

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

## 3. Modelo de datos relevante

### 3.1 `ProductPrice`

Entidad base del modulo de precios.

| Propiedad | C# | TypeScript | Nullable | Uso |
|---|---:|---:|---:|---|
| `Id` | `int` | `number` | No | Identificador del precio. |
| `PriceTypeId` | `int` | `number` | No | Tipo de precio, por ejemplo detalle o mayoreo. |
| `LinePresentationId` | `int?` | `number \| null` | Si | Requerido para precios generales. |
| `ProductId` | `int?` | `number \| null` | Si | Requerido para precios especiales. |
| `Price` | `decimal` | `number` | No | Monto del precio. |
| `ValidFrom` | `DateTime` | `string` | No | Inicio de vigencia. |
| `ValidTo` | `DateTime?` | `string \| null` | Si | Fin de vigencia. `null` significa sin fecha final. |
| `IsActive` | `bool?` | `boolean \| null` | Si | Estado logico del registro. |
| `CreatedBy` | `int` | `number` | No | Usuario creador. |
| `CreatedAt` | `DateTime?` | `string \| null` | Si | Fecha de creacion. |

Regla principal:

- Precio vigente: `IsActive == true`, `ValidFrom <= now` y (`ValidTo == null` o `ValidTo >= now`).

### 3.2 `LinePresentation`

Representa la combinacion valida entre linea y presentacion.

| Propiedad | C# | TypeScript | Nullable |
|---|---:|---:|---:|
| `Id` | `int` | `number` | No |
| `LineId` | `int` | `number` | No |
| `PresentationId` | `int` | `number` | No |

Para crear precios generales el frontend debe enviar `linePresentationId`, no `lineId` ni `presentationId` por separado.

### 3.3 `PriceType`

| Propiedad | C# | TypeScript | Nullable |
|---|---:|---:|---:|
| `Id` | `int` | `number` | No |
| `PriceName` | `string` | `string` | No |
| `IsActive` | `bool?` | `boolean \| null` | Si |

Constantes conocidas en backend:

```ts
export const PRICE_TYPE = {
  retail: 1,
  wholesale: 2
} as const;
```

Advertencia: en el estado actual de la API no hay endpoint `GET /api/PriceTypes`. Si el frontend necesita un select dinamico, hay que exponer ese catalogo en backend o mantener una lista fija sincronizada con la base de datos.

## 4. DTOs del modulo

### 4.1 `GeneralPriceDto`

Usado por:

```http
GET /api/GeneralPrices/general
```

```ts
export interface GeneralPriceDto {
  linePresentationId: number;
  lineName: string;
  presentationName: string;
  retailPrice: number | null;
  wholesalePrice: number | null;
  productsCount: number;
}
```

Uso recomendado:

- Pantalla tipo matriz por linea-presentacion.
- Muestra precio detalle y mayoreo en una sola fila.
- `productsCount` ayuda a mostrar cuantos productos seran afectados por un cambio de precio general.

### 4.2 `CreateGeneralPriceDto`

Usado por:

```http
POST /api/GeneralPrices/general
```

```ts
export interface CreateGeneralPriceRequest {
  linePresentationId: number;
  priceTypeId?: number;
  price: number;
  validFrom: string;
  validTo?: string | null;
  createdBy: number;
}
```

Ejemplo:

```json
{
  "linePresentationId": 12,
  "price": 25,
  "validFrom": "2026-06-23T00:00:00",
  "validTo": null,
  "createdBy": 1
}
```

Validaciones backend:

- `linePresentationId` debe ser mayor que `0`.
- `priceTypeId` no es requerido para crear; si se envia, el backend lo reemplaza por `1`.
- `price` debe ser mayor que `0`.
- `validFrom` es requerido.
- `validTo` no puede ser menor que `validFrom`.
- La combinacion `LinePresentationId` debe existir.
- El `PriceTypeId = 1` debe existir y estar activo.
- No puede existir otro precio general activo solapado para la combinacion `linePresentationId + priceTypeId = 1`.

### 4.3 `ChangeGeneralPriceDto`

Usado por:

```http
PUT /api/GeneralPrices/general/{linePresentationId}
```

```ts
export interface ChangeGeneralPriceRequest {
  priceTypeId: number;
  price: number;
  validFrom: string;
  validTo?: string | null;
  createdBy: number;
}
```

Ejemplo:

```json
{
  "priceTypeId": 1,
  "price": 30,
  "validFrom": "2026-07-01T00:00:00",
  "validTo": null,
  "createdBy": 1
}
```

Regla de historial:

- La API busca el precio general vigente para `linePresentationId + priceTypeId`.
- Si existe, lo cierra:
  - `IsActive = false`
  - `ValidTo = nuevoValidFrom - 1 milisegundo`
- Luego crea un nuevo `ProductPrice` activo con el nuevo monto y vigencia.
- No se sobrescribe el registro anterior.

Notas para frontend:

- `validFrom` del nuevo precio debe ser posterior al `validFrom` del registro vigente.
- Si el backend detecta solapamiento con otro rango activo o no puede cerrar/crear, responde `400`.
- Para una experiencia clara, antes de cambiar precio conviene mostrar al usuario el precio vigente y advertir que el cambio conserva historial.

### 4.4 `GeneralPriceDetailDto`

Usado por:

- `POST /api/GeneralPrices/general`
- `PUT /api/GeneralPrices/general/{linePresentationId}`
- `GET /api/GeneralPrices/general/current`
- `GET /api/GeneralPrices/general/history`

```ts
export interface GeneralPriceDetailDto {
  id: number;
  linePresentationId: number;
  lineName: string;
  presentationName: string;
  priceTypeId: number;
  priceTypeName: string;
  price: number;
  validFrom: string;
  validTo: string | null;
  isActive: boolean;
  createdBy: number;
  createdAt: string | null;
}
```

Ejemplo:

```json
{
  "id": 101,
  "linePresentationId": 12,
  "lineName": "Tradicional",
  "presentationName": "4 Onzas",
  "priceTypeId": 1,
  "priceTypeName": "Detalle",
  "price": 25,
  "validFrom": "2026-06-23T00:00:00",
  "validTo": null,
  "isActive": true,
  "createdBy": 1,
  "createdAt": "2026-06-23T10:30:00"
}
```

### 4.5 `PriceStatisticsDto`

Usado por:

```http
GET /api/prices/statistics
```

```ts
export interface PriceStatisticsDto {
  productsWithPrice: number;
  activeSpecialPrices: number;
  promotionsExpiringSoon: number;
  lastUpdate: string | null;
}
```

Significado:

| Propiedad | Regla actual |
|---|---|
| `productsWithPrice` | Cantidad de productos activos que tienen precio vigente directo por producto o precio general vigente por su `LinePresentationId`. |
| `activeSpecialPrices` | Cantidad de `ProductPrices` con `ProductId != null`, activos y vigentes. |
| `promotionsExpiringSoon` | Cantidad de precios especiales activos/vigentes cuyo `ValidTo` vence dentro de los proximos 7 dias. |
| `lastUpdate` | Ultima fecha relevante del modulo, calculada desde `CreatedAt ?? ValidFrom` de `ProductPrices`. |

Respuesta esperada:

```json
{
  "success": true,
  "message": "Operacion exitosa",
  "data": {
    "productsWithPrice": 25,
    "activeSpecialPrices": 5,
    "promotionsExpiringSoon": 2,
    "lastUpdate": "2026-06-23T10:30:00"
  }
}
```

## 5. Endpoints

### 5.1 Listar vista agrupada de precios generales

```http
GET /api/GeneralPrices/general
```

Query params:

| Nombre | Tipo | Requerido | Descripcion |
|---|---:|---:|---|
| `lineId` | `number` | No | Filtra por linea. |

Ejemplo:

```http
GET /api/GeneralPrices/general?lineId=1
```

Respuesta:

```ts
ApiResponse<GeneralPriceDto[]>
```

Ejemplo:

```json
{
  "success": true,
  "message": "Operacion exitosa",
  "data": [
    {
      "linePresentationId": 12,
      "lineName": "Tradicional",
      "presentationName": "4 Onzas",
      "retailPrice": 25,
      "wholesalePrice": 20,
      "productsCount": 3
    }
  ]
}
```

Notas:

- Devuelve todas las combinaciones `LinePresentations`, aunque no tengan precio vigente.
- Si no hay precio vigente para detalle o mayoreo, `retailPrice` o `wholesalePrice` retorna `null`.
- Esta ruta es ideal para la tabla principal de administracion de precios.

### 5.2 Crear precio general

```http
POST /api/GeneralPrices/general
```

Body:

```ts
CreateGeneralPriceRequest
```

Respuesta exitosa:

- HTTP `201 Created`.
- Body: `ApiResponse<GeneralPriceDetailDto>`.
- Mensaje: `Precio general creado exitosamente`.

Errores controlados:

```http
400 Bad Request
```

```json
{
  "success": false,
  "message": "Ya existe un precio general activo para esa linea, presentacion, tipo de precio y rango de fechas",
  "data": null
}
```

Otros mensajes posibles:

- `La presentacion de linea es requerida`
- `El tipo de precio es requerido`
- `El precio debe ser mayor que cero`
- `La fecha inicial de vigencia es requerida`
- `La fecha final de vigencia no puede ser menor que la fecha inicial`
- `La combinacion de linea y presentacion con Id {id} no existe`
- `El tipo de precio con Id {id} no existe o no esta activo`

### 5.3 Cambiar precio general conservando historial

```http
PUT /api/GeneralPrices/general/{linePresentationId}
```

Route params:

| Nombre | Tipo | Requerido |
|---|---:|---:|
| `linePresentationId` | `number` | Si |

Body:

```ts
ChangeGeneralPriceRequest
```

Respuesta exitosa:

```json
{
  "success": true,
  "message": "Precio general actualizado exitosamente",
  "data": {
    "id": 102,
    "linePresentationId": 12,
    "lineName": "Tradicional",
    "presentationName": "4 Onzas",
    "priceTypeId": 1,
    "priceTypeName": "Detalle",
    "price": 30,
    "validFrom": "2026-07-01T00:00:00",
    "validTo": null,
    "isActive": true,
    "createdBy": 1,
    "createdAt": "2026-06-23T10:30:00"
  }
}
```

Error si no se puede crear el nuevo registro:

```json
{
  "success": false,
  "message": "No se pudo crear el nuevo precio general",
  "data": null
}
```

Uso recomendado:

- Usar este endpoint cuando el usuario edita un precio vigente.
- No usar `POST` para reemplazar un precio vigente, porque `POST` no cierra automaticamente el anterior.
- Mostrar confirmacion: "Se cerrara el precio vigente y se creara un nuevo registro historico".

### 5.4 Obtener precios generales vigentes

```http
GET /api/GeneralPrices/general/current
```

Query params:

| Nombre | Tipo | Requerido | Descripcion |
|---|---:|---:|---|
| `lineId` | `number` | No | Filtra por linea. |
| `priceTypeId` | `number` | No | Filtra por tipo de precio. |

Ejemplos:

```http
GET /api/GeneralPrices/general/current
GET /api/GeneralPrices/general/current?lineId=1
GET /api/GeneralPrices/general/current?priceTypeId=2
```

Respuesta:

```ts
ApiResponse<GeneralPriceDetailDto[]>
```

Orden actual:

1. `lineName` ascendente.
2. `presentationName` ascendente.
3. `priceTypeName` ascendente.

### 5.5 Obtener historial paginado de precios generales

```http
GET /api/GeneralPrices/general/history
```

Query params:

| Nombre | Tipo | Requerido | Descripcion |
|---|---:|---:|---|
| `linePresentationId` | `number` | No | Filtra historial de una combinacion. |
| `priceTypeId` | `number` | No | Filtra por tipo de precio. |
| `page` | `number` | No | Default `1`. |
| `pageSize` | `number` | No | Default `10`, maximo `50`. |

Ejemplo:

```http
GET /api/GeneralPrices/general/history?linePresentationId=12&priceTypeId=1&page=1&pageSize=10
```

Respuesta:

```ts
ApiResponse<PagedResponse<GeneralPriceDetailDto>>
```

Ejemplo:

```json
{
  "success": true,
  "message": "Operacion exitosa",
  "data": {
    "data": [
      {
        "id": 102,
        "linePresentationId": 12,
        "lineName": "Tradicional",
        "presentationName": "4 Onzas",
        "priceTypeId": 1,
        "priceTypeName": "Detalle",
        "price": 30,
        "validFrom": "2026-07-01T00:00:00",
        "validTo": null,
        "isActive": true,
        "createdBy": 1,
        "createdAt": "2026-06-23T10:30:00"
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

Orden actual:

1. `validFrom` descendente.
2. `id` descendente.

### 5.6 Estadisticas de precios

```http
GET /api/prices/statistics
```

No recibe parametros.

Respuesta:

```ts
ApiResponse<PriceStatisticsDto>
```

Ejemplo:

```json
{
  "success": true,
  "message": "Operacion exitosa",
  "data": {
    "productsWithPrice": 25,
    "activeSpecialPrices": 5,
    "promotionsExpiringSoon": 2,
    "lastUpdate": "2026-06-23T10:30:00"
  }
}
```

Uso recomendado:

- Cards superiores del dashboard de precios.
- Refrescar despues de crear o cambiar precios.
- Mostrar `lastUpdate` como fecha/hora local del usuario.

## 6. Contratos TypeScript completos

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

export interface GeneralPriceDto {
  linePresentationId: number;
  lineName: string;
  presentationName: string;
  retailPrice: number | null;
  wholesalePrice: number | null;
  productsCount: number;
}

export interface CreateGeneralPriceRequest {
  linePresentationId: number;
  priceTypeId?: number;
  price: number;
  validFrom: string;
  validTo?: string | null;
  createdBy: number;
}

export interface ChangeGeneralPriceRequest {
  priceTypeId: number;
  price: number;
  validFrom: string;
  validTo?: string | null;
  createdBy: number;
}

export interface GeneralPriceDetailDto {
  id: number;
  linePresentationId: number;
  lineName: string;
  presentationName: string;
  priceTypeId: number;
  priceTypeName: string;
  price: number;
  validFrom: string;
  validTo: string | null;
  isActive: boolean;
  createdBy: number;
  createdAt: string | null;
}

export interface PriceStatisticsDto {
  productsWithPrice: number;
  activeSpecialPrices: number;
  promotionsExpiringSoon: number;
  lastUpdate: string | null;
}

export interface GeneralPricesQuery {
  lineId?: number;
}

export interface CurrentGeneralPricesQuery {
  lineId?: number;
  priceTypeId?: number;
}

export interface GeneralPriceHistoryQuery {
  linePresentationId?: number;
  priceTypeId?: number;
  page?: number;
  pageSize?: number;
}

export const PRICE_TYPE = {
  retail: 1,
  wholesale: 2
} as const;

export type GeneralPricesResponse = ApiResponse<GeneralPriceDto[]>;
export type GeneralPriceDetailResponse = ApiResponse<GeneralPriceDetailDto>;
export type CurrentGeneralPricesResponse = ApiResponse<GeneralPriceDetailDto[]>;
export type GeneralPriceHistoryResponse = ApiResponse<PagedResponse<GeneralPriceDetailDto>>;
export type PriceStatisticsResponse = ApiResponse<PriceStatisticsDto>;
export type ValidationErrorResponse = ApiResponse<string[]>;
```

## 7. Cliente TypeScript sugerido

```ts
const GENERAL_PRICES_BASE = "/api/GeneralPrices";
const PRICES_BASE = "/api/prices";

async function parseJson<T>(response: Response): Promise<T> {
  return (await response.json()) as T;
}

function appendQuery(params: URLSearchParams, key: string, value: unknown) {
  if (value !== undefined && value !== null && value !== "") {
    params.set(key, String(value));
  }
}

export async function getGeneralPrices(query: GeneralPricesQuery = {}) {
  const params = new URLSearchParams();
  appendQuery(params, "lineId", query.lineId);

  const url = `${GENERAL_PRICES_BASE}/general${params.size ? `?${params}` : ""}`;
  const res = await fetch(url);
  return parseJson<GeneralPricesResponse>(res);
}

export async function createGeneralPrice(payload: CreateGeneralPriceRequest) {
  const res = await fetch(`${GENERAL_PRICES_BASE}/general`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload)
  });

  return parseJson<GeneralPriceDetailResponse | ValidationErrorResponse>(res);
}

export async function changeGeneralPrice(
  linePresentationId: number,
  payload: ChangeGeneralPriceRequest
) {
  const res = await fetch(`${GENERAL_PRICES_BASE}/general/${linePresentationId}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload)
  });

  return parseJson<GeneralPriceDetailResponse | ValidationErrorResponse>(res);
}

export async function getCurrentGeneralPrices(query: CurrentGeneralPricesQuery = {}) {
  const params = new URLSearchParams();
  appendQuery(params, "lineId", query.lineId);
  appendQuery(params, "priceTypeId", query.priceTypeId);

  const url = `${GENERAL_PRICES_BASE}/general/current${params.size ? `?${params}` : ""}`;
  const res = await fetch(url);
  return parseJson<CurrentGeneralPricesResponse>(res);
}

export async function getGeneralPriceHistory(query: GeneralPriceHistoryQuery = {}) {
  const params = new URLSearchParams();
  appendQuery(params, "linePresentationId", query.linePresentationId);
  appendQuery(params, "priceTypeId", query.priceTypeId);
  appendQuery(params, "page", query.page);
  appendQuery(params, "pageSize", query.pageSize);

  const url = `${GENERAL_PRICES_BASE}/general/history${params.size ? `?${params}` : ""}`;
  const res = await fetch(url);
  return parseJson<GeneralPriceHistoryResponse>(res);
}

export async function getPriceStatistics() {
  const res = await fetch(`${PRICES_BASE}/statistics`);
  return parseJson<PriceStatisticsResponse>(res);
}
```

## 8. Flujos recomendados de UI

### 8.1 Dashboard del modulo

1. Al cargar la pantalla, llamar:

```http
GET /api/prices/statistics
```

2. Mostrar cards:
   - `productsWithPrice`: productos activos con precio vigente.
   - `activeSpecialPrices`: precios especiales activos.
   - `promotionsExpiringSoon`: promociones por vencer en 7 dias.
   - `lastUpdate`: ultima actualizacion.

3. Si `lastUpdate` viene `null`, mostrar estado vacio como "Sin actualizaciones".

### 8.2 Tabla principal de precios generales

1. Cargar:

```http
GET /api/GeneralPrices/general
```

2. Para tabs/filtro por linea:

```http
GET /api/GeneralPrices/general?lineId={lineId}
```

3. Columnas sugeridas:

| Columna | Campo |
|---|---|
| Linea | `lineName` |
| Presentacion | `presentationName` |
| Precio detalle | `retailPrice` |
| Precio mayoreo | `wholesalePrice` |
| Productos afectados | `productsCount` |

4. Si `retailPrice` viene `null`, mostrar accion "Crear precio"; la creacion registra siempre `PriceTypeId = 1`.
5. Si ya existe precio, mostrar accion "Cambiar precio" para preservar historial.

### 8.3 Crear precio general

Datos necesarios:

- `linePresentationId`: viene de la fila de `GET /api/GeneralPrices/general` o de `GET /api/LinePresentations`.
- `priceTypeId`: no pedirlo en el formulario de creacion; la API usa siempre `1`.
- `price`: monto positivo.
- `validFrom`: fecha inicial.
- `validTo`: opcional.
- `createdBy`: usuario actual.

Validaciones frontend recomendadas:

- `linePresentationId > 0`.
- `price > 0`.
- `validFrom` requerido.
- Si existe `validTo`, debe ser mayor o igual que `validFrom`.

### 8.4 Cambiar precio general

Usar:

```http
PUT /api/GeneralPrices/general/{linePresentationId}
```

Flujo sugerido:

1. Abrir modal desde una fila de precios.
2. El usuario elige si cambia detalle o mayoreo.
3. Precargar `priceTypeId` segun la columna.
4. Pedir nuevo precio y fecha de inicio.
5. Mostrar texto de confirmacion: "El precio vigente se cerrara y el nuevo precio quedara registrado en el historial".
6. Enviar `ChangeGeneralPriceRequest`.
7. Refrescar tabla principal, historial y estadisticas.

### 8.5 Historial

Usar:

```http
GET /api/GeneralPrices/general/history?linePresentationId={id}&priceTypeId={id}&page=1&pageSize=10
```

Columnas sugeridas:

| Columna | Campo |
|---|---|
| Tipo | `priceTypeName` |
| Precio | `price` |
| Vigente desde | `validFrom` |
| Vigente hasta | `validTo` |
| Estado | `isActive` |
| Creado por | `createdBy` |
| Creado el | `createdAt` |

Estados UI:

- `isActive === true` y `validTo === null`: vigente sin fin.
- `isActive === true` y `validTo !== null`: vigente hasta fecha.
- `isActive === false`: historico/cerrado.

## 9. Formato de fechas y moneda

Recomendaciones:

- Enviar fechas en formato ISO:

```ts
const validFrom = new Date(form.validFrom).toISOString();
```

- Si el usuario selecciona solo fecha en un `<input type="date">`, considerar hora local/UTC con cuidado. Para evitar sorpresas, definir una convencion en frontend, por ejemplo enviar `YYYY-MM-DDT00:00:00`.
- Mostrar moneda como cordobas si el sistema opera en C$:

```ts
export function formatCurrency(value: number | null) {
  if (value === null) return "Sin precio";

  return new Intl.NumberFormat("es-NI", {
    style: "currency",
    currency: "NIO"
  }).format(value);
}
```

## 10. Manejo de errores recomendado

```ts
export function getApiErrorMessage(response: unknown): string {
  const value = response as {
    message?: string;
    data?: unknown;
    Message?: string;
    Data?: unknown;
  };

  const data = value.data ?? value.Data;
  const message = value.message ?? value.Message;

  if (Array.isArray(data)) return data.join("\n");
  if (typeof message === "string" && message.length > 0) return message;

  return "Ocurrio un error procesando la solicitud.";
}
```

Errores que la UI debe manejar:

| Caso | Codigo esperado | Accion UI |
|---|---:|---|
| Precio menor o igual a cero | 400 | Mostrar validacion junto al input de precio. |
| Fecha final menor que inicial | 400 | Marcar campos de fecha. |
| `LinePresentation` inexistente | 400 | Refrescar catalogos y pedir reintento. |
| `PriceType` inexistente/inactivo | 400 | Revisar catalogo fijo o backend. |
| Rango solapado | 400 | Mostrar mensaje y sugerir revisar historial. |
| Error no controlado | 500 | Mostrar mensaje generico y permitir reintento. |

## 11. Checklist frontend

- Usar `GET /api/GeneralPrices/general` para la pantalla principal.
- Usar `linePresentationId`, no `productId`, para precios generales.
- Usar `POST /api/GeneralPrices/general` solo para crear un precio nuevo sin vigente solapado.
- Usar `PUT /api/GeneralPrices/general/{linePresentationId}` para cambiar un precio existente preservando historial.
- Usar `GET /api/GeneralPrices/general/current` cuando se necesite solo precios vigentes.
- Usar `GET /api/GeneralPrices/general/history` para auditoria/historial.
- Usar `GET /api/prices/statistics` para cards del dashboard.
- Mantener `PRICE_TYPE.retail = 1` y `PRICE_TYPE.wholesale = 2` mientras no exista endpoint de `PriceTypes`.
- Validar precio positivo y fechas antes de enviar.
- Despues de crear/cambiar, refrescar tabla, historial y estadisticas.
- No construir precios generales por `productId`; todos los productos de la misma `LinePresentation` comparten precio base.
