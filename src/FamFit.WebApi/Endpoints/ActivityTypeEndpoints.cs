using FamFit.Application.Commands.ActivityAttributes;
using FamFit.Application.Commands.ActivityTypes;
using FamFit.Application.Queries.ActivityAttributes;
using FamFit.Domain.Entities;
using FamFit.Domain.Enums;
using FamFit.Infrastructure.Data;
using FamFit.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FamFit.WebApi.Endpoints;

/// <summary>
/// Маршруты API для ActivityType и Attribute Definitions.
/// </summary>
public static class ActivityTypeEndpoints
{
    // DTO для значения атрибута
    public record AttributeValueDto(
        Guid Id,
        string DefinitionName,
        string DisplayName,
        AttributeDataType DataType,
        string? Unit,
        object? Value);

    // DTO для ActivityType с атрибутами
    public record ActivityTypeDto(
        Guid Id,
        string Name,
        bool IsActive,
        IReadOnlyList<AttributeValueDto> Attributes);



    // DTO для запроса обновления атрибутов
    public record UpdateAttributesRequest(
        IReadOnlyList<AttributeInput> AttributeValues);

    public record AttributeInput(
        string DefinitionName,
        int? IntValue,
        decimal? DecimalValue,
        bool? BoolValue,
        string? StringValue);

    public static IEndpointRouteBuilder MapActivityTypeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/activity-types")
            .WithTags("ActivityTypes")
            .WithOpenApi();

        // GET /api/activity-types — список всех активных типов
        group.MapGet("/", async (FamFitDbContext db) =>
        {
            var types = await db.ActivityTypes
                .Where(at => at.IsActive)
                .Select(at => new
                {
                    at.Id,
                    at.Name,
                    at.IsActive,
                    Attributes = at.ActivityTypeAttributeValues
                        .Where(atav => atav.AttributeValue != null)
                        .OrderBy(atav => atav.AttributeValue!.Definition!.DisplayOrder)
                        .ThenBy(atav => atav.AttributeValue!.Definition!.Name)
                        .Select(atav => new
                        {
                            atav.AttributeValue!.Id,
                            DefinitionName = atav.AttributeValue!.Definition!.Name,
                            DisplayName = atav.AttributeValue!.Definition!.DisplayName,
                            DataType = atav.AttributeValue!.Definition!.DataType,
                            Unit = atav.AttributeValue!.Definition!.Unit,
                            Value = atav.AttributeValue!.GetValue()
                        })
                        .ToList()
                })
                .OrderBy(at => at.Name)
                .ToListAsync();

            var result = types.Select(at => new ActivityTypeDto(
                at.Id,
                at.Name,
                at.IsActive,
                at.Attributes.Select(a => new AttributeValueDto(
                    a.Id,
                    a.DefinitionName,
                    a.DisplayName,
                    a.DataType,
                    a.Unit,
                    a.Value)).ToList()
            )).ToList();

            return Results.Ok(result);
        })
        .WithSummary("Получить список всех активных типов активностей")
        .WithDescription("Возвращает все активные типы активностей с их атрибутами.")
        .Produces<List<ActivityTypeDto>>()
        .Produces(StatusCodes.Status500InternalServerError);

