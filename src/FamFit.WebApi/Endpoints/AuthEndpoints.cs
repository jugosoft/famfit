using FamFit.Application.Interfaces;
using FamFit.Domain.Entities;
using FamFit.Domain.Enums;
using FamFit.Infrastructure.Data;
using FamFit.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FamFit.WebApi.Endpoints;

/// <summary>
/// Маршруты API для аутентификации.
/// </summary>
public static class AuthEndpoints
{
    public record LoginRequest(string Email, string Password);

    public record LoginResponse(
        string Token,
        string TokenType,
        int ExpiresInSeconds,
        UserDto User);

    public record UserDto(
        Guid Id,
        string Name,
        string Email,
        string Role,
        int StreakCount);

    public record RegisterRequest(string Name, string Email, string Password);

    public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth")
            .WithOpenApi();

        // POST /api/auth/login
        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            FamFitDbContext db,
            IConfiguration configuration) =>
        {
            // 1. Находим пользователя по email
            var user = await db.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email.Trim().ToLowerInvariant());

            if (user is null)
                return Results.Unauthorized();

            // 2. Проверяем пароль через BCrypt
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Results.Unauthorized();

            // 3. Генерируем JWT-токен
            var jwtSection = configuration.GetSection("Jwt");
            var secret = jwtSection["Secret"]
                ?? throw new InvalidOperationException("JWT secret not configured.");
            var issuer = jwtSection["Issuer"] ?? "FamFit.WebApi";
            var audience = jwtSection["Audience"] ?? "FamFitClient";

            // Валидация длины секрета (минимум 32 символа для HS256)
            if (secret.Length < 32)
                throw new InvalidOperationException(
                    "JWT secret must be at least 32 characters long.");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Name),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials);

            var response = new LoginResponse(
                new JwtSecurityTokenHandler().WriteToken(token),
                "Bearer",
                8 * 3600,
                new UserDto(
                    user.Id,
                    user.Name,
                    user.Email,
                    user.Role.ToString(),
                    user.StreakCount));

            return Results.Ok(response);
        })
        .WithSummary("Вход в систему")
        .Produces<LoginResponse>()
        .Produces(StatusCodes.Status401Unauthorized);

        // POST /api/auth/register
        group.MapPost("/register", async (
            [FromBody] RegisterRequest request,
            FamFitDbContext db) =>
        {
            // Валидация входных данных
            if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 2)
                return Results.BadRequest(new { error = "Name must be at least 2 characters." });
            if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
                return Results.BadRequest(new { error = "Valid email is required." });
            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
                return Results.BadRequest(new { error = "Password must be at least 8 characters." });

            // Проверка: email уже занят
            if (await db.Users.AnyAsync(u => u.Email == request.Email.Trim().ToLowerInvariant()))
                return Results.Conflict(new { error = "Email already registered." });

            // Хеширование пароля через BCrypt (cost factor 12)
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);

            // Создание пользователя (по умолчанию — Member)
            var user = User.Create(
                request.Name,
                request.Email,
                hashedPassword);

            db.Users.Add(user);
            await db.SaveChangesAsync();

            return Results.Created($"/api/users/{user.Id}", new
            {
                user.Id,
                user.Name,
                user.Email,
                user.Role
            });
        })
        .WithSummary("Регистрация нового пользователя")
        .Produces<UserDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        // PUT /api/auth/change-password — смена пароля (требует авторизации)
        group.MapPut("/change-password", async (
            [FromBody] ChangePasswordRequest request,
            FamFitDbContext db,
            HttpContext context) =>
        {
            var userId = context.GetUserIdFromClaim();

            var user = await db.Users.FindAsync(userId);
            if (user is null)
                return Results.NotFound();

            // Проверка текущего пароля через BCrypt
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                return Results.Unauthorized();

            // Валидация нового пароля
            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
                return Results.BadRequest(new { error = "Password must be at least 8 characters." });

            // Новый пароль не должен совпадать со старым
            if (request.NewPassword == request.CurrentPassword)
                return Results.BadRequest(new { error = "New password must be different from current." });

            // Хешируем и сохраняем
            user.ChangePassword(BCrypt.Net.BCrypt.HashPassword(request.NewPassword, workFactor: 12));
            await db.SaveChangesAsync();

            return Results.Ok(new { message = "Password changed successfully." });
        })
        .WithSummary("Смена пароля")
        .RequireAuthorization()
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        // GET /api/auth/me — текущий пользователь
        group.MapGet("/me", async (
            FamFitDbContext db,
            HttpContext context) =>
        {
            var userId = context.GetUserIdFromClaim();

            var user = await db.Users.FindAsync(userId);
            if (user is null)
                return Results.NotFound();

            return Results.Ok(new UserDto(
                user.Id,
                user.Name,
                user.Email,
                user.Role.ToString(),
                user.StreakCount));
        })
        .WithSummary("Получить данные текущего пользователя")
        .RequireAuthorization()
        .Produces<UserDto>()
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
