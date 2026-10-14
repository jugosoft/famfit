using FamFit.Application.Commands.WorkoutTemplates;
using FamFit.Application.Queries.WorkoutTemplates;
using FamFit.Infrastructure.Data;
using FamFit.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FamFit.WebApi.Endpoints;

/// <summary>
/// Маршруты API для WorkoutTemplate.
/// </summary>
public static class WorkoutTemplateEndpoints
{
    public static IEndpointRouteBuilder MapWorkoutTemplateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/workout-templates")
            .WithTags("WorkoutTemplates")
            .WithOpenApi();

        // GET /api/workout-templates — список шаблонов пользователя
        group.MapGet("/", async (
            IMediator mediator,
            HttpContext context) =>
        {
            var userId = context.GetUserIdFromClaim();

            var result = await mediator.Send(new GetUserWorkoutTemplatesQuery(userId));

            return Results.Ok(result.Select(t => new
            {
                t.Id,
                t.UserId,
                t.Name,
                t.Description,
                ItemsCount = t.Items.Count
            }));
        })
        .WithSummary("Получить список шаблонов тренировок")
        .WithDescription("Возвращает все шаблоны текущего пользователя.")
        .RequireAuthorization()
        .Produces<List<object>>();

        // GET /api/workout-templates/{id} — получить шаблон с элементами
        group.MapGet("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            HttpContext context) =>
        {
            var userId = context.GetUserIdFromClaim();

            var result = await mediator.Send(new GetWorkoutTemplateQuery(id));

            // Проверка: шаблон принадлежит текущему пользователю
            if (result.UserId != userId)
                return Results.StatusCode(403);

            return Results.Ok(new
            {
                result.Id,
                result.UserId,
                result.Name,
                result.Description,
                Items = result.Items.OrderBy(i => i.DisplayOrder).Select(i => new
                {
                    i.Id,
                    i.ActivityTypeId,
                    i.Sets,
                    i.Reps,
                    i.Weight,
                    i.DisplayOrder,
                    ActivityType = new
                    {
                        i.ActivityType!.Id,
                        i.ActivityType.Name,
                        i.ActivityType.UnitType
                    }
                })
            });
        })
        .WithSummary("Получить шаблон тренировки по ID")
        .WithDescription("Возвращает шаблон со всеми элементами. Доступно только владельцу.")
        .RequireAuthorization()
        .Produces<object>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status403Forbidden);

        // POST /api/workout-templates — создать новый шаблон
        group.MapPost("/", async (
            CreateWorkoutTemplateRequest request,
            IMediator mediator,
            HttpContext context) =>
        {
            var userId = context.GetUserIdFromClaim();

            var result = await mediator.Send(new CreateWorkoutTemplateCommand(
                userId,
                request.Name,
                request.Description));

            return Results.Created(
                $"/api/workout-templates/{result.Id}",
                new
                {
                    result.Id,
                    result.UserId,
                    result.Name,
                    result.Description
                });
        })
        .WithSummary("Создать новый шаблон тренировки")
        .WithDescription("Создаёт новый шаблон для текущего пользователя.")
        .RequireAuthorization()
        .Produces<object>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        // POST /api/workout-templates/{id}/items — добавить элемент в шаблон
        group.MapPost("/{id:guid}/items", async (
            Guid id,
            AddWorkoutTemplateItemRequest request,
            IMediator mediator,
            HttpContext context) =>
        {
            var userId = context.GetUserIdFromClaim();

            var db = context.RequestServices.GetRequiredService<FamFitDbContext>();
            var template = await db.WorkoutTemplates.FindAsync(id);

            if (template is null)
                return Results.NotFound();

            if (template.UserId != userId)
                return Results.StatusCode(403);

            var result = await mediator.Send(new AddWorkoutTemplateItemCommand(
                id,
                request.ActivityTypeId,
                request.Sets,
                request.Reps,
                request.Weight));

            return Results.Created(
                $"/api/workout-templates/{id}/items/{result.Id}",
                new
                {
                    result.Id,
                    result.TemplateId,
                    result.ActivityTypeId,
                    result.Sets,
                    result.Reps,
                    result.Weight,
                    result.DisplayOrder
                });
        })
        .WithSummary("Добавить элемент в шаблон")
        .WithDescription("Добавляет тип активности в шаблон с дефолтными значениями.")
        .RequireAuthorization()
        .Produces<object>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status403Forbidden);

        // DELETE /api/workout-templates/{id} — удалить шаблон
        group.MapDelete("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            HttpContext context) =>
        {
            var userId = context.GetUserIdFromClaim();

            var db = context.RequestServices.GetRequiredService<FamFitDbContext>();
            var template = await db.WorkoutTemplates.FindAsync(id);

            if (template is null)
                return Results.NotFound();

            if (template.UserId != userId)
                return Results.StatusCode(403);

            await mediator.Send(new DeleteWorkoutTemplateCommand(id));

            return Results.Ok(new { templateId = id, deleted = true });
        })
        .WithSummary("Удалить шаблон тренировки")
        .WithDescription("Удаляет шаблон и все его элементы.")
        .RequireAuthorization()
        .Produces<object>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status403Forbidden);

        // DELETE /api/workout-templates/{id}/items/{activityTypeId} — удалить элемент
        group.MapDelete("/{id:guid}/items/{activityTypeId:guid}", async (
            Guid id,
            Guid activityTypeId,
            IMediator mediator,
            HttpContext context) =>
        {
            var userId = context.GetUserIdFromClaim();

            var db = context.RequestServices.GetRequiredService<FamFitDbContext>();
            var template = await db.WorkoutTemplates.FindAsync(id);

            if (template is null)
                return Results.NotFound();

            if (template.UserId != userId)
                return Results.StatusCode(403);

            await mediator.Send(new RemoveWorkoutTemplateItemCommand(id, activityTypeId));

            return Results.Ok(new { templateId = id, activityTypeId, removed = true });
        })
        .WithSummary("Удалить элемент из шаблона")
        .WithDescription("Удаляет тип активности из шаблона тренировки.")
        .RequireAuthorization()
        .Produces<object>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status403Forbidden);

        return app;
    }
}
