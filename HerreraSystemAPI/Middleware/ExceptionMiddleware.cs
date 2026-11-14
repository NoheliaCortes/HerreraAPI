using HerreraSystem.Application.Common;
using System.Net;
using System.Text.Json;

namespace HerreraSystem.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
  

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Deja pasar la petición normalmente hacia el Controller
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Unhandled exception {ExceptionType} for {RequestMethod} {RequestPath}. Stack: {StackTrace}",
                    ex.GetType().Name,
                    context.Request.Method,
                    context.Request.Path,
                    ex.StackTrace);

                context.Response.Clear();
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var response = ApiResponse<object>.Fail("Error interno del servidor.");
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }

    }
}
