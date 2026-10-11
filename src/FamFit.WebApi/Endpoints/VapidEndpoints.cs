using FamFit.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace FamFit.WebApi.Endpoints;

/// <summary>
/// Маршруты API для VAPID-конфигурации.
/// </summary>
public static class VapidEndpoints
{
    public static IEndpointRouteBuilder MapVapidEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/vapid")
            .WithTags("Vapid")
            .WithOpenApi();

        // GET /api/vapid/public-key — получить публичный ключ для фронтенда
        group.MapGet("/public-key", (IOptions<WebPushOptions> options) =>
        {
            return Results.Ok(new
            {
                publicKey = options.Value.PublicKey
            });
        })
        .WithSummary("Получить VAPID публичный ключ")
        .WithDescription("Возвращает VAPID публичный ключ, который фронтенд использует при регистрации push-подписки.")
        .Produces<VapidPublicKeyResponse>()
        .Produces(StatusCodes.Status500InternalServerError);

        return app;
    }
}

public record VapidPublicKeyResponse(string PublicKey);
