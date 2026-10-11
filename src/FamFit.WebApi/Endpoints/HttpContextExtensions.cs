using System.Security.Claims;
using FamFit.Domain.Enums;

namespace FamFit.WebApi.Endpoints;

/// <summary>
/// Расширения для работы с Claims из HttpContext.
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Извлекает UserId из JWT-claim "sub" (Subject).
    /// </summary>
    public static Guid GetUserIdFromClaim(this HttpContext context)
    {
        var sub = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue("sub");

        if (sub is null || !Guid.TryParse(sub, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        return userId;
    }

    /// <summary>
    /// Проверяет, является ли пользователь Admin.
    /// </summary>
    public static bool IsAdmin(this HttpContext context)
    {
        var role = context.User.FindFirstValue(ClaimTypes.Role)
            ?? context.User.FindFirstValue("role");

        return role == Role.Admin.ToString();
    }
}
