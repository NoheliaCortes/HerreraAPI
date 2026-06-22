# Documentacion tecnica Front-end - Catalogos Lines, Presentations, LinePresentations y Flavors

Este documento describe los contratos reales disponibles en el backend para consumir los catalogos necesarios en formularios de productos: lineas, presentaciones, relaciones linea-presentacion y sabores.

Fuentes revisadas:

- `HerreraSystemAPI/Controllers/LinesController.cs`
- `HerreraSystemAPI/Controllers/PresentationsController.cs`
- `HerreraSystemAPI/Controllers/LinePresentationsController.cs`
- `HerreraSystemAPI/Controllers/FlavorsController.cs`
- `HerreraSystem.Application/DTOs/LineDtos/*.cs`
- `HerreraSystem.Application/DTOs/PresentationDtos/*.cs`
- `HerreraSystem.Application/DTOs/LinePresentationDtos/*.cs`
- `HerreraSystem.Application/DTOs/FlavorDtos/*.cs`
- `HerreraSystem.Infrastructure/Repositories/LineRepository.cs`
- `HerreraSystem.Infrastructure/Repositories/PresentationRepository.cs`
- `HerreraSystem.Infrastructure/Repositories/LinePresentationRepository.cs`
- `HerreraSystem.Infrastructure/Repositories/FlavorRepository.cs`
- `HerreraSystem.Application/Services/FlavorService.cs`

## 1. Resumen para Front-end

Endpoints principales:

| Catalogo | Metodo | Ruta | Paginado | Uso principal |
|---|---:|---|---:|---|
| Lineas | GET | `/api/Lines` | No | Select de linea. |
| Presentaciones | GET | `/api/Presentations` | No | Select de presentacion. |
| Relaciones linea-presentacion | GET | `/api/LinePresentations` | No | Obtener combinaciones validas y sus IDs. |
| Presentaciones por linea | GET | `/api/Lines/{lineId}/presentations` | No | Presentaciones disponibles para una linea. Ver nota de riesgo. |
| Sabores | GET | `/api/Flavors?page=1&pageSize=50` | Si | Select de sabor. |

Endpoints CRUD administrativos:

| Recurso | Crear | Editar | Eliminar |
|---|---|---|---|
| Lines | `POST /api/Lines` | `PUT /api/Lines/{id}` | `DELETE /api/Lines/{id}` |
| Presentations | `POST /api/Presentations` | `PUT /api/Presentations/{id}` | `DELETE /api/Presentations/{id}` |
| LinePresentations | `POST /api/LinePresentations` | No existe | `DELETE /api/LinePresentations/{id}` |
| Flavors | `POST /api/Flavors` | `PUT /api/Flavors/{id}` | `DELETE /api/Flavors/{id}` |

Nota importante:

- Para crear o editar productos, el campo que necesita `Product.LinePresentationId` viene de `GET /api/LinePresentations`, no de `GET /api/Lines` ni de `GET /api/Presentations` por separado.
- `GET /api/Lines/{lineId}/presentations` existe, pero el controlador instancia manualmente `LinePresentationRepository` con un `HerreraSystemContext` sin opciones de conexion. Puede fallar en runtime. Para frontend, la ruta mas segura es cargar `GET /api/LinePresentations` y filtrar por `line.id` en cliente, salvo que el backend corrija esa inyeccion.

## 2. Convenciones de respuesta

### 2.1 `ApiResponse<T>`

Todas las respuestas controladas usan:

```ts
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
}
```

Ejemplo:

```json
{
  "success": true,
  "message": "Operación exitosa",
  "data": []
}
```

### 2.2 `PagedResponse<T>`

Solo `Flavors` usa paginacion.

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
|---|---|---:|---|
| `page` | `number` | `1` | Sin validacion explicita. |
| `pageSize` | `number` | `10` | Maximo aplicado por backend: `50`. |

## 3. Lines

### 3.1 DTOs

#### `LineDto`

```ts
export interface LineDto {
  id: number;
  lineName: string;
  isActive: boolean | null;
}
```

| Propiedad C# | Tipo C# | Tipo TS | Nullable |
|---|---|---|---:|
| `Id` | `int` | `number` | No |
| `LineName` | `string` | `string` | No |
| `IsActive` | `bool?` | `boolean \| null` | Si |

