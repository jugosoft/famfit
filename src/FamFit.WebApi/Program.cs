using FamFit.Application.Interfaces;
using FamFit.Domain.Enums;
using FamFit.Infrastructure;
using FamFit.Infrastructure.Data;
using FamFit.WebApi.Endpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// === Configuration ===
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
    });
builder.Services.AddEndpointsApiExplorer();

// === OpenAPI / Swagger ===
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FamFit API",
        Version = "v1",
        Description = "Семейный PWA-трекер активности. OpenAPI-контракт для генерации TypeScript-типов через openapi-typescript.",
        Contact = new OpenApiContact { Name = "FamFit Support", Email = "support@famfit.local" }
    });

    // JWT Auth в Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT-токен, полученный через POST /api/auth/login"
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

// Валидация длины JWT-секрета
if (jwtSecret.Length < 32)
{
    throw new InvalidOperationException(
        "JWT secret must be at least 32 characters. Update your appsettings.json.");
}

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
            ClockSkew = TimeSpan.Zero,
            // Require expiration claim
            RequireExpirationTime = true
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception is SecurityTokenExpiredException)
                {
                    Console.WriteLine("[JWT] Token expired for user: " +
                        context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier));
                }
                else
                {
                    Console.WriteLine($"[JWT] Authentication failed: {context.Exception.Message}");
                }
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                var role = context.Principal?.FindFirstValue(ClaimTypes.Role);
                Console.WriteLine($"[JWT] Token validated for user: {userId} role: {role}");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                // Кастомный ответ при отсутствии авторизации
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                var result = System.Text.Json.JsonSerializer.Serialize(new
                {
                    error = "Unauthorized",
                    message = "Valid authentication token is required."
                });
                return context.Response.WriteAsync(result);
            }
        };
    });

// === Authorization (Admin policy) ===
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy =>
        policy.RequireClaim(ClaimTypes.Role, Role.Admin.ToString()));

    options.AddPolicy("Member", policy =>
        policy.RequireClaim(ClaimTypes.Role, Role.Member.ToString()));

    // По умолчанию — любая авторизованная роль
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// === Application Layer (MediatR) ===
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(IApplicationDbContext).Assembly));

// === Infrastructure Layer ===
builder.Services.AddInfrastructure(builder.Configuration);

// === CORS (для PWA-фронтенда) ===
var frontendUrl = builder.Configuration.GetValue<string>("FrontendUrl")
    ?? "http://localhost:5173";

builder.Services.AddCors(options =>
{
    options.AddPolicy("FamFitPWA", policy =>
        policy.WithOrigins(frontendUrl, "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10)));
});

// === Rate Limiting (для auth endpoints) ===
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("auth-limiter", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(15);
    });
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

// Production security headers
if (!app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        await next(context);
    });
}

app.UseHttpsRedirection();
app.UseCors("FamFitPWA");

// Rate limiting for auth endpoints
app.MapWhen(ctx => ctx.Request.Path.StartsWithSegments("/api/auth"), appBuilder =>
{
    appBuilder.UseRouting();
    appBuilder.UseAuthorization();
    appBuilder.UseRateLimiter();
});

app.UseRouting();
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
