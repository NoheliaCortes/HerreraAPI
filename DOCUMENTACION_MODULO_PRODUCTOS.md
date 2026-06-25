# Documentacion tecnica - Modulo de Productos

Este documento describe el contrato real del modulo de productos del proyecto `HerreraSystemAPI`, construido a partir del codigo fuente de controladores, DTOs, servicios, repositorios y configuracion de Entity Framework.

Fuentes principales revisadas:

- `HerreraSystemAPI/Controllers/ProductsController.cs`
- `HerreraSystem.Application/DTOs/ProductDtos/*.cs`
- `HerreraSystem.Application/Services/ProductService.cs`
- `HerreraSystem.Infrastructure/Repositories/ProductRepository.cs`
- `HerreraSystem.Infrastructure/Data/HerreraSystemContext.cs`
- `HerreraSystemDomain/Entities/Product.cs`
- `HerreraSystemAPI/Program.cs`
- `HerreraSystemAPI/Middleware/ExceptionMiddleware.cs`

## Zona horaria operativa

Las fechas generadas automaticamente por la API para operaciones del sistema usan hora local de Nicaragua (`America/Managua`, UTC-06:00). En Windows se resuelve mediante `Central America Standard Time`. Para productos, `CreatedAt` se registra con esta hora local centralizada.

## 1. Resumen ejecutivo para Front-end

Base path del controlador:

```http
/api/Products
```

Endpoints disponibles:

| Operacion | Metodo | Ruta | Autenticacion actual |
|---|---:|---|---|
| Listar productos basicos | GET | `/api/Products` | No requerida |
| Listar catalogo enriquecido | GET | `/api/Products/catalog` | No requerida |
| Listar productos activos por linea-presentacion | GET | `/api/Products/by-line-presentation` | No requerida |
| Obtener estadisticas de productos | GET | `/api/Products/stats` | No requerida |
| Obtener producto por ID | GET | `/api/Products/{id}` | No requerida |
| Subir imagen de producto | POST | `/api/uploads/products` | No requerida |
| Crear producto | POST | `/api/Products` | JWT Bearer requerido |
| Actualizar producto completo | PUT | `/api/Products/{id}` | JWT Bearer requerido |
| Actualizar producto parcialmente | PATCH | `/api/Products/{id}` | JWT Bearer requerido |
| Eliminar producto | DELETE | `/api/Products/{id}` | JWT Bearer requerido |

Notas importantes:

- Existe `PUT` para procesar formularios de edicion completa y `PATCH` para actualizaciones parciales.
- El campo `ImageUrl` ya no debe escribirse manualmente en el frontend. Primero se sube el archivo a `POST /api/uploads/products`; la API devuelve una URL relativa y esa URL se guarda luego en `ImageUrl`.
- El campo `CreatedBy` ya no se recibe desde el frontend al crear productos. La API lo toma del JWT mediante `ICurrentUserService`.
- Todas las respuestas exitosas/controladas estan envueltas en `ApiResponse<T>`.
- Los listados devuelven `ApiResponse<PagedResponse<T>>`.
- `POST`, `PUT`, `PATCH` y `DELETE` de productos tienen `[Authorize]`. Los `GET` siguen publicos.
- Con `[ApiController]`, los parametros complejos de `POST` y `PATCH` se infieren como `[FromBody]` aunque el atributo no este escrito.
- ASP.NET Core serializa normalmente los resultados de controladores con nombres JSON en `camelCase`. El middleware global de excepciones serializa manualmente sin opciones, por lo que errores 500 no controlados pueden salir con propiedades `Success`, `Message`, `Data` en `PascalCase`.

## 2. Convenciones de respuesta

### 2.1 `ApiResponse<T>`

Clase: `HerreraSystem.Application.Common.ApiResponse<T>`

| Propiedad C# | Tipo C# | Tipo TypeScript | Nullable | Descripcion |
|---|---|---|---:|---|
| `Success` | `bool` | `boolean` | No | Indica si la operacion fue exitosa. |
| `Message` | `string` | `string` | No | Mensaje descriptivo. Valor por defecto: `""`. |
| `Data` | `T?` | `T \| null` | Si | Payload de datos. En errores controlados queda `null`, salvo validacion de modelo, donde contiene lista de errores. |

Interface TypeScript recomendada:

```ts
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
}
```

### 2.2 `PagedResponse<T>`

Clase: `HerreraSystem.Application.Common.PagedResponse<T>`

| Propiedad C# | Tipo C# | Tipo TypeScript | Nullable | Descripcion |
|---|---|---|---:|---|
| `Data` | `IEnumerable<T>` | `T[]` | No | Registros de la pagina actual. |
| `CurrentPage` | `int` | `number` | No | Pagina actual. |
| `PageSize` | `int` | `number` | No | Tamano de pagina aplicado. |
| `TotalRecords` | `int` | `number` | No | Total de registros encontrados. |
| `TotalPages` | `int` | `number` | No | Total de paginas. |
| `HasNextPage` | `bool` solo getter | `boolean` | No | `true` si hay pagina siguiente. |
| `HasPreviousPage` | `bool` solo getter | `boolean` | No | `true` si hay pagina previa. |

Interface TypeScript recomendada:

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

### 2.3 `PaginationParams`

Clase: `HerreraSystem.Application.Common.PaginationParams`

Se recibe por query string.

| Parametro C# | Tipo C# | Tipo TypeScript | Default | Reglas |
|---|---|---|---:|---|
| `Page` | `int` | `number` | `1` | Sin validacion por Data Annotations. |
| `PageSize` | `int` | `number` | `10` | Maximo aplicado por setter: `50`. Si se envia `PageSize > 50`, se usa `50`. |

Ejemplo:

```http
GET /api/Products?page=1&pageSize=20
```

## 3. Modelo de datos Entity Framework

### 3.1 Entidad `Product`

Clase compilada: `HerreraSystem.Domain.Entities.Product` en `HerreraSystemDomain/Entities/Product.cs`

DbSet:

```csharp
public virtual DbSet<Product> Products { get; set; }
```

Configuracion EF en `HerreraSystemContext`:

- Llave primaria: `Id`.
- Tabla inferida por DbSet/convencion: `Products`.
- `ProductName`: `varchar(150)`, `IsUnicode(false)`.
- `ImageUrl`: columna `ImageURL`, `varchar(2048)`, `IsUnicode(false)`.
- `CreatedAt`: `datetime`, default SQL `(getdate())`.
- `IsActive`: default SQL/EF `true`.
- Relaciones obligatorias con `User`, `Flavor` y `LinePresentation` usando `DeleteBehavior.ClientSetNull`.

Propiedades escalares:

| Propiedad C# | Tipo C# | Tipo TypeScript | Nullable C# | Reglas EF / DB |
|---|---|---|---:|---|
| `Id` | `int` | `number` | No | PK. Generado por base de datos. |
| `LinePresentationId` | `int` | `number` | No | FK a `LinePresentation.Id`. Obligatoria. |
| `FlavorId` | `int` | `number` | No | FK a `Flavor.Id`. Obligatoria. |
| `ProductName` | `string` | `string` | No | `varchar(150)`, requerido por tipo no nullable. |
| `IsActive` | `bool?` | `boolean \| null` | Si | Default `true`. En creacion desde repositorio se fuerza `true`. |
| `CreatedBy` | `int` | `number` | No | FK a `User.Id`. Obligatoria. |
| `CreatedAt` | `DateTime?` | `string \| null` | Si | `datetime`, default DB `getdate()`. En creacion desde repositorio se asigna `DateTime.UtcNow`. |
| `ImageUrl` | `string?` | `string \| null` | Si | Columna `ImageURL`, `varchar(2048)`. |
| `MinimumStock` | `int` | `number` | No | Sin configuracion adicional EF. Validado en DTO contra negativos. |

Propiedades de navegacion:

