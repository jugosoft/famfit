using FamFit.Application.Interfaces;
using FamFit.Domain.Entities;
using FamFit.Domain.Enums;
using MediatR;

namespace FamFit.Application.Commands.ActivityLogs;

/// <summary>
/// Команда логирования активности с автоматической отправкой пуш-уведомления.
/// Используется для демонстрации сквозной логики: лог + push.
/// </summary>
public record LogActivityWithNotificationCommand(
    Guid UserId,
    Guid ActivityTypeId,
    decimal Value,
    DateTime? Timestamp = null) : IRequest<ActivityLog>;

/// <summary>
/// Handler: логирует активность и отправляет push-уведомление пользователю.
/// Пример интеграции Use Case с IPushNotificationService.
/// </summary>
public class LogActivityWithNotificationHandler
    : IRequestHandler<LogActivityWithNotificationCommand, ActivityLog>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPushNotificationService _pushService;

    public LogActivityWithNotificationHandler(
        IApplicationDbContext dbContext,
        IPushNotificationService pushService)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _pushService = pushService ?? throw new ArgumentNullException(nameof(pushService));
    }

    public async Task<ActivityLog> Handle(
        LogActivityWithNotificationCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Сохраняем факт активности
        var activityType = await _dbContext.ActivityTypes
            .FirstOrDefaultAsync(at => at.Id == request.ActivityTypeId, cancellationToken);

        if (activityType is null || !activityType.IsActive)
            throw new InvalidOperationException("Invalid or deactivated activity type.");

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
            throw new InvalidOperationException("User not found.");

        var activityLog = ActivityLog.Create(
            request.UserId,
            request.ActivityTypeId,
            request.Value,
            request.Timestamp);

        _dbContext.ActivityLogs.Add(activityLog);

        // 2. Обновляем стрик
        user.StreakCount += 1;

        await _dbContext.SaveChangesAsync(cancellationToken);

        // 3. Отправляем push-уведомление с Notification Actions
        var actions = new[]
        {
            new PushAction(
                "done",
                "\u2705 Сделано, отъебись",
                string.Empty),
            new PushAction(
                "snooze",
                "\u23F1 Напомнить позже",
                string.Empty)
        };

        await _pushService.SendNotificationAsync(
            request.UserId,
            "🏋️ FamFit — Активность записана!",
            $"{activityType.Name}: {request.Value} {activityType.UnitType}",
            actions,
            cancellationToken);

        return activityLog;
    }
}
