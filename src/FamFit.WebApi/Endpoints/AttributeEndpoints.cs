using FamFit.Application.Commands.ActivityAttributes;
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
/// Маршруты API для Attribute Definitions.
/// </summary>
public static class AttributeEndpoints
{
    // DTO для определения атрибута
    public record AttributeDefinitionDto(
        Guid Id,
        string Name,
        string DisplayName,
        AttributeDataType DataType,
        string? Unit,
        bool IsActive);

    public static IEndpointRouteBuilder MapAttributeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/attributes")
            .WithTags("Attributes")
            .WithOpenApi();

        // GET /api/attributes — список определений
        group.MapGet("/", async (IMediator mediator) =>
        {
            var definitions = await mediator.Send(new GetActivityAttributeDefinitionsQuery());
            return Results.Ok(definitions.Select(d => new AttributeDefinitionDto(
                d.Id,
                d.Name,
                d.DisplayName,
                d.DataType,
                d.Unit,
                d.IsActive)).ToList());
        })
        .WithSummary("Получить список определений атрибутов")
        .WithDescription("Возвращает все активные определения атрибутов для использования в UI.")
        .Produces<List<AttributeDefinitionDto>>()
        .Produces(StatusCodes.Status500InternalServerError);

        // POST /api/attributes — создать определение атрибута
        group.MapPost("/", async (
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
                request.Unit);

            db.ActivityAttributeDefinitions.Add(definition);
            await db.SaveChangesAsync();

            return Results.Created(
                $"/api/attributes/{definition.Id}",
                new { definition.Id, definition.Name, definition.DisplayName, definition.DataType, definition.Unit });
        })
        .WithSummary("Создать определение атрибута")
        .RequireAuthorization("Admin")
        .Produces<Guid>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status409Conflict);

        // PUT /api/attributes/{id} — обновить определение атрибута
        group.MapPut("/{id:guid}", async (
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

            await db.SaveChangesAsync();

            return Results.Ok(new { definition.Id, definition.Name, definition.DisplayName, definition.DataType, definition.Unit });
        })
        .WithSummary("Обновить определение атрибута")
        .RequireAuthorization("Admin")
        .Produces<object>()
        .Produces(StatusCodes.Status404NotFound);

        // PUT /api/attributes/{id}/deactivate — деактивировать определение атрибута
        group.MapPut("/{id:guid}/deactivate", async (
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

        // PUT /api/attributes/values/{id}/deactivate — деактивировать значение атрибута
        group.MapPut("/values/{id:guid}/deactivate", async (
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

        return app;
    }
}

public record CreateAttributeDefinitionRequest(
    string Name,
    string DisplayName,
    Domain.Enums.AttributeDataType DataType,
    string? Unit);

public record UpdateAttributeDefinitionRequest(
    string? Name,
    string? DisplayName,
    string? Unit);