| Propiedad C# | Tipo C# | Tipo TypeScript conceptual | Nullable | Relacion |
|---|---|---|---:|---|
| `Batches` | `ICollection<Batch>` | `Batch[]` | No | Un producto puede tener muchos lotes. |
| `CreatedByNavigation` | `User` | `User` | No | Usuario creador. |
| `Flavor` | `Flavor` | `Flavor` | No | Sabor del producto. |
| `LinePresentation` | `LinePresentation` | `LinePresentation` | No | Combinacion linea-presentacion. |
| `OrderDetails` | `ICollection<OrderDetail>` | `OrderDetail[]` | No | Detalles de pedidos asociados. |
| `ProductPrices` | `ICollection<ProductPrice>` | `ProductPrice[]` | No | Precios asociados. |
| `SaleDetails` | `ICollection<SaleDetail>` | `SaleDetail[]` | No | Detalles de ventas asociados. |

### 3.2 Reglas de negocio del servicio

El servicio `ProductService` aplica reglas adicionales:

- Crear:
  - `FlavorId` debe existir.
  - `LinePresentationId` debe existir.
  - No puede existir otro producto con el mismo `ProductName`, `LinePresentationId` y `FlavorId`.
  - `IsActive` se crea siempre como `true`.
  - `CreatedBy` se toma del usuario autenticado mediante `ICurrentUserService`; no se recibe desde el body.
  - `CreatedAt` se asigna con `DateTime.UtcNow`.
- Actualizar parcialmente:
  - El producto `{id}` debe existir.
  - Si se envia `FlavorId`, debe existir.
  - Si se envia `LinePresentationId`, debe existir.
  - Si se envia `ProductName`, se valida duplicado usando el nombre nuevo y los IDs finales de linea-presentacion/sabor.
- Eliminar:
  - El producto `{id}` debe existir.
  - No se puede eliminar si tiene lotes (`Batches`) registrados.
  - No se puede eliminar si tiene precios activos (`ProductPrices` con `IsActive == true`).

## 4. DTOs del modulo de productos

### 4.1 `CreateProductDto`

Archivo: `HerreraSystem.Application/DTOs/ProductDtos/CreateProductDto.cs`

Usado por:

```http
POST /api/Products
```

| Propiedad C# | Tipo C# | Tipo TypeScript | Nullable | Obligatorio | Data Annotations |
|---|---|---|---:|---:|---|
| `LinePresentationId` | `int` | `number` | No | Si | `[Required(ErrorMessage = "El LinePresentationId es obligatorio")]` |
| `FlavorId` | `int` | `number` | No | Si | `[Required(ErrorMessage = "El FlavorId es obligatorio")]` |
| `ProductName` | `string` | `string` | No | Si | `[Required(ErrorMessage = "El nombre del producto es obligatorio")]`, `[StringLength(150, ErrorMessage = "El nombre no puede exceder 150 caracteres")]` |
| `ImageUrl` | `string?` | `string \| null` | Si | No | `[Url(ErrorMessage = "La URL de la imagen no es válida")]` |
| `MinimumStock` | `int` | `number` | No | No tecnicamente en JSON; default `0` si falta | `[Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo")]` |

Advertencia de model binding:

- `LinePresentationId`, `FlavorId` y `MinimumStock` son `int` no-nullable. Si el front-end omite alguno, ASP.NET puede bindearlo como `0`; `[Required]` no detecta `0` como ausente en tipos valor.
- Por eso el front-end debe tratarlos como obligatorios aunque el servidor pueda aceptar `0` y fallar luego por reglas de negocio o FK.
- `CreatedBy` no forma parte del request. La API lo obtiene del JWT y falla con `400` si no puede identificar al usuario autenticado.

Interface TypeScript:

```ts
export interface CreateProductRequest {
  linePresentationId: number;
  flavorId: number;
  productName: string;
  imageUrl?: string | null;
  minimumStock: number;
}
```

JSON esperado:

```json
{
  "linePresentationId": 1,
  "flavorId": 2,
  "productName": "Helado Fresa 1L",
  "imageUrl": "https://example.com/productos/helado-fresa-1l.png",
  "minimumStock": 10
}
```

### 4.2 `PatchProductDto`

Archivo: `HerreraSystem.Application/DTOs/ProductDtos/PatchProductDto.cs`

Usado por:

```http
PATCH /api/Products/{id}
```

Todos los campos son opcionales. Solo se actualizan los campos enviados con valor no `null`.

| Propiedad C# | Tipo C# | Tipo TypeScript | Nullable | Obligatorio | Data Annotations |
|---|---|---|---:|---:|---|
| `LinePresentationId` | `int?` | `number \| null` | Si | No | Ninguna. Si se envia, el servicio valida existencia. |
| `FlavorId` | `int?` | `number \| null` | Si | No | Ninguna. Si se envia, el servicio valida existencia. |
| `ProductName` | `string?` | `string \| null` | Si | No | `[StringLength(150, ErrorMessage = "El nombre no puede exceder 150 caracteres")]` |
| `IsActive` | `bool?` | `boolean \| null` | Si | No | Ninguna. |
| `ImageUrl` | `string?` | `string \| null` | Si | No | `[Url(ErrorMessage = "La URL de la imagen no es válida")]` |
| `MinimumStock` | `int?` | `number \| null` | Si | No | `[Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo")]` |

Interface TypeScript:

```ts
export interface PatchProductRequest {
  linePresentationId?: number | null;
  flavorId?: number | null;
  productName?: string | null;
  isActive?: boolean | null;
  imageUrl?: string | null;
  minimumStock?: number | null;
}
```

JSON esperado:

```json
{
  "productName": "Helado Fresa 1L Premium",
  "imageUrl": "https://example.com/productos/helado-fresa-premium.png",
  "minimumStock": 15,
  "isActive": true
}
```

Ejemplo actualizando relaciones:

```json
{
  "linePresentationId": 3,
  "flavorId": 5
}
```

### 4.3 `UpdateProductDto`

Archivo: `HerreraSystem.Application/DTOs/ProductDtos/UpdateProductDto.cs`

Usado por:

```http
PUT /api/Products/{id}
```

DTO de edicion completa. El frontend debe enviar todos los campos editables del formulario.

| Propiedad C# | Tipo C# | Tipo TypeScript | Nullable | Obligatorio | Data Annotations |
|---|---|---|---:|---:|---|
| `LinePresentationId` | `int` | `number` | No | Si | `[Required(ErrorMessage = "El LinePresentationId es obligatorio")]` |
| `FlavorId` | `int` | `number` | No | Si | `[Required(ErrorMessage = "El FlavorId es obligatorio")]` |
| `ProductName` | `string` | `string` | No | Si | `[Required(ErrorMessage = "El nombre del producto es obligatorio")]`, `[StringLength(150, ErrorMessage = "El nombre no puede exceder 150 caracteres")]` |
| `IsActive` | `bool?` | `boolean \| null` | Si | No | Ninguna. |
| `ImageUrl` | `string?` | `string \| null` | Si | No | `[Url(ErrorMessage = "La URL de la imagen no es válida")]` |
| `MinimumStock` | `int` | `number` | No | No tecnicamente en JSON; default `0` si falta | `[Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo")]` |

Interface TypeScript:

```ts
export interface UpdateProductRequest {
  linePresentationId: number;
  flavorId: number;
  productName: string;
  isActive?: boolean | null;
  imageUrl?: string | null;
  minimumStock: number;
}
```

JSON esperado:

```json
{
  "linePresentationId": 1,
  "flavorId": 2,
  "productName": "Helado Fresa 1L Premium",
  "isActive": true,
  "imageUrl": "https://example.com/productos/helado-fresa-premium.png",
  "minimumStock": 15
}
```

### 4.4 `ProductDto`

Archivo: `HerreraSystem.Application/DTOs/ProductDtos/ProductDto.cs`

