using FamFit.Application.Commands.ActivityAttributes;
using FamFit.Application.Commands.ActivityTypes;
using FamFit.Application.Queries.ActivityAttributes;
using FamFit.Domain.Enums;
using FamFit.Infrastructure.Data;
using FamFit.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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
        UnitType UnitType,
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
                    at.UnitType,
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
                at.UnitType,
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
            CreateActivityTypeCommand command,
            IMediator mediator) =>
        {
            var result = await mediator.Send(command);

            return Results.Created(
                $"/api/activity-types/{result.Id}",
                new { result.Id, result.Name, result.UnitType });
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

        // === Attribute Definitions Endpoints ===

        // GET /api/activity-types/definitions — список определений
        group.MapGet("/definitions", async (IMediator mediator) =>
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

        return app;
    }
}