#### `CreateLineDto`

```ts
export interface CreateLineRequest {
  lineName: string;
}
```

Validaciones:

| Propiedad | Reglas |
|---|---|
| `LineName` | `[Required(ErrorMessage = "El nombre de la línea es obligatorio")]`, `[StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]` |

#### `UpdateLineDto`

```ts
export interface UpdateLineRequest {
  lineName: string;
  isActive?: boolean | null;
}
```

Validaciones:

| Propiedad | Reglas |
|---|---|
| `LineName` | Requerido, maximo 100 caracteres. |
| `IsActive` | Opcional/null. |

### 3.2 Endpoints

#### Listar lineas

```http
GET /api/Lines
```

Respuesta:

```json
{
  "success": true,
  "message": "Operación exitosa",
  "data": [
    {
      "id": 1,
      "lineName": "Helados",
      "isActive": true
    }
  ]
}
```

No hay paginacion ni filtro de activos. El frontend debe filtrar `isActive === true` si solo quiere mostrar opciones activas.

#### Obtener linea por ID

```http
GET /api/Lines/{id}
```

Exito:

```json
{
  "success": true,
  "message": "Operación exitosa",
  "data": {
    "id": 1,
    "lineName": "Helados",
    "isActive": true
  }
}
```

No encontrada:

```http
404 Not Found
```

```json
{
  "success": false,
  "message": "Línea con Id 1 no encontrada",
  "data": null
}
```

#### Crear linea

```http
POST /api/Lines
```

Body:

```json
{
  "lineName": "Helados"
}
```

Exito:

```http
201 Created
```

```json
{
  "success": true,
  "message": "Línea creada exitosamente",
  "data": {
    "id": 1,
    "lineName": "Helados",
    "isActive": true
  }
}
```

Nota: no hay validacion de duplicados en el repositorio actual.

#### Editar linea

```http
PUT /api/Lines/{id}
```

Body:

```json
{
  "lineName": "Helados Premium",
  "isActive": true
}
```

Exito:

```json
{
  "success": true,
  "message": "Línea actualizada exitosamente",
  "data": null
}
```

#### Eliminar linea

```http
DELETE /api/Lines/{id}
```

Exito:

```json
{
  "success": true,
  "message": "Línea eliminada exitosamente",
  "data": null
}
```

Riesgo: la eliminacion es fisica. Si existen relaciones `LinePresentations` o productos relacionados, SQL Server puede lanzar error de FK y la API respondera `500`.

## 4. Presentations

### 4.1 DTOs

#### `PresentationDto`

```ts
export interface PresentationDto {
  id: number;
  presentationName: string;
  isActive: boolean | null;
}
```

#### `CreatePresentationDto`

```ts
export interface CreatePresentationRequest {
  presentationName: string;
}
```

Validaciones:

| Propiedad | Reglas |
|---|---|
| `PresentationName` | `[Required(ErrorMessage = "El nombre de la presentación es obligatorio")]`, `[StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]` |

#### `UpdatePresentationDto`

```ts
export interface UpdatePresentationRequest {
  presentationName: string;
  isActive?: boolean | null;
}
```

### 4.2 Endpoints

#### Listar presentaciones

```http
GET /api/Presentations
```

Respuesta:

```json
{
  "success": true,
  "message": "Operación exitosa",
  "data": [
    {
      "id": 1,
      "presentationName": "1 Litro",
      "isActive": true
    }
  ]
}
```

No hay paginacion ni filtro de activos.

#### Obtener presentacion por ID

```http
GET /api/Presentations/{id}
```

No encontrada:

```json
{
  "success": false,
  "message": "Presentación con Id 1 no encontrada",
  "data": null
}
```

#### Crear presentacion

```http
POST /api/Presentations
```

Body:

```json
{
  "presentationName": "1 Litro"
}
```

Exito:

```json
{
  "success": true,
  "message": "Presentación creada exitosamente",
  "data": {
    "id": 1,
    "presentationName": "1 Litro",
    "isActive": true
  }
}
```

