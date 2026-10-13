using FamFit.Application.Commands.ActivityTypes;
using FamFit.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamFit.WebApi.Endpoints;

/// <summary>
/// Маршруты API для ActivityType.
/// </summary>
public static class ActivityTypeEndpoints
{
    public static IEndpointRouteBuilder MapActivityTypeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/activity-types")
            .WithTags("ActivityTypes")
            .WithOpenApi();

        // GET /api/activity-types — все активные типы (публичный)
        group.MapGet("/", async (FamFitDbContext db) =>
        {
            var types = await db.ActivityTypes
                .Where(at => at.IsActive)
                .Select(at => new
                {
                    at.Id,
                    at.Name,
                    at.UnitType
                })
                .OrderBy(at => at.Name)
                .ToListAsync();

            return Results.Ok(types);
        })
        .WithSummary("Получить список всех активных типов активностей")
        .WithDescription("Возвращает все активные типы активностей для отображения в UI.")
        .Produces<List<object>>()
        .Produces(StatusCodes.Status500InternalServerError);

        // POST /api/activity-types — создать новый тип (только Admin)
        group.MapPost("/", async (
            CreateActivityTypeCommand command,
            IMediator mediator) =>
        {
            var result = await mediator.Send(command);

            return Results.Created(
                $"/api/activity-types/{result.Id}",
                new
                {
                    result.Id,
                    result.Name,
                    result.UnitType
                });
        })
        .WithSummary("Создать новый тип активности")
        .WithDescription("Создаёт новый тип активности. Доступно только для FamilyAdmin.")
        .RequireAuthorization("Admin")
        .Produces<Guid>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status500InternalServerError);

        // PUT /api/activity-types/{id}/deactivate — деактивировать (только Admin)
        group.MapPut("/{id:guid}/deactivate", async (
            Guid id,
            FamFitDbContext db) =>
        {
            var activityType = await db.ActivityTypes.FindAsync(id);

            if (activityType is null)
                return Results.NotFound();

            activityType.Deactivate();
            await db.SaveChangesAsync();

            return Results.Ok(new { id, isActive = false });
        })
        .WithSummary("Деактивировать тип активности")
        .WithDescription("Делает тип активности неактивным. Доступно только для FamilyAdmin.")
        .RequireAuthorization("Admin")
        .Produces<object>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status403Forbidden);

        return app;
    }
}