Usado por:

- `GET /api/Products`
- `GET /api/Products/{id}`
- `POST /api/Products`

| Propiedad C# | Tipo C# | Tipo TypeScript | Nullable | Descripcion |
|---|---|---|---:|---|
| `Id` | `int` | `number` | No | ID del producto. |
| `LinePresentationId` | `int` | `number` | No | ID de linea-presentacion. |
| `FlavorId` | `int` | `number` | No | ID del sabor. |
| `ProductName` | `string` | `string` | No | Nombre del producto. |
| `IsActive` | `bool?` | `boolean \| null` | Si | Estado activo/inactivo. |
| `CreatedBy` | `int` | `number` | No | ID del usuario creador. |
| `CreatedAt` | `DateTime?` | `string \| null` | Si | Fecha ISO serializada por ASP.NET Core. |
| `ImageUrl` | `string?` | `string \| null` | Si | URL de imagen. |
| `MinimumStock` | `int` | `number` | No | Stock minimo configurado. |

Interface TypeScript:

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
```

Ejemplo JSON:

```json
{
  "id": 10,
  "linePresentationId": 1,
  "flavorId": 2,
  "productName": "Helado Fresa 1L",
  "isActive": true,
  "createdBy": 1,
  "createdAt": "2026-06-20T18:30:00Z",
  "imageUrl": "https://example.com/productos/helado-fresa-1l.png",
  "minimumStock": 10
}
```

### 4.5 `ProductCatalogDto`

Archivo: `HerreraSystem.Application/DTOs/ProductDtos/ProductCatalogDto.cs`

Usado por:

```http
GET /api/Products/catalog
```

| Propiedad C# | Tipo C# | Tipo TypeScript | Nullable | Descripcion |
|---|---|---|---:|---|
| `Id` | `int` | `number` | No | ID del producto. |
| `ProductName` | `string` | `string` | No | Nombre del producto. |
| `ImageUrl` | `string?` | `string \| null` | Si | URL de imagen. |
| `IsActive` | `bool?` | `boolean \| null` | Si | Estado activo/inactivo. |
| `LineName` | `string` | `string` | No | Nombre de linea. |
| `FlavorName` | `string` | `string` | No | Nombre de sabor. |
| `PresentationName` | `string` | `string` | No | Nombre de presentacion. |
| `WholesalePrice` | `decimal?` | `number \| null` | Si | Precio activo de mayoreo, si existe. |
| `RetailPrice` | `decimal?` | `number \| null` | Si | Precio activo de detalle, si existe. |

Interface TypeScript:

```ts
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
```

Logica de precios del catalogo:

- `WholesalePrice` busca en `ProductPrices` el precio activo donde:
  - `IsActive == true`
  - `LinePresentationId == product.LinePresentationId`
  - `PriceTypeId == PriceTypeConstants.Wholesale`
  - `ValidTo == null || ValidTo >= DateTime.UtcNow`
  - toma el mas reciente por `ValidFrom` descendente.
- `RetailPrice` aplica la misma logica con `PriceTypeConstants.Retail`.
- Si no hay precio vigente, el valor retorna `null`.

Ejemplo JSON:

```json
{
  "id": 10,
  "productName": "Helado Fresa 1L",
  "imageUrl": "https://example.com/productos/helado-fresa-1l.png",
  "isActive": true,
  "lineName": "Helados",
  "flavorName": "Fresa",
  "presentationName": "1 Litro",
  "wholesalePrice": 80.00,
  "retailPrice": 100.00
}
```

### 4.6 `ProductSelectionDto`

Archivo: `HerreraSystem.Application/DTOs/ProductDtos/ProductSelectionDto.cs`

Usado por:

```http
GET /api/Products/by-line-presentation?linePresentationId={linePresentationId}
```

DTO optimizado para selects de Restock y ventas cuando el frontend ya conoce el `linePresentationId`.

| Propiedad C# | Tipo C# | Tipo TypeScript | Nullable | Descripcion |
|---|---|---|---:|---|
| `ProductId` | `int` | `number` | No | ID del producto. |
| `ProductName` | `string` | `string` | No | Nombre del producto. |
| `LinePresentationId` | `int` | `number` | No | ID de la combinacion linea-presentacion. |
| `LineName` | `string` | `string` | No | Nombre de linea. |
| `PresentationName` | `string` | `string` | No | Nombre de presentacion. |
| `FlavorId` | `int` | `number` | No | ID del sabor. |
| `FlavorName` | `string` | `string` | No | Nombre del sabor. |

Interface TypeScript:

```ts
export interface ProductSelectionDto {
  productId: number;
  productName: string;
  linePresentationId: number;
  lineName: string;
  presentationName: string;
  flavorId: number;
  flavorName: string;
}
```

### 4.7 `ProductStatsDto`

Archivo: `HerreraSystem.Application/DTOs/ProductDtos/ProductStatsDto.cs`

Usado por:

```http
GET /api/Products/stats
```

| Propiedad C# | Tipo C# | Tipo TypeScript | Nullable | Descripcion |
|---|---|---|---:|---|
| `TotalProducts` | `int` | `number` | No | Total de productos registrados. |
| `ActiveProducts` | `int` | `number` | No | Productos con `IsActive == true`. |
| `InactiveProducts` | `int` | `number` | No | Productos con `IsActive != true`; incluye `false` y `null` por ser nullable. |

Interface TypeScript:

```ts
export interface ProductStatsDto {
  totalProducts: number;
  activeProducts: number;
  inactiveProducts: number;
}
```

### 4.8 `ProductImageUploadResponseDto`

Archivo: `HerreraSystem.Application/DTOs/UploadDtos/ProductImageUploadResponseDto.cs`

Usado por:

```http
POST /api/uploads/products
```

| Propiedad C# | Tipo C# | Tipo TypeScript | Nullable | Descripcion |
|---|---|---|---:|---|
| `ImageUrl` | `string` | `string` | No | URL relativa publica del archivo guardado fisicamente en `wwwroot/uploads/products`. |

Interface TypeScript:

```ts
export interface ProductImageUploadResponseDto {
  imageUrl: string;
}
```

Ejemplo JSON:

```json
{
  "imageUrl": "/uploads/products/a13c2cbe-f0b5-4d6d-8f2e-123456789abc.jpg"
}
```

## 5. Endpoints CRUD y catalogo

### 5.1 Listar productos basicos

```http
GET /api/Products
```

Metodo del controlador:

```csharp
[HttpGet]
public async Task<IActionResult> GetAll([FromQuery] PaginationParams paginationParams)
```

Parametros:

| Nombre | Ubicacion | Tipo C# | Tipo TS | Requerido | Default |
|---|---|---|---|---:|---:|
| `page` / `Page` | Query string | `int` | `number` | No | `1` |
| `pageSize` / `PageSize` | Query string | `int` | `number` | No | `10`, max `50` |

Ejemplo request:

```http
GET /api/Products?page=1&pageSize=10
```

Respuesta exitosa:

```http
200 OK
```

```json
{
  "success": true,
  "message": "Operación exitosa",
  "data": {
    "data": [
      {
        "id": 10,
        "linePresentationId": 1,
        "flavorId": 2,
        "productName": "Helado Fresa 1L",
        "isActive": true,
        "createdBy": 1,
        "createdAt": "2026-06-20T18:30:00Z",
        "imageUrl": "https://example.com/productos/helado-fresa-1l.png",
        "minimumStock": 10
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

Ordenamiento:

- El repositorio ordena por `ProductName` ascendente.

Errores comunes:

- `500 Internal Server Error` si ocurre una excepcion no controlada consultando EF/SQL Server.

### 5.2 Listar catalogo enriquecido de productos

```http
GET /api/Products/catalog
```

Metodo del controlador:

```csharp
[HttpGet("catalog")]
public async Task<IActionResult> GetCatalog(
    [FromQuery] int? lineId,
    [FromQuery] int? presentationId,
    [FromQuery] int? flavorId,
    [FromQuery] string? search,
    [FromQuery] bool? active,
    [FromQuery] PaginationParams paginationParams)
```

Parametros:

| Nombre | Ubicacion | Tipo C# | Tipo TS | Requerido | Descripcion |
|---|---|---|---|---:|---|
| `lineId` | Query string | `int?` | `number \| undefined` | No | Filtra por `LinePresentation.LineId`. |
| `presentationId` | Query string | `int?` | `number \| undefined` | No | Filtra por `Product.LinePresentation.PresentationId`. |
| `flavorId` | Query string | `int?` | `number \| undefined` | No | Filtra por `Product.FlavorId`. |
| `search` | Query string | `string?` | `string \| undefined` | No | Filtra productos cuyo `ProductName` contiene el texto. |
| `active` | Query string | `bool?` | `boolean \| undefined` | No | Filtra por `IsActive`. |
| `page` / `Page` | Query string | `int` | `number` | No | Paginacion. Default `1`. |
| `pageSize` / `PageSize` | Query string | `int` | `number` | No | Paginacion. Default `10`, max `50`. |

Ejemplo request:

```http
GET /api/Products/catalog?lineId=1&presentationId=2&flavorId=3&search=fresa&active=true&page=1&pageSize=12
```

Respuesta exitosa:

```http
200 OK
```

```json
{
  "success": true,
  "message": "Operación exitosa",
  "data": {
    "data": [
      {
        "id": 10,
        "productName": "Helado Fresa 1L",
        "imageUrl": "https://example.com/productos/helado-fresa-1l.png",
        "isActive": true,
        "lineName": "Helados",
        "flavorName": "Fresa",
        "presentationName": "1 Litro",
        "wholesalePrice": 80.00,
        "retailPrice": 100.00
      }
    ],
    "currentPage": 1,
    "pageSize": 12,
    "totalRecords": 1,
    "totalPages": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
  }
}
```

Ordenamiento:

- El repositorio ordena por `ProductName` ascendente.

Compatibilidad:

- `presentationId` es opcional.
- Si no se envia, el catalogo mantiene el comportamiento anterior.
- Puede combinarse con `lineId`, `flavorId`, `search`, `active`, `page` y `pageSize`.

Errores comunes:

- `500 Internal Server Error` si falla la consulta EF, una relacion esperada es nula o hay problemas con SQL Server.

### 5.3 Listar productos activos por LinePresentation

```http
GET /api/Products/by-line-presentation?linePresentationId={linePresentationId}
```

Endpoint auxiliar para Restock y ventas. Devuelve productos activos de una combinacion linea-presentacion especifica.

Metodo del controlador:

```csharp
[HttpGet("by-line-presentation")]
public async Task<IActionResult> GetByLinePresentation(
    [FromQuery] int linePresentationId)
```

Parametros:

| Nombre | Ubicacion | Tipo C# | Tipo TS | Requerido | Descripcion |
|---|---|---|---|---:|---|
| `linePresentationId` | Query string | `int` | `number` | Si | ID de la combinacion linea-presentacion seleccionada. Debe ser mayor que cero. |

Ejemplo request:

```http
GET /api/Products/by-line-presentation?linePresentationId=5
```

Respuesta exitosa con datos:

```http
200 OK
```

```json
{
  "success": true,
  "message": "Operación exitosa",
  "data": [
    {
      "productId": 15,
      "productName": "Premium Litro Fresa",
      "linePresentationId": 5,
      "lineName": "Premium",
      "presentationName": "Litro",
      "flavorId": 2,
      "flavorName": "Fresa"
    }
  ]
}
```

Respuesta exitosa sin productos:

```json
{
  "success": true,
  "message": "Operación exitosa",
  "data": []
}
```

Validaciones:

- Si `linePresentationId <= 0`, retorna `400 Bad Request`.
- No valida existencia de la relacion; si no existe o no tiene productos activos, retorna lista vacia.

Error por parametro invalido:

```http
400 Bad Request
```

```json
{
  "success": false,
  "message": "El LinePresentationId debe ser mayor que cero",
  "data": null
}
```

Reglas de consulta:

- Filtra `Product.LinePresentationId == linePresentationId`.
- Filtra solo productos activos con `IsActive == true`.
- Ordena por `FlavorName` y luego por `ProductName`.
- Mantiene formato `ApiResponse<List<ProductSelectionDto>>`.

### 5.4 Estadisticas de productos

```http
GET /api/Products/stats
```

Metodo del controlador:

```csharp
[HttpGet("stats")]
public async Task<IActionResult> GetStats()
```

Respuesta exitosa:

```http
200 OK
```

```json
{
  "success": true,
  "message": "Operación exitosa",
  "data": {
    "totalProducts": 200,
    "activeProducts": 180,
    "inactiveProducts": 20
  }
}
```

Reglas de calculo:

- `totalProducts`: total de filas en `Products`.
- `activeProducts`: productos con `IsActive == true`.
- `inactiveProducts`: productos con `IsActive != true`, por lo tanto incluye `false` y `null`.

### 5.5 Obtener producto por ID

```http
GET /api/Products/{id}
```

Metodo del controlador:

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetById(int id)
```

Parametros:

| Nombre | Ubicacion | Tipo C# | Tipo TS | Requerido |
|---|---|---|---|---:|
| `id` | Route | `int` | `number` | Si |

Ejemplo request:

```http
GET /api/Products/10
```

Respuesta exitosa:

```http
200 OK
```

```json
{
  "success": true,
  "message": "Operación exitosa",
  "data": {
    "id": 10,
    "linePresentationId": 1,
    "flavorId": 2,
    "productName": "Helado Fresa 1L",
    "isActive": true,
    "createdBy": 1,
    "createdAt": "2026-06-20T18:30:00Z",
    "imageUrl": "https://example.com/productos/helado-fresa-1l.png",
    "minimumStock": 10
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
  "message": "Producto con Id 10 no encontrado",
  "data": null
}
```

Errores comunes:

- `400 Bad Request` automatico de model binding si `id` no puede convertirse a `int`.
- `500 Internal Server Error` si ocurre una excepcion no controlada.

### 5.6 Subir imagen de producto

```http
POST /api/uploads/products
```

Endpoint independiente para almacenar fisicamente una imagen de producto en el servidor de la API.

Controlador:

```csharp
[Route("api/[controller]")]
[ApiController]
public class UploadsController : ControllerBase
```

Metodo:

```csharp
[HttpPost("products")]
[Consumes("multipart/form-data")]
public async Task<IActionResult> UploadProductImage(IFormFile? file)
```

Ruta fisica de almacenamiento:

```txt
HerreraSystemAPI/wwwroot/uploads/products
```

URL publica resultante:

```txt
/uploads/products/{nombre-generado}.{extension}
```

Parametros:

| Nombre | Ubicacion | Tipo C# | Tipo TS/Browser | Requerido | Descripcion |
|---|---|---|---|---:|---|
| `file` | Form-data | `IFormFile?` | `File` | Si | Archivo seleccionado desde la computadora del usuario. |

Headers importantes:

| Header | Valor |
|---|---|
| `Content-Type` | `multipart/form-data` generado automaticamente por el navegador al usar `FormData`. No establecer manualmente el boundary. |

Extensiones permitidas:

| Extension | Permitida |
|---|---:|
| `.jpg` | Si |
| `.jpeg` | Si |
| `.png` | Si |
| `.webp` | Si |

Reglas implementadas:

- Si `file` es `null`, retorna `400 Bad Request`.
- Si `file.Length <= 0`, retorna `400 Bad Request`.
- Si la extension no esta en la lista permitida, retorna `400 Bad Request`.
- No se usa el nombre original del archivo para guardarlo.
- Se genera un nombre unico con `Guid.NewGuid()`.
- Se conserva la extension normalizada a minusculas.
- Si `wwwroot/uploads/products` no existe, la API lo crea automaticamente.
- Los archivos son servidos publicamente gracias a `app.UseStaticFiles()`.

Ejemplo de nombre generado:

```txt
a13c2cbe-f0b5-4d6d-8f2e-123456789abc.jpg
```

Ejemplo request desde navegador:

```ts
const formData = new FormData();
formData.append("file", selectedFile);

const response = await fetch("/api/uploads/products", {
  method: "POST",
  body: formData
});
```

No agregar este header manualmente:

```ts
headers: { "Content-Type": "multipart/form-data" }
```

El navegador debe calcular el `boundary` de `multipart/form-data`.

Respuesta exitosa:

```http
200 OK
```

```json
{
  "success": true,
  "message": "Imagen subida correctamente",
  "data": {
    "imageUrl": "/uploads/products/a13c2cbe-f0b5-4d6d-8f2e-123456789abc.jpg"
  }
}
```

Errores controlados:

Archivo nulo:

```http
400 Bad Request
```

```json
{
  "success": false,
  "message": "El archivo es obligatorio",
  "data": null
}
```

Archivo vacio:

```http
400 Bad Request
```

```json
{
  "success": false,
  "message": "El archivo no puede estar vacío",
  "data": null
}
```

Extension no permitida:

```http
400 Bad Request
```

```json
{
  "success": false,
  "message": "Extensión de imagen no permitida",
  "data": null
}
```

Error no controlado:

- `500 Internal Server Error` si falla la escritura fisica del archivo, permisos del servidor, disco lleno u otra excepcion de IO.

### 5.7 Crear producto

```http
POST /api/Products
```

Metodo del controlador:

```csharp
[HttpPost]
[Authorize]
public async Task<IActionResult> Create(CreateProductDto dto)
```

Requiere autenticacion:

```http
Authorization: Bearer {jwt}
```

Parametros:

| Nombre | Ubicacion | Tipo C# | Tipo TS | Requerido |
|---|---|---|---|---:|
| `dto` | Body JSON | `CreateProductDto` | `CreateProductRequest` | Si |

Body JSON esperado:

```json
{
  "linePresentationId": 1,
  "flavorId": 2,
  "productName": "Helado Fresa 1L",
  "imageUrl": "https://example.com/productos/helado-fresa-1l.png",
  "minimumStock": 10
}
```

Respuesta exitosa:

```http
201 Created
Location: /api/Products/{id}
```

```json
{
  "success": true,
  "message": "Producto creado exitosamente",
  "data": {
    "id": 10,
    "linePresentationId": 1,
    "flavorId": 2,
    "productName": "Helado Fresa 1L",
    "isActive": true,
    "createdBy": 1,
    "createdAt": "2026-06-20T18:30:00Z",
    "imageUrl": "https://example.com/productos/helado-fresa-1l.png",
    "minimumStock": 10
  }
}
```

Errores controlados de negocio:

```http
400 Bad Request
```

Flavor inexistente:

```json
{
  "success": false,
  "message": "El sabor con Id 2 no existe",
  "data": null
}
```

LinePresentation inexistente:

```json
{
  "success": false,
  "message": "La presentación de línea con Id 1 no existe",
  "data": null
}
```

Producto duplicado:

```json
{
  "success": false,
  "message": "Ya existe un producto 'Helado Fresa 1L' con esa línea y sabor",
  "data": null
}
```

Error de validacion por Data Annotations:

```http
400 Bad Request
```

```json
{
  "success": false,
  "message": "Errores de validación",
  "data": [
    "El nombre del producto es obligatorio",
    "El nombre no puede exceder 150 caracteres",
    "La URL de la imagen no es válida",
    "El stock mínimo no puede ser negativo"
  ]
}
```

Errores no controlados posibles:

- `500 Internal Server Error` si ocurre cualquier error EF/SQL Server.

Ejemplo de respuesta 500 emitida por `ExceptionMiddleware`:

```json
{
  "Success": false,
  "Message": "Error interno del servidor: The INSERT statement conflicted with the FOREIGN KEY constraint ...",
  "Data": null
}
```

### 5.8 Actualizar producto completo

```http
PUT /api/Products/{id}
```

Endpoint pensado para formularios de edicion completa desde el front-end.

Metodo del controlador:

```csharp
[HttpPut("{id}")]
[Authorize]
public async Task<IActionResult> Update(int id, UpdateProductDto dto)
```

Requiere autenticacion:

```http
Authorization: Bearer {jwt}
```

Parametros:

| Nombre | Ubicacion | Tipo C# | Tipo TS | Requerido |
|---|---|---|---|---:|
| `id` | Route | `int` | `number` | Si |
| `dto` | Body JSON | `UpdateProductDto` | `UpdateProductRequest` | Si |

Body JSON esperado:

```json
{
  "linePresentationId": 1,
  "flavorId": 2,
  "productName": "Helado Fresa 1L Premium",
  "isActive": true,
  "imageUrl": "https://example.com/productos/helado-fresa-premium.png",
  "minimumStock": 15
}
```

Respuesta exitosa:

```http
200 OK
```

```json
{
  "success": true,
  "message": "Producto actualizado exitosamente",
  "data": null
}
```

Respuesta si el producto no existe:

```http
404 Not Found
```

```json
{
  "success": false,
  "message": "Producto con Id 10 no encontrado",
  "data": null
}
```

Errores controlados de negocio:

```http
400 Bad Request
```

Flavor inexistente:

```json
{
  "success": false,
  "message": "El sabor con Id 5 no existe",
  "data": null
}
```

LinePresentation inexistente:

```json
{
  "success": false,
  "message": "La presentación de línea con Id 3 no existe",
  "data": null
}
```

Producto duplicado:

```json
{
  "success": false,
  "message": "Ya existe un producto 'Helado Fresa 1L Premium' con esa línea y sabor",
  "data": null
}
```

Error de validacion por Data Annotations:

```http
400 Bad Request
```

```json
{
  "success": false,
  "message": "Errores de validación",
  "data": [
    "El nombre del producto es obligatorio",
    "El nombre no puede exceder 150 caracteres",
    "La URL de la imagen no es válida",
    "El stock mínimo no puede ser negativo"
  ]
}
```

Errores no controlados:

- `500 Internal Server Error` si EF/SQL Server falla al guardar.

### 5.9 Actualizar producto parcialmente

```http
PATCH /api/Products/{id}
```

Para ediciones completas desde formulario, preferir `PUT /api/Products/{id}`. Para cambios puntuales, usar `PATCH`.

Metodo del controlador:

```csharp
[HttpPatch("{id}")]
[Authorize]
public async Task<IActionResult> Patch(int id, PatchProductDto dto)
```

Requiere autenticacion:

```http
Authorization: Bearer {jwt}
```

Parametros:

| Nombre | Ubicacion | Tipo C# | Tipo TS | Requerido |
|---|---|---|---|---:|
| `id` | Route | `int` | `number` | Si |
| `dto` | Body JSON | `PatchProductDto` | `PatchProductRequest` | Si, aunque sus campos son opcionales |

Body JSON esperado:

```json
{
  "linePresentationId": 3,
  "flavorId": 5,
  "productName": "Helado Fresa 1L Premium",
  "isActive": true,
  "imageUrl": "https://example.com/productos/helado-fresa-premium.png",
  "minimumStock": 15
}
```

Tambien es valido enviar solo los campos a modificar:

```json
{
  "isActive": false
}
```

Respuesta exitosa:

```http
200 OK
```

```json
{
  "success": true,
  "message": "Producto actualizado exitosamente",
  "data": null
}
```

Respuesta si el producto no existe:

```http
404 Not Found
```

```json
{
  "success": false,
  "message": "Producto con Id 10 no encontrado",
  "data": null
}
```

Errores controlados de negocio:

```http
400 Bad Request
```

Flavor inexistente:

```json
{
  "success": false,
  "message": "El sabor con Id 5 no existe",
  "data": null
}
```

LinePresentation inexistente:

```json
{
  "success": false,
  "message": "La presentación de línea con Id 3 no existe",
  "data": null
}
```

Producto duplicado:

```json
{
  "success": false,
  "message": "Ya existe un producto 'Helado Fresa 1L Premium' con esa línea y sabor",
  "data": null
}
```

Error de validacion por Data Annotations:

```http
400 Bad Request
```

```json
{
  "success": false,
  "message": "Errores de validación",
  "data": [
    "El nombre no puede exceder 150 caracteres",
    "La URL de la imagen no es válida",
    "El stock mínimo no puede ser negativo"
  ]
}
```

Errores no controlados:

- `500 Internal Server Error` si EF/SQL Server falla al guardar.

### 5.10 Eliminar producto

```http
DELETE /api/Products/{id}
```

Metodo del controlador:

```csharp
[HttpDelete("{id}")]
[Authorize]
public async Task<IActionResult> Delete(int id)
```

Requiere autenticacion:

```http
Authorization: Bearer {jwt}
```

Parametros:

| Nombre | Ubicacion | Tipo C# | Tipo TS | Requerido |
|---|---|---|---|---:|
| `id` | Route | `int` | `number` | Si |

No recibe body.

Respuesta exitosa:

```http
200 OK
```

```json
{
  "success": true,
  "message": "Producto eliminado exitosamente",
  "data": null
}
```

Respuesta si el producto no existe:

```http
404 Not Found
```

```json
{
  "success": false,
  "message": "Producto con Id 10 no encontrado",
  "data": null
}
```

Errores controlados de negocio:

Producto con lotes registrados:

```http
400 Bad Request
```

```json
{
  "success": false,
  "message": "No se puede eliminar 'Helado Fresa 1L' porque tiene lotes registrados",
  "data": null
}
```

Producto con precios activos:

```http
400 Bad Request
```

```json
{
  "success": false,
  "message": "No se puede eliminar 'Helado Fresa 1L' porque tiene precios activos",
  "data": null
}
```

Errores no controlados:

- `500 Internal Server Error` si hay restricciones FK no cubiertas por las validaciones del servicio, por ejemplo referencias en ventas/pedidos, o si falla EF/SQL Server.

## 6. Seguridad y autenticacion

### 6.1 Configuracion global de la API

En `Program.cs` se configura autenticacion JWT Bearer:

```csharp
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = Jwt["Issuer"],
        ValidateAudience = true,
        ValidAudience = Jwt["Audience"],
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key!))
    };
});
```

Pipeline:

```csharp
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

