using HerreraSystem.Application.Common;
using System.Net;
using System.Text.Json;

namespace HerreraSystem.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
  

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
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
                
                
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var response = ApiResponse<object>.Fail($"Error interno del servidor: {ex.Message}");
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }

    }
}