Nota: no hay validacion de duplicados en el repositorio actual.

#### Editar presentacion

```http
PUT /api/Presentations/{id}
```

Body:

```json
{
  "presentationName": "500 ml",
  "isActive": true
}
```

#### Eliminar presentacion

```http
DELETE /api/Presentations/{id}
```

Riesgo: eliminacion fisica. Puede fallar con `500` si hay relaciones o productos/precios asociados.

## 5. LinePresentations

Este catalogo representa la combinacion valida entre una linea y una presentacion. Es clave para productos, porque `Product.LinePresentationId` apunta a esta entidad.

### 5.1 DTOs

#### `LinePresentationDto`

```ts
export interface LinePresentationDto {
  id: number;
  line: LineReferenceDto;
  presentation: PresentationReferenceDto;
}

export interface LineReferenceDto {
  id: number;
  name: string;
}

export interface PresentationReferenceDto {
  id: number;
  name: string;
}
```

Ejemplo:

```json
{
  "id": 12,
  "line": {
    "id": 1,
    "name": "Helados"
  },
  "presentation": {
    "id": 3,
    "name": "1 Litro"
  }
}
```

#### `CreateLinePresentationDto`

```ts
export interface CreateLinePresentationRequest {
  lineId: number;
  presentationId: number;
}
```

Validaciones:

| Propiedad | Reglas |
|---|---|
| `LineId` | `[Required(ErrorMessage = "La línea es obligatoria")]` |
| `PresentationId` | `[Required(ErrorMessage = "La presentación es obligatoria")]` |

Nota de model binding: al ser `int`, si se omite puede llegar como `0`. El frontend debe validar IDs positivos antes de enviar.

### 5.2 Endpoints

#### Listar relaciones

```http
GET /api/LinePresentations
```

Respuesta:

```json
{
  "success": true,
  "message": "Operación exitosa",
  "data": [
    {
      "id": 12,
      "line": {
        "id": 1,
        "name": "Helados"
      },
      "presentation": {
        "id": 3,
        "name": "1 Litro"
      }
    }
  ]
}
```

Uso recomendado en formulario de producto:

- Mostrar primero select de `line`.
- Al seleccionar una linea, filtrar relaciones donde `relation.line.id === selectedLineId`.
- Mostrar select de presentacion usando `relation.presentation`.
- Guardar en producto el `relation.id` como `linePresentationId`.

#### Obtener relacion por ID

```http
GET /api/LinePresentations/{id}
```

No encontrada:

```json
{
  "success": false,
  "message": "Relación con Id 12 no encontrada",
  "data": null
}
```

#### Crear relacion

```http
POST /api/LinePresentations
```

Body:

```json
{
  "lineId": 1,
  "presentationId": 3
}
```

Exito:

```http
201 Created
```

```json
{
  "success": true,
  "message": "Relación creada exitosamente",
  "data": {
    "id": 12,
    "line": {
      "id": 1,
      "name": "Helados"
    },
    "presentation": {
      "id": 3,
      "name": "1 Litro"
    }
  }
}
```

Error de negocio:

```http
400 Bad Request
```

```json
{
  "success": false,
  "message": "La línea o presentación no existe, o la combinación ya está registrada",
  "data": null
}
```

#### Eliminar relacion

```http
DELETE /api/LinePresentations/{id}
```

Exito:

```json
{
  "success": true,
  "message": "Relación eliminada exitosamente",
  "data": null
}
```

Riesgo: si existen productos o precios usando la relacion, la eliminacion puede fallar por FK con `500`.

### 5.3 Presentaciones por linea

```http
GET /api/Lines/{lineId}/presentations
```

Respuesta esperada:

```json
{
  "success": true,
  "message": "Operación exitosa",
  "data": [
    {
      "id": 3,
      "presentationName": "1 Litro",
      "isActive": true
    }
  ]
}
```

Advertencia tecnica:

- El action existe, pero en `LinesController` el repositorio se instancia manualmente con `new HerreraSystemContext()` en lugar de inyeccion de dependencias.
- En un entorno real puede fallar porque ese contexto no tiene `DbContextOptions` ni connection string.
- Para frontend productivo, preferir `GET /api/LinePresentations` y filtrar en cliente.