Swagger tambien declara esquema:

```http
Authorization: Bearer {jwt}
```

### 6.2 Estado real de seguridad en `ProductsController`

`ProductsController` protege solamente las operaciones que modifican datos. Las consultas siguen publicas.

| Endpoint | Requiere token Bearer JWT | Roles requeridos |
|---|---:|---|
| `GET /api/Products` | No | Ninguno |
| `GET /api/Products/catalog` | No | Ninguno |
| `GET /api/Products/by-line-presentation` | No | Ninguno |
| `GET /api/Products/stats` | No | Ninguno |
| `GET /api/Products/{id}` | No | Ninguno |
| `POST /api/uploads/products` | No | Ninguno |
| `POST /api/Products` | Si | Ninguno |
| `PUT /api/Products/{id}` | Si | Ninguno |
| `PATCH /api/Products/{id}` | Si | Ninguno |
| `DELETE /api/Products/{id}` | Si | Ninguno |

Recomendacion de integracion:

- El front-end debe enviar `Authorization: Bearer <token>` para crear, editar o eliminar productos.
- Si no se envia token en endpoints protegidos, la API responde `401 Unauthorized`.
- Si en el futuro se agregan roles a Products, un token valido sin permisos respondera `403 Forbidden`.
- La documentacion completa de JWT, claims y auditoria esta en `DOCUMENTACION_AUTORIZACION_JWT.md`.

