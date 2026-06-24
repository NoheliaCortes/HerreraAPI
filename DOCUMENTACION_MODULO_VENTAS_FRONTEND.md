# Documentacion tecnica - Modulo Sales

Base URL: `/api/Sales`

Todas las respuestas usan el contenedor estandar `ApiResponse<T>`. El listado general usa `PagedResponse<T>` dentro de `data`.

Las consultas siguen el criterio actual de Restock: endpoints GET publicos y operaciones de escritura separadas. El endpoint existente `POST /api/Sales/retail` no fue modificado.

## 1. Estadisticas de ventas

`GET /api/Sales/stats`

Devuelve estadisticas del mes actual usando la fecha de venta como base.

Response:

```json
{
  "success": true,
  "message": null,
  "data": {
    "salesThisMonth": 25,
    "totalIncomeThisMonth": 15000.00,
    "productsSoldThisMonth": 320
  }
}
```

Reglas:

- `salesThisMonth`: cantidad de ventas registradas en el mes actual.
- `totalIncomeThisMonth`: suma de `totalSale` del mes actual.
- `productsSoldThisMonth`: suma de cantidades en `saleDetails` del mes actual.
- Actualmente `Sale` no maneja estado activo/inactivo; por eso no se aplica filtro de anulacion.

## 2. Listado general de ventas

`GET /api/Sales`

Parametros:

- `startDate` opcional, formato `YYYY-MM-DD`.
- `endDate` opcional, formato `YYYY-MM-DD`.
- `page` opcional, por defecto `1`.
- `pageSize` opcional, por defecto `10`, maximo `50`.

Ejemplo:

`GET /api/Sales?startDate=2026-06-01&endDate=2026-06-30&page=1&pageSize=10`

Si no se envia rango, se usa el mes actual. `endDate` incluye todo el dia recibido.

Response:

```json
{
  "success": true,
  "message": null,
  "data": {
    "data": [
      {
        "id": 1,
        "saleCode": "VTA-2026-0001",
        "saleDate": "2026-06-20T10:30:00",
        "customerName": "Cliente contado",
        "saleTypeId": 1,
        "saleTypeName": "Detalle",
        "paymentTypeId": 1,
        "paymentTypeName": "Contado",
        "total": 500.00
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

Orden: `saleDate` descendente y luego `id` descendente.

## 3. Encabezado de venta por ID

`GET /api/Sales/{id}`

Ejemplo:

`GET /api/Sales/1`

Response:

```json
{
  "success": true,
  "message": null,
  "data": {
    "id": 1,
    "saleCode": "VTA-2026-0001",
    "saleDate": "2026-06-20T10:30:00",
    "orderCode": "P-2026-0005",
    "customer": {
      "id": 10,
      "fullName": "Maria Lopez",
      "departmentName": "Carazo",
      "municipalityName": "Jinotepe",
      "pointOfSale": "Pulperia Maria"
    },
    "total": 1500.00,
    "paymentStatusName": "Pendiente",
    "pendingBalance": 500.00,
    "createdByUserName": "admin",
    "paymentTypeId": 2,
    "paymentTypeName": "Credito",
    "saleTypeId": 2,
    "saleTypeName": "Mayoreo"
  }
}
```

Casos especiales:

- Si no hay pedido relacionado, `orderCode` viene `null`.
- En venta al detalle con cliente generico se devuelven los datos disponibles del cliente configurado.
- Si algun dato opcional no existe, el campo puede venir `null`.
- Si la venta no existe, retorna `404` con `ApiResponse` de error.

## 4. Productos vendidos por venta

`GET /api/Sales/{id}/details`

Ejemplo:

`GET /api/Sales/1/details`

Response:

```json
{
  "success": true,
  "message": null,
  "data": [
    {
      "id": 1,
      "productId": 15,
      "productName": "Premium Litro Fresa",
      "batchCode": "PRE-FRE-LIT-2026-001",
      "quantity": 10,
      "unitPrice": 100.00,
      "lineSubtotal": 1000.00
    }
  ]
}
```

Si la venta existe pero no tiene detalles, `data` es una lista vacia.

## 5. Pagos relacionados a una venta

`GET /api/Sales/{id}/payments`

Ejemplo:

`GET /api/Sales/1/payments`

Response:

```json
{
  "success": true,
  "message": null,
  "data": [
    {
      "id": 1,
      "amount": 500.00,
      "paymentMethodName": "Transferencia",
      "paymentDate": "2026-06-20T12:00:00",
      "transactionReference": "BAC-123456",
      "registeredByUserName": "admin"
    }
  ]
}
```

Casos especiales:

- Si no hay pagos relacionados, `data` es una lista vacia.
- `transactionReference` puede venir `null`.
- Si la venta no existe, retorna `404` con `ApiResponse` de error.

## Ventas al detalle y mayoreo

- Venta al detalle: normalmente usa cliente generico, tipo de venta `Detalle`, tipo de pago `Contado` y pago registrado al momento de crear la venta retail.
- Venta mayoreo: puede venir relacionada a un pedido mediante `orderCode`, tener estado de pago pendiente y saldo pendiente.
- El frontend puede construir dashboard, tabla y drawer de detalle usando estos endpoints sin consumir entidades EF directamente.