## 6. Flavors

### 6.1 DTOs

#### `FlavorDto`

```ts
export interface FlavorDto {
  id: number;
  flavorName: string;
  isActive: boolean | null;
  imageUrl: string | null;
  flavorColor: string | null;
}
```

| Propiedad C# | Tipo C# | Tipo TS | Nullable |
|---|---|---|---:|
| `Id` | `int` | `number` | No |
| `FlavorName` | `string` | `string` | No |
| `IsActive` | `bool?` | `boolean \| null` | Si |
| `ImageUrl` | `string?` | `string \| null` | Si |
| `FlavorColor` | `string?` | `string \| null` | Si |

#### `CreateFlavorDto`

```ts
export interface CreateFlavorRequest {
  flavorName: string;
  imageUrl?: string | null;
  flavorColor?: string | null;
}
```

Validaciones:

| Propiedad | Reglas |
|---|---|
| `FlavorName` | `[Required(ErrorMessage = "El nombre del sabor es obligatorio")]`, `[StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]` |
| `ImageUrl` | `[Url(ErrorMessage = "La URL de la imagen no es válida")]` |
| `FlavorColor` | `[StringLength(7, ErrorMessage = "El color debe ser un código hex válido")]` |

#### `UpdateFlavorDto`

```ts
export interface UpdateFlavorRequest {
  flavorName: string;
  isActive?: boolean | null;
  imageUrl?: string | null;
  flavorColor?: string | null;
}
```

Validaciones:

| Propiedad | Reglas |
|---|---|
| `FlavorName` | Requerido, maximo 100 caracteres. |
| `ImageUrl` | URL valida si se envia. |
| `FlavorColor` | `[RegularExpression("^#([A-Fa-f0-9]{6})$", ErrorMessage = "El color debe ser un código HEX válido")]` |

Diferencia importante:

- Crear sabor solo valida longitud 7 para `FlavorColor`.
- Editar sabor exige formato HEX exacto `#RRGGBB`.
- Para evitar inconsistencias, el frontend deberia validar siempre `^#([A-Fa-f0-9]{6})$`.

### 6.2 Endpoints

#### Listar sabores

```http
GET /api/Flavors?page=1&pageSize=50
```

Respuesta:

```json
{
  "success": true,
  "message": "Operación exitosa",
  "data": {
    "data": [
      {
        "id": 1,
        "flavorName": "Fresa",
        "isActive": true,
        "imageUrl": "/uploads/products/fresa.webp",
        "flavorColor": "#FF4F79"
      }
    ],
    "currentPage": 1,
    "pageSize": 50,
    "totalRecords": 1,
    "totalPages": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
  }
}
```

Ordenamiento:

- El backend ordena por `FlavorName` ascendente.

Para selects:

- Usar `pageSize=50`.
- Si hay mas de 50 sabores, consumir paginas siguientes.
- Filtrar en frontend `isActive === true` si se requiere mostrar solo activos.

#### Obtener sabor por ID

```http
GET /api/Flavors/{id}
```

No encontrado:

```json
{
  "success": false,
  "message": "Sabor con Id 1 no encontrado",
  "data": null
}
```

#### Crear sabor

```http
POST /api/Flavors
```

Body:

```json
{
  "flavorName": "Fresa",
  "imageUrl": "/uploads/products/fresa.webp",
  "flavorColor": "#FF4F79"
}
```

Exito:

```http
201 Created
```

```json
{
  "success": true,
  "message": "Sabor creado exitosamente",
  "data": {
    "id": 1,
    "flavorName": "Fresa",
    "isActive": true,
    "imageUrl": "/uploads/products/fresa.webp",
    "flavorColor": "#FF4F79"
  }
}
```

Duplicado:

```http
400 Bad Request
```

```json
{
  "success": false,
  "message": "Ya existe un sabor con ese nombre",
  "data": null
}
```

#### Editar sabor

```http
PUT /api/Flavors/{id}
```

Body:

```json
{
  "flavorName": "Fresa Premium",
  "isActive": true,
  "imageUrl": "/uploads/products/fresa.webp",
  "flavorColor": "#FF4F79"
}
```

Exito:

