using FamFit.Application.Commands.Workouts;
using FamFit.Application.Queries.Workouts;
using FamFit.Domain.Enums;
using FamFit.Infrastructure.Data;
using FamFit.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CreateWorkoutRequest = FamFit.WebApi.Models.CreateWorkoutRequest;

namespace FamFit.WebApi.Endpoints;

/// <summary>
/// Маршруты API для Workout.
/// </summary>
public static class WorkoutEndpoints
{
    // DTO для значения атрибута записи тренировки
    public record EntryAttributeValueDto(
        Guid Id,
        string DefinitionName,
        string DisplayName,
        AttributeDataType DataType,
        string? Unit,
        object? Value);

    // DTO для записи тренировки
    public record WorkoutEntryDto(
        Guid Id,
        Guid ActivityTypeId,
        IReadOnlyList<EntryAttributeValueDto> Attributes,
        object? ActivityType);

    public static IEndpointRouteBuilder MapWorkoutEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/workouts")
            .WithTags("Workouts")
            .WithOpenApi();

        // GET /api/workouts/{id} — получить тренировку с записями
        group.MapGet("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            HttpContext context) =>
        {
            var userId = context.GetUserIdFromClaim();

            var result = await mediator.Send(new GetWorkoutQuery(id));

            // Проверка: тренировка принадлежит текущему пользователю
            if (result.UserId != userId)
                return Results.StatusCode(403);

            var entries = result.Entries.Select(e => new WorkoutEntryDto(
                e.Id,
                e.ActivityTypeId,
                e.WorkoutEntryAttributeValues
                    .Where(aw => aw.AttributeValue != null)
                    .OrderBy(aw => aw.AttributeValue!.Definition!.DisplayOrder)
                    .ThenBy(aw => aw.AttributeValue!.Definition!.Name)
                    .Select(aw => new EntryAttributeValueDto(
                        aw.AttributeValue!.Id,
                        aw.AttributeValue!.Definition!.Name,
                        aw.AttributeValue!.Definition!.DisplayName,
                        aw.AttributeValue!.Definition!.DataType,
                        aw.AttributeValue!.Definition!.Unit,
                        aw.AttributeValue!.GetValue()))
                    .ToList(),
                e.ActivityType != null ? new
                {
                    e.ActivityType.Id,
                    e.ActivityType.Name
                } : null))
                .ToList();

            return Results.Ok(new
            {
                result.Id,
                result.UserId,
                result.Name,
                result.StartTime,
                result.EndTime,
                result.IsCompleted,
                Entries = entries
            });
        })
        .WithSummary("Получить тренировку по ID")
        .WithDescription("Возвращает тренировку со всеми записями. Доступно только владельцу.")
        .RequireAuthorization()
        .Produces<object>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status403Forbidden);

        // GET /api/workouts — список тренировок пользователя
        group.MapGet("/", async (
            IMediator mediator,
            HttpContext context) =>
        {
            var userId = context.GetUserIdFromClaim();

            var result = await mediator.Send(new GetUserWorkoutsQuery(userId));

            return Results.Ok(result.Select(w => new
            {
                w.Id,
                w.Name,
                w.StartTime,
                w.EndTime,
                w.IsCompleted
            }));
        })
        .WithSummary("Получить список тренировок пользователя")
        .WithDescription("Возвращает все тренировки текущего пользователя, отсортированные по дате.")
        .RequireAuthorization()
        .Produces<List<object>>();

        // POST /api/workouts — создать новую тренировку
        group.MapPost("/", async (
            [FromBody] CreateWorkoutRequest request,
            IMediator mediator,
            HttpContext context) =>
        {
            var userId = context.GetUserIdFromClaim();

            var result = await mediator.Send(new CreateWorkoutCommand(
                userId,
                request.Name));

            return Results.Created(
                $"/api/workouts/{result.Id}",
                new
                {
                    result.Id,
                    result.UserId,
                    result.Name,
                    result.StartTime,
                    result.IsCompleted
                });
        })
        .WithSummary("Создать новую тренировку")
        .WithDescription("Создаёт новую тренировку для текущего пользователя.")
        .RequireAuthorization()
        .Produces<object>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        // POST /api/workouts/{id}/entries — добавить запись в тренировку
        group.MapPost("/{id:guid}/entries", async (
            Guid id,
            [FromBody] AddWorkoutEntryCommand command,
            IMediator mediator,
            HttpContext context) =>
        {
            var userId = context.GetUserIdFromClaim();

            var db = context.RequestServices.GetRequiredService<FamFitDbContext>();
            var workout = await db.Workouts.FindAsync(id);

            if (workout is null)
                return Results.NotFound();

            if (workout.UserId != userId)
                return Results.StatusCode(403);

            if (workout.IsCompleted)
                return Results.Conflict("Cannot add entries to a completed workout.");

            var result = await mediator.Send(command);

            return Results.Created(
                $"/api/workouts/{id}/entries/{result.Id}",
                new
                {
                    result.Id,
                    result.WorkoutId,
                    result.ActivityTypeId
                });
        })
        .WithSummary("Добавить запись в тренировку")
        .WithDescription("Добавляет новую активность в текущую тренировку.")
        .RequireAuthorization()
        .Produces<object>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        // POST /api/workouts/{id}/complete — завершить тренировку
        group.MapPost("/{id:guid}/complete", async (
            Guid id,
            IMediator mediator,
            HttpContext context) =>
        {
            var userId = context.GetUserIdFromClaim();

            var db = context.RequestServices.GetRequiredService<FamFitDbContext>();
            var workout = await db.Workouts.FindAsync(id);

            if (workout is null)
                return Results.NotFound();

            if (workout.UserId != userId)
                return Results.StatusCode(403);

            if (workout.IsCompleted)
                return Results.Conflict("Workout is already completed.");

            await mediator.Send(new CompleteWorkoutCommand(id));

            return Results.Ok(new { workoutId = id, completed = true });
        })
        .WithSummary("Завершить тренировку")
        .WithDescription("Завершает тренировку и автоматически создаёт ActivityLog для каждой записи.")
        .RequireAuthorization()
        .Produces<object>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        // DELETE /api/workouts/{id} — удалить тренировку
        group.MapDelete("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            HttpContext context) =>
        {
            var userId = context.GetUserIdFromClaim();

            var db = context.RequestServices.GetRequiredService<FamFitDbContext>();
            var workout = await db.Workouts.FindAsync(id);

            if (workout is null)
                return Results.NotFound();

            if (workout.UserId != userId)
                return Results.StatusCode(403);

            await mediator.Send(new DeleteWorkoutCommand(id));

            return Results.Ok(new { workoutId = id, deleted = true });
        })
        .WithSummary("Удалить тренировку")
        .WithDescription("Удаляет тренировку и все её записи.")
        .RequireAuthorization()
        .Produces<object>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status403Forbidden);

        return app;
    }
}
