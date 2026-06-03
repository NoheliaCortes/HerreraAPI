using HerreraSystem.API.Middleware;
using HerreraSystem.Application.Common;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Application.Interfaces.Services;
using HerreraSystem.Application.Services;
using HerreraSystem.Infrastructure.Data;
using HerreraSystem.Infrastructure.Persistence;
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

builder.Services.AddScoped<ILinePresentationRepository, LinePresentationRepository>();

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();


builder.Services.AddScoped<IGeneralPriceRepository, GeneralPriceRepository>();
builder.Services.AddScoped<IGeneralPriceService, GeneralPriceService>();

builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IInventoryService, InventoryService>();

// Repositories
builder.Services.AddScoped<IRestockRepository, RestockRepository>();
builder.Services.AddScoped<IRestockService, RestockService>();

builder.Services.AddScoped<IBatchRepository, BatchRepository>();

builder.Services.AddScoped<IBatchLocationRepository, BatchLocationRepository>();

builder.Services.AddScoped<IInventoryMovementRepository, InventoryMovementRepository>();

builder.Services.AddScoped<IMovementDetailRepository, MovementDetailRepository>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<ISaleRepository, SaleRepository>();
builder.Services.AddScoped<ISaleDetailRepository, SaleDetailRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IProductPriceRepository, ProductPriceRepository>();
builder.Services.AddScoped<IRetailSaleService, RetailSaleService>();

builder.Services.AddScoped<IInventoryMovementRepository, InventoryMovementRepository>();
builder.Services.AddScoped<IInventoryMovementService, InventoryMovementService>();

builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

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

app.UseCors("PermitirFrontend");
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
