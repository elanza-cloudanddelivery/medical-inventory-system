using Microsoft.EntityFrameworkCore;
using MedicalInventory.API.Data;

var builder = WebApplication.CreateBuilder(args);

// === CONFIGURACIÓN DE SERVICIOS ===

// Configurar Entity Framework con SQLite
builder.Services.AddDbContext<MedicalInventoryDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar controladores (para tus APIs futuras)
builder.Services.AddControllers();

// Configurar Swagger para documentación de APIs
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Medical Inventory API", 
        Version = "v1",
        Description = "Sistema de Gestión de Inventario Médico"
    });
});

var app = builder.Build();

// === CONFIGURACIÓN DEL PIPELINE ===

// Configurar Swagger solo en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Medical Inventory API v1");
        c.RoutePrefix = string.Empty; // Swagger en la raíz del proyecto
    });
}

app.UseHttpsRedirection();

// Agregar soporte para controladores
app.MapControllers();

app.Run();