```json
{
  "success": true,
  "message": "Sabor actualizado exitosamente",
  "data": null
}
```

#### Eliminar sabor

```http
DELETE /api/Flavors/{id}
```

Errores de negocio:

```json
{
  "success": false,
  "message": "No se puede eliminar el sabor porque tiene productos asociados",
  "data": null
}
```

## 7. Tipos TypeScript completos

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

export interface LineDto {
  id: number;
  lineName: string;
  isActive: boolean | null;
}

export interface CreateLineRequest {
  lineName: string;
}

export interface UpdateLineRequest {
  lineName: string;
  isActive?: boolean | null;
}

export interface PresentationDto {
  id: number;
  presentationName: string;
  isActive: boolean | null;
}

export interface CreatePresentationRequest {
  presentationName: string;
}

export interface UpdatePresentationRequest {
  presentationName: string;
  isActive?: boolean | null;
}

export interface LineReferenceDto {
  id: number;
  name: string;
}

export interface PresentationReferenceDto {
  id: number;
  name: string;
}

export interface LinePresentationDto {
  id: number;
  line: LineReferenceDto;
  presentation: PresentationReferenceDto;
}

export interface CreateLinePresentationRequest {
  lineId: number;
  presentationId: number;
}

export interface FlavorDto {
  id: number;
  flavorName: string;
  isActive: boolean | null;
  imageUrl: string | null;
  flavorColor: string | null;
}

export interface CreateFlavorRequest {
  flavorName: string;
  imageUrl?: string | null;
  flavorColor?: string | null;
}

export interface UpdateFlavorRequest {
  flavorName: string;
  isActive?: boolean | null;
  imageUrl?: string | null;
  flavorColor?: string | null;
}

export interface PaginationQuery {
  page?: number;
  pageSize?: number;
}

export type LinesResponse = ApiResponse<LineDto[]>;
export type LineResponse = ApiResponse<LineDto>;
export type PresentationsResponse = ApiResponse<PresentationDto[]>;
export type PresentationResponse = ApiResponse<PresentationDto>;
export type LinePresentationsResponse = ApiResponse<LinePresentationDto[]>;
export type LinePresentationResponse = ApiResponse<LinePresentationDto>;
export type FlavorsResponse = ApiResponse<PagedResponse<FlavorDto>>;
export type FlavorResponse = ApiResponse<FlavorDto>;
export type MutationResponse = ApiResponse<null>;
export type ValidationErrorResponse = ApiResponse<string[]>;
```

## 8. Cliente TypeScript sugerido

```ts
const API_BASE = "";

async function parseJson<T>(response: Response): Promise<T> {
  return (await response.json()) as T;
}

export async function getLines() {
  const res = await fetch(`${API_BASE}/api/Lines`);
  return parseJson<LinesResponse>(res);
}

export async function getLineById(id: number) {
  const res = await fetch(`${API_BASE}/api/Lines/${id}`);
  return parseJson<LineResponse>(res);
}

export async function createLine(payload: CreateLineRequest) {
  const res = await fetch(`${API_BASE}/api/Lines`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload)
  });
  return parseJson<LineResponse | ValidationErrorResponse>(res);
}

export async function updateLine(id: number, payload: UpdateLineRequest) {
  const res = await fetch(`${API_BASE}/api/Lines/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload)
  });
  return parseJson<MutationResponse | ValidationErrorResponse>(res);
}

export async function deleteLine(id: number) {
  const res = await fetch(`${API_BASE}/api/Lines/${id}`, { method: "DELETE" });
  return parseJson<MutationResponse>(res);
}

export async function getPresentations() {
  const res = await fetch(`${API_BASE}/api/Presentations`);
  return parseJson<PresentationsResponse>(res);
}

export async function createPresentation(payload: CreatePresentationRequest) {
  const res = await fetch(`${API_BASE}/api/Presentations`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload)
  });
  return parseJson<PresentationResponse | ValidationErrorResponse>(res);
}

export async function updatePresentation(id: number, payload: UpdatePresentationRequest) {
  const res = await fetch(`${API_BASE}/api/Presentations/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload)
  });
  return parseJson<MutationResponse | ValidationErrorResponse>(res);
}

