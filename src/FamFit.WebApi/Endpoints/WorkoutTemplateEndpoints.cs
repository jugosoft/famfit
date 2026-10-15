using FamFit.Application.Commands.WorkoutTemplates;
using FamFit.Application.Queries.WorkoutTemplates;
using FamFit.Domain.Enums;
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
    // DTO для значения атрибута элемента шаблона
    public record TemplateItemAttributeValueDto(
        Guid Id,
        string DefinitionName,
        string DisplayName,
        AttributeDataType DataType,
        string? Unit,
        object? Value);

    // DTO для элемента шаблона
    public record WorkoutTemplateItemDto(
        Guid Id,
        Guid ActivityTypeId,
        int DisplayOrder,
        IReadOnlyList<TemplateItemAttributeValueDto> Attributes,
        object? ActivityType);

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

            var items = result.Items.Select(i => new WorkoutTemplateItemDto(
                i.Id,
                i.ActivityTypeId,
                i.DisplayOrder,
                i.WorkoutTemplateItemAttributeValues
                    .Where(awi => awi.AttributeValue != null)
                    .OrderBy(awi => awi.AttributeValue!.Definition!.DisplayOrder)
                    .ThenBy(awi => awi.AttributeValue!.Definition!.Name)
                    .Select(awi => new TemplateItemAttributeValueDto(
                        awi.AttributeValue!.Id,
                        awi.AttributeValue!.Definition!.Name,
                        awi.AttributeValue!.Definition!.DisplayName,
                        awi.AttributeValue!.Definition!.DataType,
                        awi.AttributeValue!.Definition!.Unit,
                        awi.AttributeValue!.GetValue()))
                    .ToList(),
                i.ActivityType != null ? new
                {
                    i.ActivityType.Id,
                    i.ActivityType.Name,
                    i.ActivityType.UnitType
                } : null))
                .ToList();

            return Results.Ok(new
            {
                result.Id,
                result.UserId,
                result.Name,
                result.Description,
                Items = items.OrderBy(i => i.DisplayOrder)
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
                request.AttributeValues.Select(av => new Application.Commands.ActivityAttributes.AttributeValueInput(
                    av.DefinitionName,
                    av.IntValue,
                    av.DecimalValue,
                    av.BoolValue,
                    av.StringValue)).ToList(),
                request.DisplayOrder));

            return Results.Created(
                $"/api/workout-templates/{id}/items/{result.Id}",
                new
                {
                    result.Id,
                    result.TemplateId,
                    result.ActivityTypeId,
                    result.DisplayOrder
                });
        })
        .WithSummary("Добавить элемент в шаблон")
        .WithDescription("Добавляет тип активности в шаблон с атрибутами.")
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

        // DELETE /api/workout-templates/{id}/items/{activityTypeId:guid} — удалить элемент
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
