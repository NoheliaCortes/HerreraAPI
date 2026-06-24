# Documentacion tecnica - Autenticacion, autorizacion y auditoria JWT

Este documento describe como funciona actualmente la autenticacion JWT, la autorizacion por endpoint y el acceso reutilizable al usuario autenticado en `HerreraSystemAPI`.

Fuentes principales:

- `HerreraSystem.Application/Services/AuthService.cs`
- `HerreraSystemAPI/Program.cs`
- `HerreraSystemAPI/Services/CurrentUserService.cs`
- `HerreraSystem.Application/Interfaces/Services/ICurrentUserService.cs`
- `HerreraSystemAPI/Controllers/ProductsController.cs`
- `HerreraSystem.Application/Services/ProductService.cs`

## 1. Resumen

La API usa JWT Bearer. El login emite un token firmado con la configuracion `Jwt` de `appsettings`. Los endpoints protegidos usan `[Authorize]` y ASP.NET Core valida automaticamente el token recibido en el header:

```http
Authorization: Bearer {token}
```

La auditoria no debe depender de campos enviados por el frontend. Para obtener el usuario actual se creo `ICurrentUserService`, que lee los claims del JWT desde `HttpContext.User`.

## 2. Flujo completo

1. El usuario envia credenciales a login.
2. `AuthService.LoginAsync` valida usuario activo y password.
3. `AuthService.GenerateJwtToken` genera un JWT con ID, username y roles.
4. El frontend guarda el token de forma segura segun su estrategia de sesion.
5. En llamadas protegidas, el frontend envia `Authorization: Bearer {token}`.
6. `UseAuthentication()` valida firma, issuer, audience y expiracion.
7. `UseAuthorization()` aplica `[Authorize]`, roles o politicas.
8. Los servicios de aplicacion pueden leer el usuario con `ICurrentUserService`.

## 3. Claims incluidos en el JWT

El token generado durante login incluye estos claims relevantes:

| Claim | Valor | Uso |
|---|---|---|
| `sub` | `user.Id.ToString()` | Identificador estandar del sujeto del token. |
| `UserId` | `user.Id.ToString()` | Claim explicito para auditoria interna. |
| `ClaimTypes.NameIdentifier` | `user.Id.ToString()` | Compatibilidad con APIs de identidad de .NET. |
| `ClaimTypes.Name` | `user.UserName` | Nombre de usuario autenticado. |
| `username` | `user.UserName` | Compatibilidad con el claim previo del proyecto. |
| `jti` | `Guid.NewGuid().ToString()` | Identificador unico del token. |
| `FullName` | Nombre y apellido | Informacion descriptiva. |
| `email` | Email del usuario, si existe | Informacion descriptiva. |
| `ClaimTypes.Role` | Cada rol del usuario | Autorizacion por roles. |

Fragmento conceptual:

```csharp
new("UserId", user.Id.ToString())
new(ClaimTypes.NameIdentifier, user.Id.ToString())
new(ClaimTypes.Name, user.UserName)
new Claim(ClaimTypes.Role, role)
```

Si el usuario tiene varios roles, el token contiene varios claims `ClaimTypes.Role`, uno por rol.

## 4. Configuracion en `Program.cs`

La API registra autenticacion JWT Bearer:

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

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
```

El orden del pipeline es importante:

```csharp
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

`UseAuthentication()` debe ejecutarse antes que `UseAuthorization()` para que los claims ya esten disponibles cuando se evalua `[Authorize]`.

## 5. Autorizacion por endpoints

`[Authorize]` exige que exista un token valido. Si no hay token o el token no es valido, ASP.NET Core responde `401 Unauthorized`.

Ejemplo:

```csharp
[HttpPost]
[Authorize]
public async Task<IActionResult> Create(CreateProductDto dto)
```

Para roles se puede usar:

```csharp
[Authorize(Roles = "Administrador")]
```

o varios roles:

```csharp
[Authorize(Roles = "Administrador, Admin")]
```

Si el token es valido pero el usuario no tiene el rol requerido, la respuesta esperada es `403 Forbidden`.

## 6. Servicio de usuario actual

Interfaz:

```csharp
public interface ICurrentUserService
{
    int? CurrentUserId { get; }
    string? CurrentUsername { get; }
    string? CurrentRole { get; }
    bool IsAuthenticated { get; }
}
```

Implementacion:

```csharp
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public int? CurrentUserId => // UserId, NameIdentifier o sub
    public string? CurrentUsername => // ClaimTypes.Name o username
    public string? CurrentRole => // ClaimTypes.Role
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;
}
```

Este servicio evita repetir en controladores o servicios:

```csharp
User.FindFirst(...)
```

## 7. Auditoria basada en usuario autenticado

Regla general:

- El frontend no debe enviar `CreatedBy`, `UpdatedBy`, `CreatedAt` ni `UpdatedAt`.
- La API debe establecer esos valores usando `ICurrentUserService` y la hora del servidor.
- Los DTOs de entrada no deben incluir campos de auditoria.
- Los DTOs de salida si pueden exponer campos de auditoria cuando sean utiles para la UI.

Ejemplo actual en Products:

```csharp
if (!_currentUserService.IsAuthenticated || _currentUserService.CurrentUserId is null)
    return ServiceResult<ProductDto>.Fail("No se pudo identificar el usuario autenticado");

var created = await _productRepository.CreateAsync(
    dto,
    _currentUserService.CurrentUserId.Value);
```

El repositorio asigna:

```csharp
CreatedBy = createdBy,
CreatedAt = DateTime.UtcNow
```

## 8. Estado actual por modulo

| Modulo | Estado |
|---|---|
| Auth/Login | Emite JWT con `UserId`, username y roles. |
| Program.cs | Configura autenticacion, autorizacion e `IHttpContextAccessor`. |
| Products | Mutaciones protegidas con `[Authorize]`; create usa usuario autenticado para `CreatedBy`. |
| Customers | Pendiente de migrar al patron. |
| Restock | Pendiente de migrar al patron. |
| Sales | Pendiente de migrar al patron. |
| Inventory Movements | Pendiente de migrar al patron. |
| Orders | Pendiente de migrar al patron. |

## 9. Endpoints protegidos actualmente en Products

| Endpoint | Requiere JWT | Roles requeridos |
|---|---:|---|
| `GET /api/Products` | No | Ninguno |
| `GET /api/Products/catalog` | No | Ninguno |
| `GET /api/Products/{id}` | No | Ninguno |
| `POST /api/uploads/products` | No | Ninguno |
| `POST /api/Products` | Si | Ninguno |
| `PUT /api/Products/{id}` | Si | Ninguno |
| `PATCH /api/Products/{id}` | Si | Ninguno |
| `DELETE /api/Products/{id}` | Si | Ninguno |

## 10. Integracion Front-end

Para endpoints protegidos:

```ts
const token = getStoredToken();

await fetch("/api/Products", {
  method: "POST",
  headers: {
    "Content-Type": "application/json",
    "Authorization": `Bearer ${token}`
  },
  body: JSON.stringify(payload)
});
```

El body de productos no debe incluir `createdBy`:

```json
{
  "linePresentationId": 1,
  "flavorId": 2,
  "productName": "Helado Fresa 1L",
  "imageUrl": "/uploads/products/imagen.png",
  "minimumStock": 10
}
```

## 11. Respuestas esperadas de seguridad

| Caso | Codigo | Explicacion |
|---|---:|---|
| Sin header `Authorization` en endpoint protegido | 401 | No hay credenciales. |
| Token expirado | 401 | `ValidateLifetime = true`. |
| Token con firma invalida | 401 | No coincide con `Jwt:Key`. |
| Token con issuer invalido | 401 | No coincide con `Jwt:Issuer`. |
| Token con audience invalida | 401 | No coincide con `Jwt:Audience`. |
| Token valido sin rol requerido | 403 | Aplica cuando se usa `[Authorize(Roles = "...")]`. |

## 12. Patron para expandir a otros modulos

Para migrar un modulo:

1. Agregar `[Authorize]` a las acciones que modifican estado.
2. Quitar campos de auditoria de los DTOs de entrada.
3. Inyectar `ICurrentUserService` en el servicio de aplicacion.
4. Validar que `IsAuthenticated` sea `true` y `CurrentUserId` tenga valor.
5. Pasar `CurrentUserId.Value` al repositorio o asignarlo en la entidad.
6. Usar hora del servidor para `CreatedAt`/`UpdatedAt`.
7. Mantener campos de auditoria en DTOs de respuesta si el frontend necesita mostrarlos.

Ejemplo de constructor:

```csharp
private readonly ICurrentUserService _currentUserService;

public RestockService(..., ICurrentUserService currentUserService)
{
    _currentUserService = currentUserService;
}
```

Ejemplo en create:

```csharp
if (!_currentUserService.IsAuthenticated || _currentUserService.CurrentUserId is null)
    return ServiceResult<T>.Fail("No se pudo identificar el usuario autenticado");

entity.CreatedBy = _currentUserService.CurrentUserId.Value;
entity.CreatedAt = DateTime.UtcNow;
```

## 13. Consideraciones pendientes

- Definir roles o politicas por operacion cuando el negocio lo requiera.
- Migrar gradualmente `Customers`, `Restock`, `Sales`, `Inventory Movements` y `Orders`.
- Si se agregan `UpdatedBy`/`UpdatedAt` a Products en base de datos, poblarlos con el mismo servicio.
- Considerar una abstraccion comun para entidades auditables si varios modelos comparten `CreatedBy`, `CreatedAt`, `UpdatedBy` y `UpdatedAt`.