export async function deletePresentation(id: number) {
  const res = await fetch(`${API_BASE}/api/Presentations/${id}`, { method: "DELETE" });
  return parseJson<MutationResponse>(res);
}

export async function getLinePresentations() {
  const res = await fetch(`${API_BASE}/api/LinePresentations`);
  return parseJson<LinePresentationsResponse>(res);
}

export async function createLinePresentation(payload: CreateLinePresentationRequest) {
  const res = await fetch(`${API_BASE}/api/LinePresentations`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload)
  });
  return parseJson<LinePresentationResponse | ValidationErrorResponse>(res);
}

export async function deleteLinePresentation(id: number) {
  const res = await fetch(`${API_BASE}/api/LinePresentations/${id}`, { method: "DELETE" });
  return parseJson<MutationResponse>(res);
}

export async function getFlavors(query: PaginationQuery = { page: 1, pageSize: 50 }) {
  const params = new URLSearchParams();
  if (query.page !== undefined) params.set("page", String(query.page));
  if (query.pageSize !== undefined) params.set("pageSize", String(query.pageSize));

  const res = await fetch(`${API_BASE}/api/Flavors?${params.toString()}`);
  return parseJson<FlavorsResponse>(res);
}

export async function createFlavor(payload: CreateFlavorRequest) {
  const res = await fetch(`${API_BASE}/api/Flavors`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload)
  });
  return parseJson<FlavorResponse | ValidationErrorResponse>(res);
}

export async function updateFlavor(id: number, payload: UpdateFlavorRequest) {
  const res = await fetch(`${API_BASE}/api/Flavors/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload)
  });
  return parseJson<MutationResponse | ValidationErrorResponse>(res);
}

export async function deleteFlavor(id: number) {
  const res = await fetch(`${API_BASE}/api/Flavors/${id}`, { method: "DELETE" });
  return parseJson<MutationResponse>(res);
}
```

## 9. Construccion de selects para formulario de producto

Flujo recomendado:

1. Cargar relaciones:

```ts
const linePresentationsResult = await getLinePresentations();
const relations = linePresentationsResult.data ?? [];
```

2. Construir select de lineas desde relaciones para mostrar solo lineas con presentaciones configuradas:

```ts
const lineOptions = Array.from(
  new Map(relations.map((item) => [item.line.id, item.line])).values()
);
```

3. Cuando el usuario seleccione una linea, construir presentaciones disponibles:

```ts
function getPresentationOptionsByLine(lineId: number, relations: LinePresentationDto[]) {
  return relations
    .filter((item) => item.line.id === lineId)
    .map((item) => ({
      linePresentationId: item.id,
      presentationId: item.presentation.id,
      presentationName: item.presentation.name
    }));
}
```

4. Al seleccionar una presentacion, guardar el `linePresentationId` seleccionado en el payload del producto:

```ts
const productPayload = {
  linePresentationId: selectedLinePresentationId,
  flavorId: selectedFlavorId,
  productName,
  createdBy,
  imageUrl,
  minimumStock
};
```

5. Cargar sabores:

```ts
const flavorsResult = await getFlavors({ page: 1, pageSize: 50 });
const flavorOptions = (flavorsResult.data?.data ?? []).filter((flavor) => flavor.isActive === true);
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

## 11. Checklist frontend

- Para producto, obtener `linePresentationId` desde `GET /api/LinePresentations`.
- No enviar `lineId` ni `presentationId` al crear producto; producto espera `linePresentationId`.
- Para sabores, consumir `GET /api/Flavors?page=1&pageSize=50`.
- Filtrar `isActive === true` en frontend para selects si se quiere ocultar inactivos.
- Validar nombres requeridos y maximo 100 caracteres en lineas, presentaciones y sabores.
- Validar color de sabor como `#RRGGBB`.
- Evitar borrar lineas, presentaciones o relaciones si ya estan vinculadas a productos; el backend puede responder `500` por restricciones FK.
- Preferir desactivar (`isActive: false`) en Lines, Presentations y Flavors cuando haya historial.
- Recordar que `LinePresentations` no tiene update; para cambiar una combinacion se elimina y se crea otra.
