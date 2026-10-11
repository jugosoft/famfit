using FamFit.Application.Interfaces;
using FamFit.Domain.Enums;
using FamFit.Infrastructure;
using FamFit.Infrastructure.Data;
using FamFit.WebApi.Endpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// === Configuration ===
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// === OpenAPI / Swagger ===
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FamFit API",
        Version = "v1",
        Description = "Семейный PWA-трекер активности. OpenAPI-контракт для генерации TypeScript-типов через openapi-typescript."
    });

    // JWT Auth в Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT-токен, полученный через /api/auth/login"
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

// === Authentication (JWT Bearer) ===
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtSecret = jwtSection["Secret"]
    ?? throw new InvalidOperationException("JWT secret is not configured.");
var jwtIssuer = jwtSection["Issuer"] ?? "FamFit.WebApi";
var jwtAudience = jwtSection["Audience"] ?? "FamFitClient";

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
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"[JWT] Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                Console.WriteLine($"[JWT] Token validated for user: {userId}");
                return Task.CompletedTask;
            }
        };
    });

// === Authorization (Admin policy) ===
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy =>
        policy.RequireClaim(ClaimTypes.Role, Role.Admin.ToString()));
});

// === Application Layer (MediatR) ===
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(FamFit.Application.Interfaces.IApplicationDbContext).Assembly));

// === Infrastructure Layer ===
builder.Services.AddInfrastructure(builder.Configuration);

// === CORS (для PWA-фронтенда) ===
builder.Services.AddCors(options =>
{
    options.AddPolicy("FamFitPWA", policy =>
        policy.WithOrigins(
                builder.Configuration.GetValue<string>("FrontendUrl")
                ?? "http://localhost:5173",
                "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

// === Health Checks ===
builder.Services.AddHealthChecks()
    .AddDbContextCheck<FamFitDbContext>("Database");

var app = builder.Build();

// === Middleware Pipeline ===
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "FamFit API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors("FamFitPWA");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// === Seed Database ===
try
{
    await DbSeeder.SeedAsync(app.Services);
}
catch (Exception ex)
{
    Console.WriteLine($"[Seed] Failed to seed database: {ex.Message}");
}

// === Map API Endpoints ===
app.MapAuthEndpoints();
app.MapActivityTypeEndpoints();
app.MapActivityLogEndpoints();
app.MapPushSubscriptionEndpoints();
app.MapVapidEndpoints();

// === Health Check ===
app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Redirect("/swagger"));

Console.WriteLine("[FamFit] API started. Open /swagger for documentation.");
app.Run();
