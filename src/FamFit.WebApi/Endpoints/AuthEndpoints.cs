using FamFit.Application.Interfaces;
using FamFit.Domain.Entities;
using FamFit.Domain.Enums;
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
    /// <summary>
    /// Запрос на вход с email + password.
    /// </summary>
    public record LoginRequest(string Email, string Password);

    /// <summary>
    /// Ответ с JWT-токеном.
    /// </summary>
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

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth")
            .WithOpenApi();

        // POST /api/auth/login — вход пользователя
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

            // 2. Проверяем пароль (в реальном проекте — через Application-сервис хеширования)
            var passwordHasher = new PasswordHasher<User>();
            var result = passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password);

            if (result != PasswordVerificationResult.Success)
                return Results.Unauthorized();

            // 3. Генерируем JWT-токен
            var jwtSection = configuration.GetSection("Jwt");
            var secret = jwtSection["Secret"]
                ?? throw new InvalidOperationException("JWT secret not configured.");
            var issuer = jwtSection["Issuer"] ?? "FamFit.WebApi";
            var audience = jwtSection["Audience"] ?? "FamFitClient";

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
        .WithDescription("Аутентификация пользователя по email и паролю. Возвращает JWT-токен.")
        .Produces<LoginResponse>()
        .Produces(StatusCodes.Status401Unauthorized);

        // POST /api/auth/register — регистрация нового пользователя
        group.MapPost("/register", async (
            [FromBody] RegisterRequest request,
            FamFitDbContext db) =>
        {
            // Проверка: email уже занят
            if (await db.Users.AnyAsync(u => u.Email == request.Email.Trim().ToLowerInvariant()))
                return Results.Conflict(new { error = "Email already registered." });

            // Создание пользователя (по умолчанию — Member)
            var user = User.Create(
                request.Name,
                request.Email,
                request.Password);

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
        .WithDescription("Создаёт нового пользователя с ролью FamilyMember.")
        .Produces<UserDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status409Conflict);

        return app;
    }

    /// <summary>
    /// Запрос на регистрацию.
    /// </summary>
    public record RegisterRequest(string Name, string Email, string Password);
}
