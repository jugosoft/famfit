using FamFit.Application.Commands.ActivityLogs;
using FamFit.Application.Commands.ActivityTypes;
using FamFit.Infrastructure.Data;
using FamFit.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FamFit.WebApi.Endpoints;

/// <summary>
/// Маршруты API для ActivityLog.
/// </summary>
public static class ActivityLogEndpoints
{
    public static IEndpointRouteBuilder MapActivityLogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/activity-logs")
            .WithTags("ActivityLogs")
            .WithOpenApi();

        // POST /api/activity-logs — записать факт активности
        group.MapPost("/", async (
            LogActivityCommand command,
            IMediator mediator,
            HttpContext context) =>
        {
            // В реальном проекте: userId берётся из JWT-токена
            var userId = context.GetUserIdFromClaim();

            var result = await mediator.Send(new LogActivityCommand(
                userId,
                command.ActivityTypeId,
                command.Value,
                command.Timestamp));

            return Results.Ok(new
            {
                result.Id,
                result.UserId,
                result.ActivityTypeId,
                result.Value,
                result.Timestamp
            });
        })
        .WithSummary("Записать факт активности")
        .WithDescription("Сохраняет факт выполнения активности. userId берётся из JWT-токена.")
        .RequireAuthorization()
        .Produces<object>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError);

        // POST /api/activity-logs/with-notification — записать + отправить push
        group.MapPost("/with-notification", async (
            LogActivityWithNotificationCommand command,
            IMediator mediator,
            HttpContext context) =>
        {
            var userId = context.GetUserIdFromClaim();

            var result = await mediator.Send(new LogActivityWithNotificationCommand(
                userId,
                command.ActivityTypeId,
                command.Value,
                command.Timestamp));

            return Results.Ok(new
            {
                result.Id,
                result.UserId,
                result.ActivityTypeId,
                result.Value,
                result.Timestamp
            });
        })
        .WithSummary("Записать факт активности + отправить push-уведомление")
        .WithDescription("Сохраняет факт и отправляет интерактивное push-уведомление с кнопками действий.")
        .RequireAuthorization()
        .Produces<object>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError);

        // GET /api/activity-logs — история пользователя (последние N записей)
        group.MapGet("/", async (
            Guid userId,
            FamFitDbContext db,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var query = db.ActivityLogs
                .Where(al => al.UserId == userId)
                .Include(al => al.ActivityType)
                .OrderByDescending(al => al.Timestamp);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(al => new
                {
                    al.Id,
                    al.Value,
                    al.Timestamp,
                    ActivityType = new
                    {
                        al.ActivityType!.Id,
                        al.ActivityType.Name,
                        al.ActivityType.UnitType
                    }
                })
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return Results.Ok(new PagedResponse<object>(
                items,
                page,
                pageSize,
                totalCount,
                totalPages));
        })
        .WithSummary("Получить историю активности пользователя")
        .WithDescription("Возвращает пагинированный список записей об активности.")
        .RequireAuthorization()
        .Produces<PagedResponse<object>>()
        .Produces(StatusCodes.Status400BadRequest);

        return app;
    }
}