## 7. Mapa TypeScript completo recomendado

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

export interface ProductSelectionDto {
  productId: number;
  productName: string;
  linePresentationId: number;
  lineName: string;
  presentationName: string;
  flavorId: number;
  flavorName: string;
}

export interface ProductStatsDto {
  totalProducts: number;
  activeProducts: number;
  inactiveProducts: number;
}

export interface ProductImageUploadResponseDto {
  imageUrl: string;
}

export interface CreateProductRequest {
  linePresentationId: number;
  flavorId: number;
  productName: string;
  imageUrl?: string | null;
  minimumStock: number;
}

export interface UpdateProductRequest {
  linePresentationId: number;
  flavorId: number;
  productName: string;
  isActive?: boolean | null;
  imageUrl?: string | null;
  minimumStock: number;
}

export interface PatchProductRequest {
  linePresentationId?: number | null;
  flavorId?: number | null;
  productName?: string | null;
  isActive?: boolean | null;
  imageUrl?: string | null;
  minimumStock?: number | null;
}

export interface ProductListQuery {
  page?: number;
  pageSize?: number;
}

export interface ProductCatalogQuery extends ProductListQuery {
  lineId?: number;
  presentationId?: number;
  flavorId?: number;
  search?: string;
  active?: boolean;
}

export type ProductResponse = ApiResponse<ProductDto>;
export type ProductListResponse = ApiResponse<PagedResponse<ProductDto>>;
export type ProductCatalogResponse = ApiResponse<PagedResponse<ProductCatalogDto>>;
export type ProductSelectionResponse = ApiResponse<ProductSelectionDto[]>;
export type ProductStatsResponse = ApiResponse<ProductStatsDto>;
export type ProductImageUploadResponse = ApiResponse<ProductImageUploadResponseDto>;
export type ProductMutationResponse = ApiResponse<null>;
export type ValidationErrorResponse = ApiResponse<string[]>;
```

## 8. Consideraciones practicas para construir el front-end

### 8.1 Listado administrativo

Usar:

```http
GET /api/Products?page={page}&pageSize={pageSize}
```

Muestra datos basicos y IDs necesarios para editar:

- `id`
- `linePresentationId`
- `flavorId`
- `productName`
- `isActive`
- `createdBy`
- `createdAt`
- `imageUrl`
- `minimumStock`

### 8.2 Catalogo visual

Usar:

```http
GET /api/Products/catalog?lineId={lineId}&presentationId={presentationId}&flavorId={flavorId}&search={search}&active={active}&page={page}&pageSize={pageSize}
```

Muestra datos listos para UI:

- Nombre de linea.
- Nombre de sabor.
- Nombre de presentacion.
- Precio mayoreo.
- Precio detalle.

Filtros disponibles:

- `lineId`: filtra por linea.
- `presentationId`: filtra por presentacion.
- `flavorId`: filtra por sabor.
- `search`: busca por nombre del producto.
- `active`: filtra activos/inactivos.

No expone `linePresentationId` ni `flavorId`; para editar desde una tarjeta de catalogo, el front-end necesitara llamar tambien a:

```http
GET /api/Products/{id}
```

### 8.3 Productos para Restock y ventas por LinePresentation

Usar cuando el usuario ya selecciono linea y presentacion, y el frontend ya conoce el `linePresentationId`.

```http
GET /api/Products/by-line-presentation?linePresentationId={linePresentationId}
```

Este endpoint devuelve solo productos activos de esa combinacion, con datos listos para select:

```ts
const result = await getProductsByLinePresentation(selectedLinePresentationId);
const productOptions = result.data ?? [];
```

Cada opcion tiene:

- `productId`: valor a enviar en Restock/ventas cuando se elige producto.
- `productName`: texto principal del producto.
- `flavorId` y `flavorName`: utiles para mostrar o agrupar por sabor.
- `lineName` y `presentationName`: contexto visual.

Si no hay productos activos, la API devuelve `success: true` y `data: []`.

### 8.4 Estadisticas de productos

Usar en dashboards o tarjetas de resumen:

```http
GET /api/Products/stats
```

Ejemplo de uso:

```ts
const statsResult = await getProductStats();
const stats = statsResult.data;
```

Campos:

- `totalProducts`
- `activeProducts`
- `inactiveProducts`

### 8.5 Crear producto

El front-end debe obtener previamente:

- `linePresentationId` desde el modulo/endpoints de line-presentations.
- `flavorId` desde el modulo/endpoints de flavors.
- JWT del usuario autenticado para enviarlo en el header `Authorization`.

Flujo recomendado cuando el usuario selecciona una imagen:

1. El usuario selecciona un archivo en un `<input type="file">`.
2. El frontend envia ese archivo a `POST /api/uploads/products` usando `FormData`.
3. La API devuelve `data.imageUrl`, por ejemplo `/uploads/products/a13c2cbe-f0b5-4d6d-8f2e-123456789abc.jpg`.
4. El frontend asigna ese valor al campo `imageUrl` del `CreateProductRequest`.
5. El frontend crea el producto con `POST /api/Products` enviando `Authorization: Bearer {jwt}`.

Validaciones recomendadas antes de enviar:

- `productName`: requerido, maximo 150 caracteres.
- `imageUrl`: no se escribe manualmente; debe venir de `POST /api/uploads/products`.
- Archivo de imagen: permitir en UI solo `.jpg`, `.jpeg`, `.png`, `.webp`.
- `minimumStock`: entero mayor o igual a `0`.
- `linePresentationId` y `flavorId`: enteros positivos.

Ejemplo de flujo completo:

```ts
const uploadResult = await uploadProductImage(selectedFile);