        // POST /api/activity-types — создать новый тип
        group.MapPost("/", async (
            [FromBody] CreateActivityTypeRequest request,
            FamFitDbContext db) =>
        {
            // Создаём тип активности
            var activityType = ActivityType.Create(request.Name);
            db.ActivityTypes.Add(activityType);
            await db.SaveChangesAsync();

            // Создаём атрибуты, если они переданы
            if (request.AttributeValues != null && request.AttributeValues.Count > 0)
            {
                foreach (var attrInput in request.AttributeValues)
                {
                    var definition = await db.ActivityAttributeDefinitions
                        .FirstOrDefaultAsync(d => d.Name == attrInput.DefinitionName);

                    if (definition is null)
                        continue;

                    // Всегда создаём новое значение атрибута для этого типа активности
                    var attributeValue = ActivityAttributeValue.Create(
                        definition.Id,
                        attrInput.IntValue,
                        attrInput.DecimalValue,
                        attrInput.BoolValue ?? false,
                        attrInput.StringValue);

                    db.AttributeValues.Add(attributeValue);

                    // Создаём связь
                    db.ActivityTypeAttributeValues.Add(
                        ActivityTypeAttributeValue.Create(activityType.Id, attributeValue.Id));
                }

                await db.SaveChangesAsync();
            }

            return Results.Created(
                $"/api/activity-types/{activityType.Id}",
                new { activityType.Id, activityType.Name });
        })
        .WithSummary("Создать новый тип активности")
        .WithDescription("Создаёт новый тип активности. Доступно только для FamilyAdmin.")
        .RequireAuthorization("Admin")
        .Produces<Guid>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status500InternalServerError);

        // PUT /api/activity-types/{id}/deactivate — деактивировать
        group.MapPut("/{id:guid}/deactivate", async (
            Guid id,
            FamFitDbContext db) =>
        {
            var activityType = await db.ActivityTypes
                .Include(at => at.ActivityTypeAttributeValues)
                .Include(at => at.WorkoutTemplateItems)
                .Include(at => at.ActivityLogs)
                .FirstOrDefaultAsync(at => at.Id == id);

            if (activityType is null)
                return Results.NotFound();

            // Check if activity type is referenced by other entities
            var references = new List<string>();

            var hasTypeAttributeValues = await db.ActivityTypeAttributeValues
                .AnyAsync(atav => atav.ActivityTypeId == id);
            if (hasTypeAttributeValues)
                references.Add("шаблонов атрибутов типов активностей");

            var hasTemplateItems = await db.WorkoutTemplateItems
                .AnyAsync(wti => wti.ActivityTypeId == id);
            if (hasTemplateItems)
                references.Add("элементов шаблонов тренировок");

            var hasWorkoutEntries = await db.WorkoutEntries
                .AnyAsync(we => we.ActivityTypeId == id);
            if (hasWorkoutEntries)
                references.Add("записей тренировок");

            var hasActivityLogs = await db.ActivityLogs
                .AnyAsync(al => al.ActivityTypeId == id);
            if (hasActivityLogs)
                references.Add("журналов активности");

            if (references.Count > 0)
            {
                var referencedIn = string.Join(", ", references);
                return Results.Conflict(new
                {
                    error = "ActivityTypeInUse",
                    message = $"Невозможно деактивировать тип активности, так как он используется в: {referencedIn}. Сначала удалите или обновите связанные записи.",
                    references
                });
            }

            activityType.Deactivate();
            await db.SaveChangesAsync();

            return Results.Ok(new { id, isActive = false });
        })
        .WithSummary("Деактивировать тип активности")
        .WithDescription("Делает тип активности неактивным. Доступно только для FamilyAdmin. Возвращает ошибку 409, если тип используется в других сущностях.")
        .RequireAuthorization("Admin")
        .Produces<object>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status409Conflict);

        // PUT /api/activity-types/{id}/attributes — обновить атрибуты
        group.MapPut("/{id:guid}/attributes", async (
            Guid id,
            UpdateAttributesRequest request,
            IMediator mediator) =>
        {
            await mediator.Send(new UpdateActivityTypeAttributesCommand(
                id,
                request.AttributeValues.Select(av => new AttributeValueInput(
                    av.DefinitionName,
                    av.IntValue,
                    av.DecimalValue,
                    av.BoolValue,
                    av.StringValue)).ToList()));

            return Results.Ok(new { activityTypeId = id, updated = true });
        })
        .WithSummary("Обновить атрибуты типа активности")
        .WithDescription("Устанавливает значения атрибутов для типа активности.")
        .RequireAuthorization("Admin")
        .Produces<object>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status403Forbidden);

        // PUT /api/activity-types/{id} — обновить название и атрибуты
        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdateActivityTypeRequest request,
            FamFitDbContext db) =>
        {
            var activityType = await db.ActivityTypes.FindAsync(id);
            if (activityType is null)
                return Results.NotFound();

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                if (request.Name.Length < 2)
                    return Results.BadRequest(new { error = "Name must be at least 2 characters." });
                activityType.UpdateName(request.Name);
            }

            // Обновляем атрибуты, если они переданы
            if (request.AttributeValueIds != null && request.AttributeValueIds.Count > 0)
            {
                // Удаляем старые связи
                var existingLinks = db.ActivityTypeAttributeValues
                    .Where(atav => atav.ActivityTypeId == id);
                db.ActivityTypeAttributeValues.RemoveRange(existingLinks);

                // Создаём новые связи с переданными attributeValueId
                foreach (var attributeValueId in request.AttributeValueIds)
                {
                    var attributeValue = await db.AttributeValues.FindAsync(attributeValueId);
                    if (attributeValue is null)
                        continue;

                    db.ActivityTypeAttributeValues.Add(
                        ActivityTypeAttributeValue.Create(id, attributeValue.Id));
                }
            }

            await db.SaveChangesAsync();

            return Results.Ok(new { id, activityType.Name });
        })
        .WithSummary("Обновить тип активности")
        .RequireAuthorization("Admin")
        .Produces<object>()
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static bool MatchValue(ActivityAttributeValue value, AttributeValueInput input, Domain.Enums.AttributeDataType dataType)
    {
        return dataType switch
        {
            Domain.Enums.AttributeDataType.Integer => value.IntValue == input.IntValue,
            Domain.Enums.AttributeDataType.Decimal => value.DecimalValue == input.DecimalValue,
            Domain.Enums.AttributeDataType.Boolean => value.BoolValue == (input.BoolValue ?? false),
            Domain.Enums.AttributeDataType.String => value.StringValue == input.StringValue,
            _ => false
        };
    }
}

public record CreateActivityTypeRequest(
    string Name,
    IReadOnlyList<AttributeValueInput>? AttributeValues);

public record UpdateActivityTypeRequest(
    string? Name,
    IReadOnlyList<Guid>? AttributeValueIds);


