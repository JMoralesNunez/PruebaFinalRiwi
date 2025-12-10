using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using System.Text;
using TalentoPlus.Api.Data; 
using TalentoPlus.Api.Data.Repositories.Implementations;
using TalentoPlus.Api.Data.Repositories.Interfaces;
using TalentoPlus.Api.Services;
using TalentoPlus.Api.Services.Implementations;
using TalentoPlus.Api.Services.Interfaces;
using DotNetEnv;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración de QuestPDF
QuestPDF.Settings.License = LicenseType.Community;

// 2. Base de Datos (MySQL)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 3. Identity (Usuarios y Roles)
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// 4. Configuración de JWT
var jwtKey = builder.Configuration["Jwt:Key"];
var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// 5. Configuración de CORS (Para VueJS)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp",
        policy => policy
            .WithOrigins("http://localhost:5173") 
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// 6. Inyección de Dependencias (Repositorios y Servicios)
builder.Services.AddScoped<IDepartamentoRepository, DepartamentoRepository>();
builder.Services.AddScoped<IEmpleadoRepository, EmpleadoRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IEmpleadoService, EmpleadoService>();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 7. Configuración de Swagger con Soporte JWT
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "TalentoPlus API", 
        Version = "v1",
        Description = "Sistema de Gestión de Empleados y RRHH"
    });

    // Definición de seguridad mejorada para JWT (Tipo HTTP)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http, // Cambiado de ApiKey a Http para mejor UX
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese su token JWT aquí (no necesita escribir 'Bearer ')"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// --- Pipeline de Peticiones HTTP ---


app.UseSwagger();
app.UseSwaggerUI(); 

app.UseHttpsRedirection();

app.UseCors("AllowVueApp"); 

// Importante: Orden correcto de Auth
app.UseAuthentication(); 
app.UseAuthorization();  

app.MapControllers();

// --- Data Seeder (Semilla de Datos) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await TalentoPlus.Api.Data.DbSeeder.SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error en la creación de roles o admin.");
    }
}

app.Run();