if (!uploadResult.success || !uploadResult.data) {
  throw new Error(uploadResult.message);
}

const payload: CreateProductRequest = {
  linePresentationId: Number(form.linePresentationId),
  flavorId: Number(form.flavorId),
  productName: form.productName,
  imageUrl: uploadResult.data.imageUrl,
  minimumStock: Number(form.minimumStock)
};

const createResult = await createProduct(payload);
```

### 8.6 Editar producto

Usar `PUT` cuando el formulario de edicion envia el producto completo. Usar `PATCH` solo para cambios puntuales.

Flujo recomendado al editar imagen:

1. Si el usuario no selecciona una nueva imagen, conservar el `imageUrl` actual del producto.
2. Si el usuario selecciona una nueva imagen, subirla primero con `POST /api/uploads/products`.
3. Usar el nuevo `data.imageUrl` en el `PUT /api/Products/{id}`.
4. Por ahora la API no elimina automaticamente la imagen anterior; planificar limpieza futura para evitar archivos huerfanos.

Ejemplo con `PUT`:

```ts
let imageUrl = currentProduct.imageUrl;

if (selectedFile) {
  const uploadResult = await uploadProductImage(selectedFile);
  if (!uploadResult.success || !uploadResult.data) {
    throw new Error(uploadResult.message);
  }

  imageUrl = uploadResult.data.imageUrl;
}

