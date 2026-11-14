# Módulo de encuestas de consumidores

## Endpoint

`POST /api/consumer-surveys`

El endpoint guarda encuestas en MongoDB Atlas, base `HerreraAnalytics`, colección `consumer_surveys`. No modifica operaciones de ventas ni tablas de SQL Server.

## Configuración local

Configura el secreto fuera de Git. En PowerShell, para la sesión actual:

```powershell
$env:MongoDB__ConnectionString = "mongodb+srv://<usuario>:<password>@<cluster>/"
$env:Cors__ConsumerSurveyOrigins__0 = "http://localhost:3000"
```

Agrega un origen por índice adicional cuando sea necesario, por ejemplo `Cors__ConsumerSurveyOrigins__1`. En despliegue, configura las mismas claves como variables de entorno de la aplicación. La API puede arrancar sin MongoDB configurado; el endpoint devuelve `503` hasta que se configure la conexión.

La base y colección son configuración del servidor, con valores predeterminados `HerreraAnalytics` y `consumer_surveys`. El cliente no puede elegirlas ni enviar el canal.

## Solicitud de ejemplo

```json
{
  "reviewerName": "María López",
  "productName": "Sorbete de chocolate",
  "line": "Tradicionales",
  "flavor": "Chocolate",
  "presentation": "Vaso grande",
  "municipality": "Jinotepe",
  "department": "Carazo",
  "rating": 5,
  "wouldRecommend": true,
  "sentiment": "Muy satisfecho",
  "comment": "Excelente sabor y textura",
  "surveyDate": "2026-11-13"
}
```

`surveyDate` es opcional; si se omite, el servidor usa la fecha local de Nicaragua en formato `yyyy-MM-dd`. `channel` se asigna como `Formulario web`. MongoDB agrega automáticamente `_id`. La colección no almacena identificadores de SQL Server ni relaciones con ventas.

## Respuestas

- `201 Created`: encuesta insertada correctamente.
- `400 Bad Request`: campos ausentes, textos fuera de longitud, calificación fuera de 1–5 o fecha inválida.
- `503 Service Unavailable`: MongoDB no está configurado o no está disponible. La respuesta no revela detalles internos ni credenciales.

El CORS de este endpoint usa únicamente los orígenes de `Cors:ConsumerSurveyOrigins`; por defecto no se permiten orígenes cross-origin hasta configurarlos.
