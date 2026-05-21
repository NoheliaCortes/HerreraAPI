using HerreraSystem.API.Middleware;
using HerreraSystem.Application.Common;
using HerreraSystem.Application.Interfaces;
using HerreraSystem.Infrastructure.Data;
using HerreraSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;



var builder = WebApplication.CreateBuilder(args);

// Registra los controllers y configura el formato de error
// cuando las validaciones de los DTOs fallan (400 Bad Request)
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            // Extrae todos los mensajes de error de validación
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .SelectMany(e => e.Value!.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            var response = ApiResponse<List<string>>.Fail("Errores de validación");
            response.Data = errors;

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(response);
        };
    });


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer(); 
builder.Services.AddSwaggerGen();
//builder.Services.AddOpenApi();

builder.Services.AddDbContext<HerreraSystemContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IFlavorRepository, FlavorRepository>();

builder.Services.AddScoped<ILineRepository, LineRepository>();

builder.Services.AddScoped<IPresentationRepository, PresentationRepository>();

var app = builder.Build();

// Registra el middleware PRIMERO antes que todo
// para que capture errores de cualquier parte del pipeline
app.UseMiddleware<ExceptionMiddleware>();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