const payload: UpdateProductRequest = {
  linePresentationId: Number(form.linePresentationId),
  flavorId: Number(form.flavorId),
  productName: form.productName,
  isActive: form.isActive,
  imageUrl,
  minimumStock: Number(form.minimumStock)
};

const updateResult = await updateProduct(productId, payload);
```

Enviar solo campos modificados para reducir riesgo:

```json
{
  "minimumStock": 20
}
```

Para cambiar nombre, sabor o linea-presentacion, considerar que el backend validara duplicados contra la combinacion final:

```text
ProductName + LinePresentationId + FlavorId
```

### 8.7 Eliminar producto

La eliminacion es fisica (`_context.Products.Remove(product)`), no soft delete.

Puede fallar por reglas de negocio si:

- Tiene lotes registrados.
- Tiene precios activos.

Aunque no esta bloqueado explicitamente por el servicio, tambien podria fallar por FK si existen ventas, pedidos u otras referencias.

Para "desactivar" visualmente un producto, usar:

```http
PATCH /api/Products/{id}
```

```json
{
  "isActive": false
}
```

## 9. Matriz de errores

| Caso | Endpoint | Codigo | Formato |
|---|---|---:|---|
| Validacion Data Annotations | `POST`, `PUT`, `PATCH` | 400 | `ApiResponse<List<string>>` con `data` como arreglo de mensajes. |
| Producto no existe | `GET {id}`, `PUT {id}`, `PATCH {id}`, `DELETE {id}` | 404 | `ApiResponse<T>.Fail(...)` con `data: null`. |
| Flavor no existe | `POST`, `PUT`, `PATCH` | 400 | `ApiResponse<T>.Fail(...)` con `data: null`. |
| LinePresentation no existe | `POST`, `PUT`, `PATCH` | 400 | `ApiResponse<T>.Fail(...)` con `data: null`. |
| Producto duplicado | `POST`, `PUT`, `PATCH` | 400 | `ApiResponse<T>.Fail(...)` con `data: null`. |
| Producto con lotes | `DELETE` | 400 | `ApiResponse<object>.Fail(...)` con `data: null`. |
| Producto con precios activos | `DELETE` | 400 | `ApiResponse<object>.Fail(...)` con `data: null`. |
| Token ausente, expirado o invalido | `POST`, `PUT`, `PATCH`, `DELETE` | 401 | Respuesta de ASP.NET Core por `[Authorize]`. |
| Token valido sin rol requerido futuro | `POST`, `PUT`, `PATCH`, `DELETE` | 403 | Aplica si luego se agregan roles/politicas. |
| Archivo de imagen nulo | `POST /api/uploads/products` | 400 | `ApiResponse<object>.Fail("El archivo es obligatorio")`. |
| Archivo de imagen vacio | `POST /api/uploads/products` | 400 | `ApiResponse<object>.Fail("El archivo no puede estar vacío")`. |
| Extension de imagen no permitida | `POST /api/uploads/products` | 400 | `ApiResponse<object>.Fail("Extensión de imagen no permitida")`. |
| Excepcion EF/SQL no controlada | Cualquiera | 500 | `ApiResponse<object>.Fail(...)` serializado por middleware, probablemente en `PascalCase`. |
| Ruta `id` no convertible a int | `GET {id}`, `PUT {id}`, `PATCH {id}`, `DELETE {id}` | 400 | Respuesta automatica de ASP.NET Core para model binding. |

## 10. Contratos HTTP listos para cliente

### 10.1 Servicio TypeScript sugerido

```ts
const API_BASE = "/api/Products";
const PRODUCT_UPLOADS_BASE = "/api/uploads/products";

function authHeaders(token: string): HeadersInit {
  return {
    "Content-Type": "application/json",
    "Authorization": `Bearer ${token}`
  };
}

export async function uploadProductImage(file: File) {
  const formData = new FormData();
  formData.append("file", file);

  const res = await fetch(PRODUCT_UPLOADS_BASE, {
    method: "POST",
    body: formData
  });

  return (await res.json()) as ProductImageUploadResponse;
}

export async function getProducts(query: ProductListQuery = {}) {
  const params = new URLSearchParams();
  if (query.page !== undefined) params.set("page", String(query.page));
  if (query.pageSize !== undefined) params.set("pageSize", String(query.pageSize));

  const res = await fetch(`${API_BASE}?${params.toString()}`);
  return (await res.json()) as ProductListResponse;
}

