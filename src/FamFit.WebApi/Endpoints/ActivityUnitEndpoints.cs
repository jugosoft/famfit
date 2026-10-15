using FamFit.Application.Commands.ActivityUnits;
using FamFit.Application.Queries.ActivityUnits;
using FamFit.Infrastructure.Data;
using FamFit.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace FamFit.WebApi.Endpoints;

/// <summary>
/// Маршруты API для ActivityUnit (единицы измерения активностей).
/// </summary>
public static class ActivityUnitEndpoints
{
    // DTO для единицы измерения
    public record ActivityUnitDto(
        Guid Id,
        string Name,
        string Symbol,
        string DataType,
        bool IsActive,
        int DisplayOrder);

    // DTO для запроса создания
    public record CreateActivityUnitRequest(
        string Name,
        string Symbol,
        string DataType,
        int DisplayOrder);

    // DTO для запроса обновления
    public record UpdateActivityUnitRequest(
        string? Name,
        string? Symbol,
        string? DataType,
        bool? IsActive,
        int? DisplayOrder);

    public static IEndpointRouteBuilder MapActivityUnitEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/activity-units")
            .WithTags("ActivityUnits")
            .WithOpenApi();

        // GET /api/activity-units — список всех единиц измерения
        group.MapGet("/", async (IMediator mediator) =>
        {
            var units = await mediator.Send(new GetActivityUnitsQuery());
            return Results.Ok(units.Select(u => new ActivityUnitDto(
                u.Id,
                u.Name,
                u.Symbol,
                u.DataType,
                u.IsActive,
                u.DisplayOrder)).ToList());
        })
        .WithSummary("Получить список всех единиц измерения")
        .WithDescription("Возвращает все единицы измерения активностей для использования в UI.")
        .Produces<List<ActivityUnitDto>>()
        .Produces(StatusCodes.Status500InternalServerError);

        // POST /api/activity-units — создать новую единицу измерения
        group.MapPost("/", async (
            CreateActivityUnitRequest request,
            IMediator mediator) =>
        {
            var result = await mediator.Send(new CreateActivityUnitCommand(
                request.Name,
                request.Symbol,
                request.DataType,
                request.DisplayOrder));

            return Results.Created(
                $"/api/activity-units/{result.Id}",
                new { result.Id, result.Name, result.Symbol });
        })
        .WithSummary("Создать новую единицу измерения")
        .WithDescription("Создаёт новую единицу измерения для типов активностей. Доступно только для Admin.")
        .RequireAuthorization("Admin")
        .Produces<Guid>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status500InternalServerError);

        // PUT /api/activity-units/{id} — обновить единицу измерения
        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateActivityUnitRequest request,
            IMediator mediator) =>
        {
            await mediator.Send(new UpdateActivityUnitCommand(
                id,
                request.Name,
                request.Symbol,
                request.DataType,
                request.IsActive,
                request.DisplayOrder));

            return Results.Ok(new { id, updated = true });
        })
        .WithSummary("Обновить единицу измерения")
        .WithDescription("Обновляет название, символ, тип данных или порядок отображения единицы измерения.")
        .RequireAuthorization("Admin")
        .Produces<object>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status500InternalServerError);

        // PUT /api/activity-units/{id}/deactivate — деактивировать единицу измерения
        group.MapPut("/{id:guid}/deactivate", async (
            Guid id,
            FamFitDbContext db) =>
        {
            var unit = await db.ActivityUnits
                .Include(u => u.ActivityTypes)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (unit is null)
                return Results.NotFound();

            // Проверяем, используется ли единица измерения в активных типах активностей
            var activeTypes = unit.ActivityTypes.Count(at => at.IsActive);
            if (activeTypes > 0)
            {
                return Results.Conflict(new
                {
                    error = "ActivityUnitInUse",
                    message = $"Невозможно деактивировать единицу измерения '{unit.Name}', так как она используется в {activeTypes} активных типах активностей.",
                    references = new[] { "типов активностей" }
                });
            }

            unit.Deactivate();
            await db.SaveChangesAsync();

            return Results.Ok(new { id, isActive = false });
        })
        .WithSummary("Деактивировать единицу измерения")
        .WithDescription("Делает единицу измерения неактивной. Доступно только для Admin. Возвращает 409, если единица используется.")
        .RequireAuthorization("Admin")
        .Produces<object>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status409Conflict);

        return app;
    }
}
