namespace FamFit.WebApi.Endpoints;

/// <summary>
/// Маршруты API для Push-подписок.
/// </summary>
public static class PushSubscriptionEndpoints
{
    public static IEndpointRouteBuilder MapPushSubscriptionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/push-subscriptions")
            .WithTags("PushSubscriptions")
            .WithOpenApi();

        // POST /api/push-subscriptions — зарегистрировать / обновить подписку
        group.MapPost("/", async (
            RegisterPushSubscriptionCommand command,
            IMediator mediator,
            HttpContext context) =>
        {
            var userId = context.GetUserIdFromClaim();

            var result = await mediator.Send(new RegisterPushSubscriptionCommand(
                userId,
                command.Endpoint,
                command.P256dh,
                command.Auth));

            return Results.Ok(new
            {
                result.Id,
                result.UserId,
                result.Endpoint,
                result.CreatedAt
            });
        })
        .WithSummary("Зарегистрировать или обновить push-подписку")
        .WithDescription("Создаёт новую push-подписку или обновляет существующую (upsert по endpoint).")
        .RequireAuthorization()
        .Produces<object>()
        .Produces(StatusCodes.Status400BadRequest);

        // DELETE /api/push-subscriptions/{id} — удалить подписку
        group.MapDelete("/{id:guid}", async (
            Guid id,
            FamFitDbContext db,
            HttpContext context) =>
        {
            var userId = context.GetUserIdFromClaim();

            var subscription = await db.PushSubscriptions
                .FirstOrDefaultAsync(ps => ps.Id == id && ps.UserId == userId);

            if (subscription is null)
                return Results.NotFound();

            db.PushSubscriptions.Remove(subscription);
            await db.SaveChangesAsync();

            return Results.Ok(new { id, deleted = true });
        })
        .WithSummary("Удалить push-подписку")
        .WithDescription("Удаляет push-подписку пользователя.")
        .RequireAuthorization()
        .Produces<object>()
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
