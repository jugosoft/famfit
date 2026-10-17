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

    // DTO для определения атрибута
    public record AttributeDefinitionDto(
        Guid Id,
        string Name,
        string DisplayName,
        AttributeDataType DataType,
        string? Unit,
        bool IsActive,
        int DisplayOrder);

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
            IMediator mediator) =>
        {
            var command = new CreateActivityTypeCommand(request.Name);

            var result = await mediator.Send(command);

            return Results.Created(
                $"/api/activity-types/{result.Id}",
                new { result.Id, result.Name });
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

        // PUT /api/activity-types/attributes/values/{id}/deactivate — деактивировать значение атрибута
        group.MapPut("/attributes/values/{id:guid}/deactivate", async (
            Guid id,
            FamFitDbContext db) =>
        {
            var attributeValue = await db.AttributeValues
                .Include(av => av.ActivityTypeAttributeValues)
                .Include(av => av.WorkoutEntryAttributeValues)
                .Include(av => av.WorkoutTemplateItemAttributeValues)
                .FirstOrDefaultAsync(av => av.Id == id);

            if (attributeValue is null)
                return Results.NotFound();

            var references = new List<string>();

            var hasTypeAttributeValues = await db.ActivityTypeAttributeValues
                .AnyAsync(atav => atav.AttributeValueId == id);
            if (hasTypeAttributeValues)
                references.Add("типов активностей");

            var hasWorkoutEntryAttrValues = await db.WorkoutEntryAttributeValues
                .AnyAsync(weav => weav.AttributeValueId == id);
            if (hasWorkoutEntryAttrValues)
                references.Add("записей тренировок");

            var hasTemplateItemAttrValues = await db.WorkoutTemplateItemAttributeValues
                .AnyAsync(wtiev => wtiev.AttributeValueId == id);
            if (hasTemplateItemAttrValues)
                references.Add("элементов шаблонов тренировок");

            if (references.Count > 0)
            {
                var referencedIn = string.Join(", ", references);
                return Results.Conflict(new
                {
                    error = "AttributeValueInUse",
                    message = $"Невозможно деактивировать значение атрибута, так как оно используется в: {referencedIn}. Сначала удалите связанные записи.",
                    references
                });
            }

            // Деактивируем значение атрибута, устанавливая его в null
            attributeValue.Update(intValue: null, decimalValue: null, boolValue: null, stringValue: null);
            await db.SaveChangesAsync();

            return Results.Ok(new { id, isActive = false });
        })
        .WithSummary("Деактивировать значение атрибута")
        .WithDescription("Сбрасывает значение атрибута. Доступно только для FamilyAdmin. Возвращает 409, если значение используется.")
        .RequireAuthorization("Admin")
        .Produces<object>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status409Conflict);

        // === Attribute Definitions Endpoints ===

        // POST /api/activity-types/attributes — создать определение атрибута
        group.MapPost("/attributes", async (
            [FromBody] CreateAttributeDefinitionRequest request,
            FamFitDbContext db) =>
        {
            // Проверка уникальности по имени
            var exists = await db.ActivityAttributeDefinitions
                .AnyAsync(d => d.Name == request.Name);
            if (exists)
                return Results.Conflict(new { error = "AttributeAlreadyExists", message = "Атрибут с таким именем уже существует." });

            var definition = ActivityAttributeDefinition.Create(
                request.Name,
                request.DisplayName,
                request.DataType,
                request.Unit,
                request.DisplayOrder);

            db.ActivityAttributeDefinitions.Add(definition);
            await db.SaveChangesAsync();

            return Results.Created(
                $"/api/activity-types/attributes/{definition.Id}",
                new { definition.Id, definition.Name, definition.DisplayName, definition.DataType, definition.Unit, definition.DisplayOrder });
        })
        .WithSummary("Создать определение атрибута")
        .RequireAuthorization("Admin")
        .Produces<Guid>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status409Conflict);

        // PUT /api/activity-types/attributes/{id} — обновить определение атрибута
        group.MapPut("/attributes/{id:guid}", async (
            Guid id,
            [FromBody] UpdateAttributeDefinitionRequest request,
            FamFitDbContext db) =>
        {
            var definition = await db.ActivityAttributeDefinitions.FindAsync(id);
            if (definition is null)
                return Results.NotFound();

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                if (request.Name.Length > 100)
                    return Results.BadRequest(new { error = "Name too long." });
                definition.UpdateName(request.Name);
            }

            if (!string.IsNullOrWhiteSpace(request.DisplayName))
            {
                if (request.DisplayName.Length > 100)
                    return Results.BadRequest(new { error = "Display name too long." });
                definition.UpdateDisplayName(request.DisplayName);
            }

            if (!string.IsNullOrWhiteSpace(request.Unit))
            {
                definition.UpdateUnit(request.Unit);
            }

            if (request.DisplayOrder.HasValue)
            {
                definition.UpdateDisplayOrder(request.DisplayOrder.Value);
            }

            await db.SaveChangesAsync();

            return Results.Ok(new { definition.Id, definition.Name, definition.DisplayName, definition.DataType, definition.Unit, definition.DisplayOrder });
        })
        .WithSummary("Обновить определение атрибута")
        .RequireAuthorization("Admin")
        .Produces<object>()
        .Produces(StatusCodes.Status404NotFound);

        // PUT /api/activity-types/attributes/{id}/deactivate — деактивировать определение атрибута
        group.MapPut("/attributes/{id:guid}/deactivate", async (
            Guid id,
            FamFitDbContext db) =>
        {
            var definition = await db.ActivityAttributeDefinitions.FindAsync(id);

            if (definition is null)
                return Results.NotFound();

            // Проверяем, используется ли определение в ActivityAttributeValue
            var hasAttributeValues = await db.AttributeValues
                .AnyAsync(av => av.DefinitionId == id);
            if (hasAttributeValues)
            {
                return Results.Conflict(new
                {
                    error = "AttributeInUse",
                    message = $"Невозможно деактивировать атрибут, так как он используется в значениях атрибутов. Сначала удалите связанные записи.",
                    references = new[] { "значений атрибутов" }
                });
            }

            definition.Update(isActive: false);
            await db.SaveChangesAsync();

            return Results.Ok(new { id, isActive = false });
        })
        .WithSummary("Деактивировать атрибут")
        .WithDescription("Делает атрибут неактивным. Доступно только для FamilyAdmin. Возвращает 409, если атрибут используется.")
        .RequireAuthorization("Admin")
        .Produces<object>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status409Conflict);

        // GET /api/activity-types/attributes — список определений
        group.MapGet("/attributes", async (IMediator mediator) =>
        {
            var definitions = await mediator.Send(new GetActivityAttributeDefinitionsQuery());
            return Results.Ok(definitions.Select(d => new AttributeDefinitionDto(
                d.Id,
                d.Name,
                d.DisplayName,
                d.DataType,
                d.Unit,
                d.IsActive,
                d.DisplayOrder)).ToList());
        })
        .WithSummary("Получить список определений атрибутов")
        .WithDescription("Возвращает все активные определения атрибутов для использования в UI.")
        .Produces<List<AttributeDefinitionDto>>()
        .Produces(StatusCodes.Status500InternalServerError);

        // PUT /api/activity-types/{id} — обновить название
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

            await db.SaveChangesAsync();

            return Results.Ok(new { id, activityType.Name });
        })
        .WithSummary("Обновить тип активности")
        .RequireAuthorization("Admin")
        .Produces<object>()
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}

public record CreateActivityTypeRequest(string Name);

public record UpdateActivityTypeRequest(string? Name);

public record CreateAttributeDefinitionRequest(
    string Name,
    string DisplayName,
    Domain.Enums.AttributeDataType DataType,
    string? Unit,
    int DisplayOrder);

public record UpdateAttributeDefinitionRequest(
    string? Name,
    string? DisplayName,
    string? Unit,
    int? DisplayOrder);
