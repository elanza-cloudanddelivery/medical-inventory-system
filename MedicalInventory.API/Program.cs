using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using MedicalInventory.API.Data;
using MedicalInventory.API.Services.AuthService;
using MedicalInventory.API.Services.JwtService;

var builder = WebApplication.CreateBuilder(args);

// === CONFIGURACIÓN DE SERVICIOS ===

// Configurar Entity Framework con SQLite
builder.Services.AddDbContext<MedicalInventoryDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrar servicios de negocio
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Configurar JWT Authentication
var jwtKey = builder.Configuration["JWT:Key"] ?? "MiClaveSecretaSuperSeguraDeAlMenos32CaracteresParaSerSegura123!";
var jwtIssuer = builder.Configuration["JWT:Issuer"] ?? "MedicalInventoryAPI";
var jwtAudience = builder.Configuration["JWT:Audience"] ?? "MedicalInventoryClient";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Configurar controladores
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
    
    // Configurar JWT en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
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

// ORDEN CORRECTO: Authentication → Authorization → Controllers
app.UseAuthentication();
app.UseAuthorization();

// Agregar soporte para controladores
app.MapControllers();

app.Run();