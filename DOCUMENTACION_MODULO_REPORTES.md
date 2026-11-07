# Documentación técnica del módulo de reportes

Base URL: `/api/Reports`

El módulo genera consultas analíticas directamente desde la base operacional, siguiendo la separación existente entre controller, servicio de Application, interfaz de repositorio y consultas EF Core en Infrastructure. No depende del Data Warehouse, SSIS ni SSAS.

Todos los endpoints requieren `startDate` y `endDate` en formato `YYYY-MM-DD`. El rango incluye ambos días. Si falta una fecha o `startDate` es posterior a `endDate`, la API responde con el error de validación estándar. Los reportes usan la fecha de la venta y excluyen al cliente genérico (Id 1).

## Ventas por línea y sabor

`GET /api/Reports/sales-by-line-flavor?startDate=2026-01-01&endDate=2026-12-31`

Devuelve unidades vendidas e importe por combinación de línea y sabor. El importe se calcula sumando `LineSubtotal`, con respaldo de `Quantity * AppliedPrice` cuando el subtotal sea nulo.

## Tendencia mensual

`GET /api/Reports/monthly-sales-trend?startDate=2026-01-01&endDate=2026-12-31`

Devuelve unidades vendidas agrupadas por año, mes y línea. Año y mes se devuelven por separado para distinguir períodos que abarcan más de un año.

## Ventas por geografía

`GET /api/Reports/sales-by-geography?startDate=2026-01-01&endDate=2026-12-31`

Devuelve unidades e importe por departamento y municipio del cliente. El cliente genérico se excluye.

## Top 10 clientes

`GET /api/Reports/top-customers?startDate=2026-01-01&endDate=2026-12-31`

Devuelve hasta diez clientes ordenados por unidades compradas en el período. El cliente genérico se excluye.

## Ventas por presentación

`GET /api/Reports/sales-by-presentation?startDate=2026-01-01&endDate=2026-12-31`

Devuelve unidades por presentación y línea, junto con el porcentaje de participación sobre las unidades vendidas del período.