export async function getProductCatalog(query: ProductCatalogQuery = {}) {
  const params = new URLSearchParams();
  if (query.lineId !== undefined) params.set("lineId", String(query.lineId));
  if (query.presentationId !== undefined) params.set("presentationId", String(query.presentationId));
  if (query.flavorId !== undefined) params.set("flavorId", String(query.flavorId));
  if (query.search) params.set("search", query.search);
  if (query.active !== undefined) params.set("active", String(query.active));
  if (query.page !== undefined) params.set("page", String(query.page));
  if (query.pageSize !== undefined) params.set("pageSize", String(query.pageSize));

  const res = await fetch(`${API_BASE}/catalog?${params.toString()}`);
  return (await res.json()) as ProductCatalogResponse;
}

export async function getProductsByLinePresentation(linePresentationId: number) {
  const params = new URLSearchParams();
  params.set("linePresentationId", String(linePresentationId));

  const res = await fetch(`${API_BASE}/by-line-presentation?${params.toString()}`);
  return (await res.json()) as ProductSelectionResponse;
}

export async function getProductStats() {
  const res = await fetch(`${API_BASE}/stats`);
  return (await res.json()) as ProductStatsResponse;
}

export async function getProductById(id: number) {
  const res = await fetch(`${API_BASE}/${id}`);
  return (await res.json()) as ProductResponse;
}

export async function createProduct(payload: CreateProductRequest, token: string) {
  const res = await fetch(API_BASE, {
    method: "POST",
    headers: authHeaders(token),
    body: JSON.stringify(payload)
  });
  return (await res.json()) as ProductResponse | ValidationErrorResponse;
}

export async function updateProduct(id: number, payload: UpdateProductRequest, token: string) {
  const res = await fetch(`${API_BASE}/${id}`, {
    method: "PUT",
    headers: authHeaders(token),
    body: JSON.stringify(payload)
  });
  return (await res.json()) as ProductMutationResponse | ValidationErrorResponse;
}

export async function patchProduct(id: number, payload: PatchProductRequest, token: string) {
  const res = await fetch(`${API_BASE}/${id}`, {
    method: "PATCH",
    headers: authHeaders(token),
    body: JSON.stringify(payload)
  });
  return (await res.json()) as ProductMutationResponse | ValidationErrorResponse;
}

export async function deleteProduct(id: number, token: string) {
  const res = await fetch(`${API_BASE}/${id}`, {
    method: "DELETE",
    headers: {
      "Authorization": `Bearer ${token}`
    }
  });
  return (await res.json()) as ProductMutationResponse;
}
```

### 10.2 Previsualizacion de imagenes

La API devuelve una URL relativa:

```txt
/uploads/products/a13c2cbe-f0b5-4d6d-8f2e-123456789abc.jpg
```

Si el frontend y backend corren en el mismo origen, puede usarse directamente:

```tsx
<img src={product.imageUrl ?? ""} alt={product.productName} />
```

Si el frontend corre en otro puerto/dominio, anteponer la URL base de la API:

```ts
const API_ORIGIN = "https://localhost:7000";

function resolveProductImageUrl(imageUrl: string | null): string | null {
  if (!imageUrl) return null;
  if (imageUrl.startsWith("http://") || imageUrl.startsWith("https://")) return imageUrl;
  return `${API_ORIGIN}${imageUrl}`;
}
```

Ejemplo:

```tsx
const src = resolveProductImageUrl(product.imageUrl);

return src ? (
  <img src={src} alt={product.productName} />
) : (
  <div>Sin imagen</div>
);
```

### 10.3 Validacion frontend recomendada para archivos

Aunque el backend valida extension, conviene filtrar en el cliente para mejor UX.

Input recomendado:

```tsx
<input
  type="file"
  accept=".jpg,.jpeg,.png,.webp,image/jpeg,image/png,image/webp"
  onChange={(event) => {
    const file = event.target.files?.[0] ?? null;
    setSelectedFile(file);
  }}
/>
```

Validacion auxiliar:

```ts
const ALLOWED_IMAGE_EXTENSIONS = [".jpg", ".jpeg", ".png", ".webp"];

function isAllowedProductImage(file: File): boolean {
  const lowerName = file.name.toLowerCase();
  return ALLOWED_IMAGE_EXTENSIONS.some((extension) => lowerName.endsWith(extension));
}
```

### 10.4 Manejo de errores recomendado

```ts
function getApiErrorMessage(response: unknown): string {
  const value = response as {
    success?: boolean;
    message?: string;
    data?: unknown;
    Success?: boolean;
    Message?: string;
    Data?: unknown;
  };

  const message = value.message ?? value.Message;
  const data = value.data ?? value.Data;

  if (Array.isArray(data)) return data.join("\n");
  if (typeof message === "string" && message.length > 0) return message;
  return "Ocurrio un error procesando la solicitud.";
}
```

## 11. Checklist de implementacion Front-end

- Usar `<input type="file">` para que el usuario seleccione imagen desde su computadora.
- Subir imagen con `POST /api/uploads/products` y `FormData`.
- Enviar el campo form-data con nombre exacto `file`.
- No establecer manualmente `Content-Type` al subir imagen; dejar que el navegador genere `multipart/form-data` con boundary.
- Aceptar solo `.jpg`, `.jpeg`, `.png`, `.webp` en UI.
- Guardar `data.imageUrl` devuelto por uploads en `imageUrl` del producto.
- Usar `GET /api/Products/catalog` para vistas publicas o tarjetas enriquecidas.
- Usar `GET /api/Products` para administracion donde se necesitan IDs.
- Usar `GET /api/Products/{id}` antes de abrir formulario de edicion si se viene desde catalogo.
- Crear con `POST /api/Products` enviando `Authorization: Bearer {jwt}`.
- Editar formulario completo con `PUT /api/Products/{id}` enviando `Authorization: Bearer {jwt}`.
- Editar cambios puntuales con `PATCH /api/Products/{id}` enviando `Authorization: Bearer {jwt}`.
- Eliminar con `DELETE /api/Products/{id}` enviando `Authorization: Bearer {jwt}` y mostrar mensajes de bloqueo por lotes/precios activos.
- Validar en cliente: nombre requerido/max 150, archivo permitido, stock minimo >= 0, IDs positivos.
- No enviar `createdBy` al crear productos; la API lo obtiene del token JWT.

## 12. Buenas practicas para reemplazo y eliminacion futura de imagenes

La infraestructura actual sube y publica archivos, pero no elimina automaticamente imagenes antiguas. Para una siguiente iteracion, se recomienda:

- Al reemplazar imagen:
  - Subir primero la nueva imagen a `POST /api/uploads/products`.
  - Actualizar el producto con `PUT /api/Products/{id}` usando el nuevo `imageUrl`.
  - Si la actualizacion del producto fue exitosa, eliminar la imagen anterior si ya no esta referenciada por otro producto.

- Al eliminar producto:
  - Revisar si se quiere eliminar tambien el archivo fisico asociado a `ImageUrl`.
  - Antes de borrar, validar que ningun otro producto use la misma URL.

- Al borrar archivo fisico:
  - Convertir la URL relativa a ruta fisica solo si empieza con `/uploads/products/`.
  - Resolver la ruta absoluta y confirmar que queda dentro de `wwwroot/uploads/products`.
  - No aceptar rutas arbitrarias enviadas desde el frontend para borrar archivos.

- Limpieza de huerfanos:
  - Crear un job administrativo que compare archivos en `wwwroot/uploads/products` contra valores `Products.ImageUrl`.
  - Borrar solo archivos no referenciados y con antiguedad suficiente para evitar eliminar uploads recientes que aun no se han guardado en producto.

- Produccion:
  - Asegurar permisos de escritura para la identidad del proceso de la API sobre `wwwroot/uploads/products`.
  - Respaldar la carpeta de uploads junto con la base de datos.
  - Definir un limite maximo de tamano por archivo si las imagenes pueden crecer demasiado.
