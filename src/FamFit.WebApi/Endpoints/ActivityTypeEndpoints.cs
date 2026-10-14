using FamFit.Application.Commands.ActivityTypes;
using FamFit.Application.Commands.ActivityTypeAttributes;
using FamFit.Infrastructure.Data;
using FamFit.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using UpdateActivityTypeAttributesRequest = FamFit.WebApi.Models.UpdateActivityTypeAttributesRequest;

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
        [AllowAnonymous]
        group.MapGet("/", async (FamFitDbContext db) =>
        {
                .Where(at => at.IsActive)
                .Select(at => new
                {
                    at.Id,
                    at.Name,
                    at.UnitType,
                    Attributes = at.ActivityTypeAttributes != null ? new
                    {
                        at.ActivityTypeAttributes.Sets,
                        at.ActivityTypeAttributes.Reps,
                        at.ActivityTypeAttributes.Weight
                    } : (object?)null
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

        // PUT /api/activity-types/{id}/attributes — обновить атрибуты (только Admin)
        group.MapPut("/{id:guid}/attributes", async (
            Guid id,
            UpdateActivityTypeAttributesRequest request,
            IMediator mediator) =>
        {
            await mediator.Send(new UpdateActivityTypeAttributesCommand(
                id,
                request.Sets,
                request.Reps,
                request.Weight));

            return Results.Ok(new { activityTypeId = id, updated = true });
        })
        .WithSummary("Обновить атрибуты типа активности")
        .WithDescription("Устанавливает подходы, повторения и вес для типа активности.")
        .RequireAuthorization("Admin")
        .Produces<object>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status403Forbidden);

        return app;
    }
}
